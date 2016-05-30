using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Common.Data.Player
{
    public class Brand
    {
        public Guid     Id { get; set; }
        public string   Name { get; set; }
        public Guid?    DefaultVipLevelId { get; set; }
        public Guid LicenseeId { get; set; }

        public VipLevel DefaultVipLevel { get; set; }

        public ICollection<VipLevel> VipLevels { get; set; }

        public string TimezoneId { get; set; }
    }
}