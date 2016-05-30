using System;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    public class FundInBonusApplication
    {
        public Guid PlayerId;
        public Guid? BonusId;
        public string BonusCode;
        public Guid DestinationWalletTemplateId;
        public decimal Amount;
    }
}
