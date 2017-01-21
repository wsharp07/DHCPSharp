using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace DHCPSharp.DAL.Extensions
{
    public static class SQLiteConnectionExtensions
    {
        public static bool TableExists(this SQLiteConnection connection, string tableName)
        {
            var result = 
                connection.ExecuteScalar<long>(
                    @"SELECT COUNT(*) FROM sqlite_master WHERE name = @TableName", 
                    tableName);

            return result == 1;
        }
    }
}
