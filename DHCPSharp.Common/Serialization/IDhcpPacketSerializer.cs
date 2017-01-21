using System.IO;

namespace DHCPSharp.Common.Serialization
{
    public interface IDhcpPacketSerializer
    {
        void Serialize(BinaryWriter writer, DhcpPacket packet);
        DhcpPacket Deserialize(byte[] data);
    }
}
