using System;

namespace AFT.RegoV2.Core.Common.Events
{
    public class PlayerBankAccountRejected : PlayerBankAccountEvent
    {
        public PlayerBankAccountRejected() { }

        public PlayerBankAccountRejected(
            Guid playerId,
            Guid playerBankAccountId,
            string accountNumber,
            string remarks)
            : base(playerId, playerBankAccountId, accountNumber)
        {
            Remarks = remarks;
        }

        public string Remarks { get; set; }
    }
}