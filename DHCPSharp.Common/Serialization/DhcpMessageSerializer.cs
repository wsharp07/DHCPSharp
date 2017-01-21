using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using DHCPSharp.Common.Enums;
using DHCPSharp.Common.Extensions;

namespace DHCPSharp.Common.Serialization
{
    public class DhcpMessageSerializer : IDhcpMessageSerializer
    {
        public DhcpMessage ToMessage(DhcpPacket packet)
        {
            DhcpOptionParser parser = new DhcpOptionParser();
            DhcpMessage result = new DhcpMessage
            {
                OperationCode = (DhcpOperation)packet.Op,
                HardwareType = (HardwareType)packet.HType,
                HardwareAddressLength = packet.HLen,
                Hops = packet.Hops,
                TransactionId = BitConverter.ToInt32(packet.XID.Reverse().ToArray(), 0),
                SecondsElapsed = BitConverter.ToUInt16(packet.Secs, 0),
                Flags = BitConverter.ToUInt16(packet.Flags, 0),
                ClientIPAddress = new IPAddress(packet.CIAddr),
                YourIPAddress = new IPAddress(packet.YIAddr),
                ServerIPAddress = new IPAddress(packet.SIAddr),
                GatewayIPAddress = new IPAddress(packet.GIAddr),
                ClientHardwareAddress = new PhysicalAddress(packet.CHAddr.Take(packet.HLen).ToArray()),
                File = packet.File,
                Cookie = packet.Cookie,
                Options = parser.GetOptions(packet.Options)
            };

            return result;
        }

        public DhcpPacket ToPacket(DhcpMessage message, byte[] options)
        {
            DhcpPacket packet = new DhcpPacket
            {
                Op = (byte) message.OperationCode,
                HType = (byte) message.HardwareType,
                Hops = (byte) message.Hops,
                XID = BitConverter.GetBytes(message.TransactionId).ReverseArray(),
                Secs = BitConverter.GetBytes(message.SecondsElapsed),
                Flags = BitConverter.GetBytes(message.Flags),
                CIAddr = message.ClientIPAddress.GetAddressBytes(),
                YIAddr = message.YourIPAddress.GetAddressBytes(),
                SIAddr = message.ServerIPAddress.GetAddressBytes(),
                GIAddr = message.GatewayIPAddress.GetAddressBytes(),
                Options = options
            };

            packet = SetClientHardwareAddressFields(packet, message);
            return packet;
        }

        private DhcpPacket SetClientHardwareAddressFields(DhcpPacket packet, DhcpMessage message)
        {
            var addressBytes = message.ClientHardwareAddress.GetAddressBytes();
            var addressLength = addressBytes.Length;
            byte[] chAddressArray = new byte[16];

            addressBytes.CopyTo(chAddressArray, 0);

            packet.CHAddr = chAddressArray;
            packet.HLen = (byte)addressLength;

            return packet;
        }
    }
}
