using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPSharp.UnitTests
{
    public class SQLiteTestConfig : IDbConfiguration
    {
        public SQLiteTestConfig()
        {
            ConnectionString = TestHelpers.GetRandomDb();
        }
        public string ConnectionString { get; }

    }
}
