using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using DHCPSharp.DAL.Extensions;
using DHCPSharp.DAL.Models;
using SQLite;

namespace DHCPSharp.DAL.Repository
{
    public class LeaseRepo : SQLiteRepo<Lease>, ILeaseRepo
    {
        private readonly SQLiteAsyncConnection _db;
        public LeaseRepo(SQLiteAsyncConnection db) : base(db)
        {
            _db = db;
        }
        public async Task<Lease> GetByIpAddress(IPAddress ipAddress)
        {
            return await GetByIpAddress(ipAddress.ToString()).ConfigureAwait(false);
        }

        public async Task<Lease> GetByIpAddress(string ipAddress)
        {
            return await Get(x => x.IpAddress == ipAddress).ConfigureAwait(false);
        }

        public async Task<Lease> GetByPhysicalAddress(PhysicalAddress physicalAddress)
        {
            return await GetByPhysicalAddress(physicalAddress.ToString()).ConfigureAwait(false);
        }

        public async Task<Lease> GetByPhysicalAddress(string physicalAddress)
        {
            return await Get(x => x.PhysicalAddress == physicalAddress).ConfigureAwait(false);
        }

        
    }
}
