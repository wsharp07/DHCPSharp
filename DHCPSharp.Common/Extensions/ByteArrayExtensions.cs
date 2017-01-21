using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPSharp.Common.Extensions
{
    public static class ByteArrayExtensions
    {
        public static byte[] ReverseArray(this byte[] me)
        {
            return me.Reverse().ToArray();
        }
    }
}
