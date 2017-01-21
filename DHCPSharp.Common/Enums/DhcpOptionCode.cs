namespace DHCPSharp.Common.Enums
{
    public enum DhcpOptionCode : byte
    {
        Pad = 0x00,
        SubnetMask = 0x01,
        TimeOffset = 0x02,
        Router = 0x03,
        TimeServer = 0x04,
        NameServer = 0x05,
        DomainNameServer = 0x06,
        LogServer = 0x07,
        CookieServer = 0x08,
        LprServer = 0x09,
        ImpressServer = 0x0A,
        ResourceLocationServer = 0x0B,
        Hostname = 0x0C,
        BootFileSize = 0x0D,
        MeritDumpFile = 0x0E,
        DomainNameSuffix = 0x0F,
        RequestedIpAddress = 0x32,
        AddressTime = 0x33,
        DhcpMessageType = 0x35,
        DhcpAddress = 0x36,
        ParameterList = 0x37,
        DhcpMessage = 0x38,
        DhcpMaxMessageSize = 0x39,
        ClassId = 0x3C,
        ClientId = 0x3D,
        AutoConfig = 0x74,
        End = 0xFF
    }
}
