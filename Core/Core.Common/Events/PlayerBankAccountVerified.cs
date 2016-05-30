using System;

namespace AFT.RegoV2.Core.Common.Events
{
    public class PlayerBankAccountVerified : PlayerBankAccountEvent
    {
        public PlayerBankAccountVerified() { }

        public PlayerBankAccountVerified(
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
