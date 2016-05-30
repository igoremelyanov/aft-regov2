using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events
{
    public abstract class PlayerStatusChanged : DomainEventBase
    {
        public Guid PlayerId { get; set; }

        protected PlayerStatusChanged() { }

        protected PlayerStatusChanged(Guid playerId)
        {
            PlayerId = playerId;
        }
    }

    public class PlayerActivated : PlayerStatusChanged
    {
        public PlayerActivated() { }

        public PlayerActivated(Guid playerId) : base(playerId) { }
    }

    public class PlayerDeactivated : PlayerStatusChanged
    {
        public PlayerDeactivated() { }

        public PlayerDeactivated(Guid playerId) : base(playerId) { }
    }
}
