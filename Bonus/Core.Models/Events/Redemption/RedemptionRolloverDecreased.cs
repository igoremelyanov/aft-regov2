using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Bonus.Core.Models.Events.Redemption
{
    public class RedemptionRolloverDecreased : DomainEventBase
    {
        public Guid GameId { get; set; }
        public decimal Decreasement { get; set; }
        public decimal RemainingRollover { get; set; }
        public string CurrencyCode { get; set; }
    }
}