using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DHCPSharp.Common.Enums;
using DHCPSharp.Common.Extensions;

namespace DHCPSharp.Common
{
    public class DhcpOptionBuilder
    {
        private readonly List<byte> _bytes;

        public DhcpOptionBuilder()
        {
            _bytes = new List<byte>();
        }
        public byte[] GetBytes()
        {
            var byteArray = _bytes.ToArray();
            byteArray = AppendEndByte(byteArray);
            return byteArray;
        }
        public void AddOption(DhcpOptionCode opCode, int data, bool isReversed)
        {
            var bytes = BitConverter.GetBytes(data);

            AddOption(opCode, isReversed ? bytes.ReverseArray() : bytes);
        }
        public void AddOption(DhcpOptionCode opCode, DhcpMessageType messageType)
        {
            AddOption(opCode, (byte)messageType);
        }
        public void AddOption(DhcpOptionCode opCode, IPAddress data)
        {
            AddOption(opCode, data.GetAddressBytes());
        }
        public void AddOption(DhcpOptionCode opCode, byte data)
        {
            AddOption(opCode, new[] { data });
        }
        public void AddOption(DhcpOptionCode opCode, byte[] data)
        {
            _bytes.Add((byte)opCode);
            _bytes.Add((byte)data.Length);
            _bytes.AddRange(data);
        }

        private byte[] AppendEndByte(byte[] byteArray)
        {
            var newArray = new byte[byteArray.Length + 1];
            byteArray.CopyTo(newArray, 0);
            newArray[newArray.Length-1] = (byte)DhcpOptionCode.End;
            return newArray;
        }
    }
}
