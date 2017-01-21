using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DHCPSharp.Common.Serialization;

namespace DHCPSharp.Common.Extensions
{
    public static class DhcpPacketExtensions
    {
        public static byte[] GetBytes(this DhcpPacket packet, IDhcpPacketSerializer serializer)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter br = new BinaryWriter(ms))
            {
                serializer.Serialize(br, packet);
                return ms.ToArray();
            }
        }
    }
}
