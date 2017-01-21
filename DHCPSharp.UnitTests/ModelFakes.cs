using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DHCPSharp.DAL.Models;

namespace DHCPSharp.UnitTests
{
    public static class ModelFakes
    {
        public static Lease GetFakeLease()
        {
            return new Lease
            {
                Id = 1,
                HostName = "myserver.local",
                InsertedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IpAddress = "10.10.10.10",
                Expiration = DateTime.Now.AddSeconds(86400),
                PhysicalAddress = "00:00:00:00:00:00"
            };
        }
        public static Lease GetFakeLease(string ipAddress)
        {
            var result = GetFakeLease();
            result.IpAddress = ipAddress;
            return result;
        }
    }
}
