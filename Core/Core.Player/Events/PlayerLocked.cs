using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Player.Events
{
    public class PlayerLocked : DomainEventBase
    {
        public Guid PlayerId { get; set; }

        public PlayerLocked()
        { }

        public PlayerLocked(Guid playerId)
        {
            PlayerId = playerId;
        }
    }
}
