using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DHCPSharp.Common
{
    public interface IDhcpConfiguration
    {
        IPAddress StartIpAddress { get; set; }
        IPAddress EndIpAddress { get; set; }
        IPAddress SubnetMask { get; set; }
        IPAddress Gateway { get; set; }
        IPAddress BindIp { get; set; }
        TimeSpan LeaseTime { get; set; }
        int LeaseTimeSeconds { get; }
    }
    public class DhcpConfiguration : IDhcpConfiguration
    {
        public IPAddress StartIpAddress { get; set; }
        public IPAddress EndIpAddress { get; set; }
        public IPAddress SubnetMask { get; set; }
        public IPAddress Gateway { get; set; }
        public IPAddress BindIp { get; set; }
        public TimeSpan LeaseTime { get; set; }
        public int LeaseTimeSeconds => (int)LeaseTime.TotalSeconds;
    }
}
