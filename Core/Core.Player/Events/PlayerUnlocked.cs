using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Player.Events
{
    public class PlayerUnlocked : DomainEventBase
    {
        public Guid PlayerId { get; set; }

        public PlayerUnlocked()
        { }

        public PlayerUnlocked(Guid playerId)
        {
            PlayerId = playerId;
        }
    }
}
