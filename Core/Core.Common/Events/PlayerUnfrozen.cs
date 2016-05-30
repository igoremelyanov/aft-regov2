using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events
{
    public class PlayerUnfrozen : DomainEventBase
    {
        public Guid PlayerId { get; set; }

        public PlayerUnfrozen()
        { }

        public PlayerUnfrozen(Guid playerId)
        {
            PlayerId = playerId;
        }
    }
}
