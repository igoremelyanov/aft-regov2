using System;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    public class DepositBonusApplication
    {
        public Guid PlayerId;
        public Guid? BonusId;
        public string BonusCode;
        public Guid DepositId;
        public decimal Amount;
    }
}
