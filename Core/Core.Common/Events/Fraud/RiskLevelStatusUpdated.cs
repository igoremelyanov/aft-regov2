using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Fraud
{
    public class RiskLevelStatusUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public RiskLevelStatus NewStatus { get; set; }

        public RiskLevelStatusUpdated()
        { }

        public RiskLevelStatusUpdated(Guid id, RiskLevelStatus newStatus)
        {
            Id = id;
            NewStatus = newStatus;
        }
    }
}
