using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Bonus.Core.Models.Events.Redemption
{
    public class RedemptionClaimed : DomainEventBase
    {
        public Guid PlayerId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
    }
}