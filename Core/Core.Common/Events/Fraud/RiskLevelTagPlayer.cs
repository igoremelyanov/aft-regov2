using System;

namespace AFT.RegoV2.Core.Common.Events.Fraud
{
    public class RiskLevelTagPlayer : Interfaces.DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid RiskLevelId { get; set; }
        public string Description { get; set; }

        public RiskLevelTagPlayer()
        { }

        public RiskLevelTagPlayer(Guid id, Guid playerId, Guid riskLevelId, string description)
        {
            Id = id;
            PlayerId = playerId;
            RiskLevelId = riskLevelId;
            Description = description;
        }
    }
}
