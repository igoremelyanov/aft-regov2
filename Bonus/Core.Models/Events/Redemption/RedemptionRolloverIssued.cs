using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Bonus.Core.Models.Events.Redemption
{
    public class RedemptionRolloverIssued : DomainEventBase
    {
        public decimal LockedAmount { get; set; }
        public decimal WageringRequrement { get; set; }
        public string CurrencyCode { get; set; }
    }
}