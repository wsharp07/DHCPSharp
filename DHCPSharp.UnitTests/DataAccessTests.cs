using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DHCPSharp.DAL;
using DHCPSharp.DAL.Repository;
using DHCPSharp.DAL.Models;
using SQLite;
using Xunit;

namespace DHCPSharp.UnitTests
{
    public class DataAccessTests
    {
        readonly SQLiteAsyncConnection _conn;
        public DataAccessTests()
        {
            RemoveDb();
            _conn = new SQLiteAsyncConnection(TestHelpers.GetRandomDb());
            CreateTables();
        }

        [Fact]
        public async void InsertLease_Expect_RetrieveByIpAddress()
        {
            var repo = new LeaseRepo(_conn);
            var ipAddress = "10.10.10.10";

            var lease = new Lease
            {
                HostName = "myserver.local",
                IpAddress = ipAddress,
                PhysicalAddress = "00:00:00:00:00:00",
                Expiration = DateTime.Now.AddSeconds(86400)
            };

            var id = await repo.Insert(lease);
            var entity = await repo.GetByIpAddress(ipAddress);

            Assert.Equal(id, entity.Id);
        }

        [Fact]
        public async void InsertLease_Expect_RetrieveByPhysicalAddress()
        {
            var repo = new LeaseRepo(_conn);
            var ipAddress = "10.10.10.10";

            var lease = new Lease
            {
                HostName = "myserver.local",
                IpAddress = ipAddress,
                PhysicalAddress = "00:00:00:00:00:00",
                Expiration = DateTime.Now.AddSeconds(86400)
            };

            var id = await repo.Insert(lease);
            var entity = await repo.GetByPhysicalAddress(lease.PhysicalAddress);

            Assert.Equal(lease.PhysicalAddress, entity.PhysicalAddress);
        }

        [Fact]
        public async void UpdateLease_Expect_UpdatedValueInDb()
        {
            var newHostName = "myhost22.local";
            var newIpAddress = "192.168.8.11";
            var newPhysicalAddress = "0012345678";

            var repo = new LeaseRepo(_conn);
            var lease = ModelFakes.GetFakeLease();
            var id = await repo.Insert(lease);

            var entity = await repo.Get(id);
            entity.HostName = newHostName;
            entity.IpAddress = newIpAddress;
            entity.PhysicalAddress = newPhysicalAddress;

            await repo.Update(entity);

            entity = await repo.Get(id);

            Assert.Equal(newIpAddress, entity.IpAddress);
            Assert.Equal(newHostName, entity.HostName);
            Assert.Equal(newPhysicalAddress, entity.PhysicalAddress);
        }

        private void RemoveDb()
        {
            File.Delete(DbHelpers.DbFile);
        }

        private async void CreateTables()
        {
            await _conn.CreateTableAsync<Lease>();
        }
    }
}
