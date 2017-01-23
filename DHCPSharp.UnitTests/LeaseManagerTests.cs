using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using DHCPSharp.Common.Extensions;
using DHCPSharp.DAL;
using DHCPSharp.DAL.Models;
using DHCPSharp.DAL.Repository;
using SQLite;
using Xunit;

namespace DHCPSharp.UnitTests
{
    public class LeaseManagerTests : IDisposable
    {
        readonly IDbConfiguration DbConfig;
        public LeaseManagerTests()
        {
            DbConfig = new SQLiteTestConfig();

            CreateTables();
        }

        public void Dispose()
        {
            RemoveDb(DbConfig);
        }

        [Fact]
        public async void InsertLease_Expect_NextLeaseToBeNextIpAddress()
        {
            var config = DhcpFakes.FakeDhcpConfiguration();
            var nextIp = config.StartIpAddress.ToNextIpAddress();
            var leaseRepo = new LeaseRepo(DbConfig);
            var leaseManager = new LeaseManager(DhcpFakes.FakeDhcpConfiguration(), leaseRepo);

            var lease = ModelFakes.GetFakeLease(config.StartIpAddress.ToString());
            await leaseRepo.Insert(lease).ConfigureAwait(false);

            var nextLease = await leaseManager.GetNextLease();

            Assert.Equal(nextIp, nextLease);
        }

        [Fact]
        public async void NoLeases_Expect_NextLeaseToBeStartIpAddress()
        {
            var config = DhcpFakes.FakeDhcpConfiguration();
            var leaseRepo = new LeaseRepo(DbConfig);
            var leaseManager = new LeaseManager(config, leaseRepo);

            var nextLease = await leaseManager.GetNextLease().ConfigureAwait(false);

            Assert.Equal(config.StartIpAddress, nextLease);
        }

        [Fact]
        public async void AddLease_Expect_LeaseInDb()
        {
            var leaseRepo = new LeaseRepo(DbConfig);
            var leaseManager = new LeaseManager(DhcpFakes.FakeDhcpConfiguration(), leaseRepo);

            var ipAddress = IPAddress.Parse("10.10.10.10");
            var hostName = "myserver.local";
            var physicalAddress = PhysicalAddress.Parse("000000000000");

            await leaseManager.AddLease(ipAddress, physicalAddress, hostName).ConfigureAwait(false);

            var entity = await leaseRepo.GetByIpAddress(ipAddress).ConfigureAwait(false);

            Assert.Equal(ipAddress.ToString(), entity.IpAddress);
            Assert.Equal(physicalAddress.ToString(), entity.PhysicalAddress);
            Assert.Equal(hostName, entity.HostName);
        }

        [Fact]
        public async void RemoveLease_Expect_NoLeaseInDb()
        {
            var leaseRepo = new LeaseRepo(DbConfig);
            var leaseManager = new LeaseManager(DhcpFakes.FakeDhcpConfiguration(), leaseRepo);

            var ipAddress = IPAddress.Parse("10.10.10.10");
            var hostName = "myserver.local";
            var physicalAddress = PhysicalAddress.Parse("000000000000");

            await leaseManager.AddLease(ipAddress, physicalAddress, hostName).ConfigureAwait(false);
            await leaseManager.RemoveLease(ipAddress).ConfigureAwait(false);

            var entity = await leaseRepo.GetByIpAddress(ipAddress).ConfigureAwait(false);

            Assert.Null(entity);
        }

        [Fact]
        public async void KeepLeaseWithMatch_Expect_RequestAccepted()
        {
            var leaseRepo = new LeaseRepo(DbConfig);
            var leaseManager = new LeaseManager(DhcpFakes.FakeDhcpConfiguration(), leaseRepo);
            
            var ipAddress = IPAddress.Parse("10.10.10.10");
            var hostName = "myserver.local";
            var physicalAddress = PhysicalAddress.Parse("000000000000");

            await leaseManager.AddLease(ipAddress, physicalAddress, hostName).ConfigureAwait(false);

            var keepLeaseResponse = await leaseManager.KeepLeaseRequest(ipAddress, physicalAddress, hostName).ConfigureAwait(false);

            Assert.True(keepLeaseResponse);
        }

        [Fact]
        public async void KeepLeaseWithNoMatch_Expect_RequestAccepted()
        {
            var leaseRepo = new LeaseRepo(DbConfig);
            var leaseManager = new LeaseManager(DhcpFakes.FakeDhcpConfiguration(), leaseRepo);

            var ipAddress = IPAddress.Parse("10.10.10.10");
            var hostName = "myserver.local";
            var physicalAddress = PhysicalAddress.Parse("000000000000");

            bool keepLeaseResponse = await leaseManager.KeepLeaseRequest(ipAddress, physicalAddress, hostName).ConfigureAwait(false);

            Assert.True(keepLeaseResponse);
        }

        [Fact]
        public async void KeepLeaseWithMatch_WrongMAC_Expect_RequestRejected()
        {
            var leaseRepo = new LeaseRepo(DbConfig);
            var leaseManager = new LeaseManager(DhcpFakes.FakeDhcpConfiguration(), leaseRepo);

            var ipAddress = IPAddress.Parse("10.10.10.10");
            var hostName = "myserver.local";
            var physicalAddress = PhysicalAddress.Parse("000000000000");
            var physicalAddress2 = PhysicalAddress.Parse("999999999999");

            await leaseManager.AddLease(ipAddress, physicalAddress, hostName).ConfigureAwait(false);

            var keepLeaseResponse = await leaseManager.KeepLeaseRequest(ipAddress, physicalAddress2, hostName).ConfigureAwait(false);

            Assert.False(keepLeaseResponse);
        }

        [Fact]
        public async void CleanupExpiredLease_Expect_NoExpiredLeases()
        {
            var leaseRepo = new LeaseRepo(DbConfig);
            var leaseManager = new LeaseManager(DhcpFakes.FakeDhcpConfiguration(), leaseRepo);

            var expiredLease = ModelFakes.GetFakeLease();
            expiredLease.Expiration = DateTime.UtcNow.AddMinutes(-2);

            var validLease = ModelFakes.GetFakeLease();
            validLease.IpAddress = "10.10.10.11";
            validLease.PhysicalAddress = "332211556677";
            validLease.HostName = "boot22.local";

            await leaseRepo.Insert(expiredLease).ConfigureAwait(false);
            await leaseRepo.Insert(validLease).ConfigureAwait(false);

            await leaseManager.CleanExpiredLeases();

            var expiredEntity = await leaseRepo.GetByIpAddress(expiredLease.IpAddress).ConfigureAwait(false);
            var validEntity = await leaseRepo.GetByIpAddress(validLease.IpAddress).ConfigureAwait(false);

            Assert.Null(expiredEntity);
            Assert.NotNull(validEntity);
        }

        private void RemoveDb(IDbConfiguration config)
        {
            //File.Delete(dbFilePath);
        }

        private void CreateTables()
        {
            var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.CreateTable<Lease>();
        }
    }
}
