using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace DHCPSharp.DAL.Models
{
    public class Lease : ITimeStampModel
    {
        public Lease()
        {
            InsertedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
        [Unique]
        public string PhysicalAddress { get; set; }
        public string IpAddress { get; set; }
        public string HostName { get; set; }
        public DateTime Expiration { get; set; }
        public DateTime InsertedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
