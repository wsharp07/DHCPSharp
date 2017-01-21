using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPSharp.DAL
{
    public interface ITimeStampModel
    {
        DateTime InsertedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}
