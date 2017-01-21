using System;
using System.IO;
using DHCPSharp.Common.Extensions;

namespace DHCPSharp.Common.Serialization
{
    public class DhcpPacketSerializer : IDhcpPacketSerializer
    {
        private const int OPTION_OFFSET = 240;
        private const uint DHCP_OPTIONS_MAGIC_NUMBER = 1669485411;
        public void Serialize(BinaryWriter writer, DhcpPacket packet)
        {
            writer.Write(packet.Op);
            writer.Write(packet.HType);
            writer.Write(packet.HLen);
            writer.Write(packet.Hops);
            writer.Write(packet.XID);

            byte[] secsBytes = new byte[2];
            packet.Secs.CopyTo(secsBytes, 0);
            writer.Write(secsBytes);

            byte[] flagBytes = new byte[2];
            packet.Flags.CopyTo(flagBytes, 0);
            writer.Write(flagBytes);

            writer.Write(packet.CIAddr);
            writer.Write(packet.YIAddr);
            writer.Write(packet.SIAddr);
            writer.Write(packet.GIAddr);
            writer.Write(packet.CHAddr);

            byte[] snameBytes = new byte[64];
            writer.Write(snameBytes);

            byte[] fileBytes = new byte[128];
            writer.Write(fileBytes);

            writer.Write(GetDhcpMagicNumber());
            writer.Write(packet.Options);
        }

        private byte[] GetDhcpMagicNumber()
        {
            var bytes = BitConverter.GetBytes(DHCP_OPTIONS_MAGIC_NUMBER);
            return bytes.ReverseArray();
        }

        public DhcpPacket Deserialize(byte[] data)
        {
            int packetLength = data.Length;
            DhcpPacket result = new DhcpPacket();

            using (MemoryStream stm = new MemoryStream(data))
            using (BinaryReader rdr = new BinaryReader(stm))
            {
                result.Op = rdr.ReadByte();
                result.HType = rdr.ReadByte();
                result.HLen = rdr.ReadByte();
                result.Hops = rdr.ReadByte();
                result.XID = rdr.ReadBytes(4);
                result.Secs = rdr.ReadBytes(2);
                result.Flags = rdr.ReadBytes(2);
                result.CIAddr = rdr.ReadBytes(4);
                result.YIAddr = rdr.ReadBytes(4);
                result.SIAddr = rdr.ReadBytes(4);
                result.GIAddr = rdr.ReadBytes(4);
                result.CHAddr = rdr.ReadBytes(16);
                result.SName = rdr.ReadBytes(64);
                result.File = rdr.ReadBytes(128);
                result.Cookie = rdr.ReadBytes(4);
                result.Options = rdr.ReadBytes(packetLength - OPTION_OFFSET);
            }


            return result;
        }
    }
}
