using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DHCPSharp.Common
{
    public class DhcpData
    {
        public IPEndPoint Source { get; }
        public byte[] MessageBuffer { get; }
        public DhcpData(byte[] messageBuffer)
        {
            MessageBuffer = messageBuffer;
        }

        public DhcpData(IPEndPoint source, byte[] messageBuffer)
        {
            MessageBuffer = messageBuffer;
            Source = source;
        }
    }
}
