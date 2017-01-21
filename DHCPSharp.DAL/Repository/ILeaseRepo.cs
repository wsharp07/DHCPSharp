using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using DHCPSharp.DAL.Models;

namespace DHCPSharp.DAL.Repository
{
    public interface ILeaseRepo : IRepo<Lease>
    {
        Task<Lease> GetByIpAddress(IPAddress ipAddress);
        Task<Lease> GetByIpAddress(string ipAddress);
        Task<Lease> GetByPhysicalAddress(PhysicalAddress physicalAddress);
        Task<Lease> GetByPhysicalAddress(string physicalAddress);
    }
}
