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
        readonly SQLiteAsyncConnection _conn;
        readonly string DbFilePath;
        public LeaseManagerTests()
        {
            DbFilePath = TestHelpers.GetRandomDb();
            _conn = new SQLiteAsyncConnection(DbFilePath);

            CreateTables();
        }

        public void Dispose()
        {
            RemoveDb(DbFilePath);
        }

        [Fact]
        public async void InsertLease_Expect_NextLeaseToBeNextIpAddress()
        {
            var config = DhcpFakes.FakeDhcpConfiguration();
            var nextIp = config.StartIpAddress.ToNextIpAddress();
            var leaseRepo = new LeaseRepo(_conn);
            var leaseManager = new LeaseManager(DhcpFakes.FakeDhcpConfiguration(), leaseRepo);

            var lease = ModelFakes.GetFakeLease(config.StartIpAddress.ToString());
            await leaseRepo.Insert(lease);

            var nextLease = await leaseManager.GetNextLease();

            Assert.Equal(nextIp, nextLease);
        }

        [Fact]
        public async void NoLeases_Expect_NextLeaseToBeStartIpAddress()
        {
            var config = DhcpFakes.FakeDhcpConfiguration();
            var leaseRepo = new LeaseRepo(_conn);
            var leaseManager = new LeaseManager(config, leaseRepo);

            var nextLease = await leaseManager.GetNextLease();

            Assert.Equal(config.StartIpAddress, nextLease);
        }

        [Fact]
        public async void AddLease_Expect_LeaseInDb()
        {
            var leaseRepo = new LeaseRepo(_conn);
            var leaseManager = new LeaseManager(DhcpFakes.FakeDhcpConfiguration(), leaseRepo);

            var ipAddress = IPAddress.Parse("10.10.10.10");
            var hostName = "myserver.local";
            var physicalAddress = PhysicalAddress.Parse("000000000000");

            await leaseManager.AddLease(ipAddress, physicalAddress, hostName);

            var entity = await leaseRepo.GetByIpAddress(ipAddress);

            Assert.Equal(ipAddress.ToString(), entity.IpAddress);
            Assert.Equal(physicalAddress.ToString(), entity.PhysicalAddress);
            Assert.Equal(hostName, entity.HostName);
        }

        [Fact]
        public async void RemoveLease_Expect_NoLeaseInDb()
        {
            var leaseRepo = new LeaseRepo(_conn);
            var leaseManager = new LeaseManager(DhcpFakes.FakeDhcpConfiguration(), leaseRepo);

            var ipAddress = IPAddress.Parse("10.10.10.10");
            var hostName = "myserver.local";
            var physicalAddress = PhysicalAddress.Parse("000000000000");

            await leaseManager.AddLease(ipAddress, physicalAddress, hostName);
            await leaseManager.RemoveLease(ipAddress);

            var entity = await leaseRepo.GetByIpAddress(ipAddress);

            Assert.Null(entity);
        }

        [Fact]
        public async void KeepLeaseWithMatch_Expect_RequestAccepted()
        {
            var leaseRepo = new LeaseRepo(_conn);
            var leaseManager = new LeaseManager(DhcpFakes.FakeDhcpConfiguration(), leaseRepo);
            
            var ipAddress = IPAddress.Parse("10.10.10.10");
            var hostName = "myserver.local";
            var physicalAddress = PhysicalAddress.Parse("000000000000");

            await leaseManager.AddLease(ipAddress, physicalAddress, hostName);

            var keepLeaseResponse = await leaseManager.KeepLeaseRequest(ipAddress, physicalAddress, hostName);

            Assert.True(keepLeaseResponse);
        }

        [Fact]
        public async void KeepLeaseWithNoMatch_Expect_RequestAccepted()
        {
            var leaseRepo = new LeaseRepo(_conn);
            var leaseManager = new LeaseManager(DhcpFakes.FakeDhcpConfiguration(), leaseRepo);

            var ipAddress = IPAddress.Parse("10.10.10.10");
            var hostName = "myserver.local";
            var physicalAddress = PhysicalAddress.Parse("000000000000");

            bool keepLeaseResponse = await leaseManager.KeepLeaseRequest(ipAddress, physicalAddress, hostName);

            Assert.True(keepLeaseResponse);
        }

        [Fact]
        public async void KeepLeaseWithMatch_WrongMAC_Expect_RequestRejected()
        {
            var leaseRepo = new LeaseRepo(_conn);
            var leaseManager = new LeaseManager(DhcpFakes.FakeDhcpConfiguration(), leaseRepo);

            var ipAddress = IPAddress.Parse("10.10.10.10");
            var hostName = "myserver.local";
            var physicalAddress = PhysicalAddress.Parse("000000000000");
            var physicalAddress2 = PhysicalAddress.Parse("999999999999");

            await leaseManager.AddLease(ipAddress, physicalAddress, hostName);

            var keepLeaseResponse = await leaseManager.KeepLeaseRequest(ipAddress, physicalAddress2, hostName);

            Assert.False(keepLeaseResponse);
        }

        private void RemoveDb(string dbFilePath)
        {
            //File.Delete(dbFilePath);
        }

        private async void CreateTables()
        {
            await _conn.CreateTableAsync<Lease>();
        }
    }
}
