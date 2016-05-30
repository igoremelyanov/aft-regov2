using System;

namespace AFT.RegoV2.Bonus.Core.Data
{
    public class LockUnlockParams
    {
        public Guid WalletTemplateId { get; set; }
        public decimal Amount { get; set; }
    }
}
