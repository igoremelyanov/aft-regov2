using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Security.Data
{
    public class Brand
    {
        public Guid Id { get; set; }
        public Guid LicenseeId { get; set; }
        public string TimeZoneId { get; set; }
    }
}
