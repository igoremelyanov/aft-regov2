using System;

namespace AFT.RegoV2.Core.Common.Events.Fraud
{
    public class RiskLevelUntagPlayer : Interfaces.DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid RiskLevelId { get; set; }
        public string Description { get; set; }

        public RiskLevelUntagPlayer()
        { }

        public RiskLevelUntagPlayer(Guid id, Guid playerId, Guid riskLevelId, string remarks)
        {
            Id = id;
            PlayerId = playerId;
            RiskLevelId = riskLevelId;
            Description = remarks;
        }
    }
}
