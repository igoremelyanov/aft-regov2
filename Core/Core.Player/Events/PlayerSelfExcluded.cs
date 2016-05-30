using System;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Player.Events
{
    public class PlayerSelfExcluded : DomainEventBase
    {
        public Guid PlayerId { get; set; }
        public string SelfExclusionType { get; set; }
        public DateTimeOffset SelfExclusionEndDate { get; set; }
        public PlayerSelfExcluded()
        { }

        public PlayerSelfExcluded(Guid playerId, SelfExclusion selfExclusion,DateTimeOffset selfExclusionEndDate)
        {
            PlayerId = playerId;
            SelfExclusionType = selfExclusion.ToString();
            SelfExclusionEndDate = selfExclusionEndDate;
        }
    }
}
