using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Bonus.Core.Models.Events.Redemption
{
    public class RedemptionRolloverCompleted : DomainEventBase
    {
        public decimal UnlockedAmount { get; set; }
        public decimal MainBalanceAdjustment { get; set; }
        public decimal BonusBalanceAdjustment { get; set; }
        public string CurrencyCode { get; set; }
    }
}