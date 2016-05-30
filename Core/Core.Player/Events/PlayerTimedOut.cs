using System;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Player.Events
{
    public class PlayerTimedOut : DomainEventBase
    {
        public Guid PlayerId { get; set; }
        public string TimeOut { get; set; }
        public DateTimeOffset TimeOutEndDate { get; set; }

        public PlayerTimedOut()
        { }

        public PlayerTimedOut(Guid playerId, TimeOut timeOut,DateTimeOffset timeoutEndDate)
        {
            PlayerId = playerId;
            TimeOut = timeOut.ToString();
            TimeOutEndDate = timeoutEndDate;
        }
    }
}
