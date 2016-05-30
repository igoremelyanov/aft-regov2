using System;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Bonus.Core.Models.Events.Management
{
    public class BonusCreated : DomainEventBase
    {
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public BonusType BonusType { get; set; }
        public Guid BrandId { get; set; }
    }
}
