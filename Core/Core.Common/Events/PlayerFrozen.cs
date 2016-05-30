using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events
{
    public class PlayerFrozen : DomainEventBase
    {
        public Guid PlayerId { get; set; }

        public PlayerFrozen()
        { }

        public PlayerFrozen(Guid playerId)
        {
            PlayerId = playerId;
        }
    }
}
