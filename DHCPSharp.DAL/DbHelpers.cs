using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DHCPSharp.DAL.Extensions;

namespace DHCPSharp.DAL
{
    public static class DbHelpers
    {
        private static string DATABASE_NAME = "DHCPSharpDb.sqlite";
        public static string DbFile => $"{Environment.CurrentDirectory}\\{DATABASE_NAME}";
        public static bool DatabaseExists()
        {
            return File.Exists(DbFile);
        }
    }
}
