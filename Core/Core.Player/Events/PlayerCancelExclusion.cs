using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Player.Events
{
    public class PlayerCancelExclusion : DomainEventBase
    {
        public Guid PlayerId { get; set; }

        public PlayerCancelExclusion()
        { }

        public PlayerCancelExclusion(Guid playerId)
        {
            PlayerId = playerId;
        }
    }
}
