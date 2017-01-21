using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DHCPSharp.Common.Extensions
{
    public static class NetworkInterfaceTypeExtensions
    {
        public static bool IsUsableNetworkInterface(this NetworkInterfaceType me, OperationalStatus status)
        {
            if ((me == NetworkInterfaceType.Ethernet || me == NetworkInterfaceType.GigabitEthernet || me == NetworkInterfaceType.Wireless80211) && status == OperationalStatus.Up)
            {
                return true;
            }

            return false;
        }
    }
}
