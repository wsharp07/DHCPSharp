using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using DHCPSharp.Common;
using DHCPSharp.Common.Enums;

namespace DHCPSharp.Common
{
    public class DhcpMessage
    {
        public DhcpOperation OperationCode { get; set; }
        public HardwareType HardwareType { get; set; }
        public ushort HardwareAddressLength { get; set; }
        public ushort Hops { get; set; }
        public int TransactionId { get; set; }
        public ushort SecondsElapsed { get; set; }
        public ushort Flags { get; set; }
        public IPAddress ClientIPAddress { get; set; }
        public IPAddress YourIPAddress { get; set; }
        public IPAddress ServerIPAddress { get; set; }
        public IPAddress GatewayIPAddress { get; set; }
        public PhysicalAddress ClientHardwareAddress { get; set; }
        public byte[] File { get; set; }
        public byte[] Cookie { get; set; }
        public Dictionary<DhcpOptionCode, byte[]> Options { get; set; }

        public DhcpMessageType DhcpMessageType
        {
            get
            {
                if (Options.Count == 0)
                {
                    return DhcpMessageType.Unknown;
                }
                if (Options.ContainsKey(DhcpOptionCode.DhcpMessageType) == false)
                {
                    return DhcpMessageType.Unknown;
                }

                var data = Options[DhcpOptionCode.DhcpMessageType][0];
                return (DhcpMessageType)data;    
            }
        }

        public string HostName
        {
            get
            {
                if (Options.Count == 0)
                {
                    return string.Empty;
                }
                if (Options.ContainsKey(DhcpOptionCode.Hostname) == false)
                {
                    return string.Empty;
                }

                var data = Options[DhcpOptionCode.Hostname];
                return Encoding.Default.GetString(data);
            }
        }

    }
}
