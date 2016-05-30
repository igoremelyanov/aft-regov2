using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Bonus.Core.Models.Events.Redemption
{
    public class RedemptionCanceled : DomainEventBase
    {
        public decimal MainBalanceAdjustment { get; set; }
        public decimal BonusBalanceAdjustment { get; set; }
        public decimal NonTransferableAdjustment { get; set; }
        public decimal UnlockedAmount { get; set; }
        public string CurrencyCode { get; set; }
    }
}