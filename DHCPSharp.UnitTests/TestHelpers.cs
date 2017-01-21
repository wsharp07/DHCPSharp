using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPSharp.UnitTests
{
    public static class TestHelpers
    {
        public static string GetRandomDb()
        {
            return Environment.CurrentDirectory + "//DHCPSharpDb_Test_" + Guid.NewGuid() + ".sqlite";
        }
    }
}
