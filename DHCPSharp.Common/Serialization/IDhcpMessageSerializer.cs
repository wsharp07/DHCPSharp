namespace DHCPSharp.Common.Serialization
{
    public interface IDhcpMessageSerializer
    {
        DhcpMessage ToMessage(DhcpPacket packet);
        DhcpPacket ToPacket(DhcpMessage message, byte[] options);
    }
}
