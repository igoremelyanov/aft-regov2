using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;

namespace AFT.RegoV2.Core.Game.Events
{
    public class BetLimitGroupUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int ExternalId { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public BetLimitGroupUpdated()
        {
            
        }

        public BetLimitGroupUpdated(BetLimitGroup betLimitGroup)
        {
            Id = betLimitGroup.Id;
            Name = betLimitGroup.Name;
            ExternalId = betLimitGroup.ExternalId;
            UpdatedDate = betLimitGroup.UpdatedDate.GetValueOrDefault();
            UpdatedBy = betLimitGroup.UpdatedBy;
        }
    }
}
