using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Brand
{
    public class VipLevelUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Rank { get; set; }
        public string Description { get; set; }
        public string ColorCode { get; set; }
        public string Remark { get; set; }
        public ICollection<VipLevelLimitData> VipLevelLimits { get; set; } 
    }
}