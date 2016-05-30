using System;
using System.Collections.Generic;

namespace FakeUGS.Core.Data
{
    public class VipLevel
    {
        public VipLevel()
        {
            VipLevelLimits = new List<VipLevelGameProviderBetLimit>();
        }

        public Guid Id { get; set; }
        public Guid BrandId { get; set; }

        public Brand                            Brand { get; set; }
        public ICollection<VipLevelGameProviderBetLimit>    VipLevelLimits { get; set; } 
    }
}