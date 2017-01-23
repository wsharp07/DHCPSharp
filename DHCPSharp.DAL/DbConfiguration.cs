using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPSharp
{
    public interface IDbConfiguration
    {
        string ConnectionString { get; }
    }
    public class SQLiteConfiguration : IDbConfiguration
    {
        private string DATABASE_NAME = "DHCPSharpDb.sqlite";
        public string DbFile => $"{Environment.CurrentDirectory}\\{DATABASE_NAME}";
        public string ConnectionString => DbFile;
    }
}
