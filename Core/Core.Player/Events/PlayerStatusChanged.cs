using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Player.Events
{
    public class PlayerStatusChanged : DomainEventBase
    {
        #region Properties

        public Guid PlayerId { get; set; }
        public AccountStatus AccountStatus { get; set; }

        #endregion

        #region Constructors

        public PlayerStatusChanged()
        {
        } // default constructor is required for publishing event to MQ

        public PlayerStatusChanged(Guid playerId, AccountStatus accountStatus)
        {
            PlayerId = playerId;
            AccountStatus = accountStatus;
        }

        #endregion
    }
}