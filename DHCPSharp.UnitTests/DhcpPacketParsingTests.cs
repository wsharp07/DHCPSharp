using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using DHCPSharp.Common;
using DHCPSharp.Common;
using DHCPSharp.Common.Enums;
using DHCPSharp.Common.Serialization;
using Xunit;

namespace DHCPSharp.UnitTests
{
   
    public class DhcpPacketParsingTests
    {
        private readonly IDhcpMessageSerializer _messagerSerializer = new DhcpMessageSerializer();

        [Fact]
        public void DiscoveryCIAddr_Expect_CorrectClientIP()
        {
            DhcpPacket discoveryPacket = DhcpFakes.FakeDhcpDiscoverPacket();
            discoveryPacket.CIAddr = new byte[] { 0, 0, 0, 0 };
            var sut = _messagerSerializer.ToMessage(discoveryPacket);
            Assert.Equal("0.0.0.0", sut.ClientIPAddress.ToString());
        }

        [Fact]
        public void DiscoveryOptions_Expect_CorrectHostName()
        {
            DhcpPacket discoveryPacket = DhcpFakes.FakeDhcpDiscoverPacket();
            var sut = _messagerSerializer.ToMessage(discoveryPacket);
            Assert.Equal("SpeedwayR-11-7A-3C", sut.HostName);
        }

        [Fact]
        public void DiscoveryOptions_Expect_MessageTypeOfDiscover()
        {
            DhcpPacket discoveryPacket = DhcpFakes.FakeDhcpDiscoverPacket();
            var sut = _messagerSerializer.ToMessage(discoveryPacket);
            Assert.Equal(DhcpMessageType.Discover, sut.DhcpMessageType);
        }

        [Fact]
        public void DiscoveryOptions_Expect_CorrectClientHAddr()
        {
            DhcpPacket discoveryPacket = DhcpFakes.FakeDhcpDiscoverPacket();
            var sut = _messagerSerializer.ToMessage(discoveryPacket);
            PhysicalAddress expeced = new PhysicalAddress(new byte[] { 0, 22, 37, 17, 122, 60 });
            Assert.Equal(expeced, sut.ClientHardwareAddress);
        }

        [Fact]
        public void DiscoveryOptions_Expect_CorrectXID()
        {
            DhcpPacket discoveryPacket = DhcpFakes.FakeDhcpDiscoverPacket();
            discoveryPacket.XID = new byte[] { 0x19, 0xe1, 0x79, 0x50 };
            var sut = _messagerSerializer.ToMessage(discoveryPacket);
            var expectedHex = "19e17950";
            Assert.Equal(expectedHex, sut.TransactionId.ToString("x4"));
        }
    }
}
