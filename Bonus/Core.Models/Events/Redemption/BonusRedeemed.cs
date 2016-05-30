using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Bonus.Core.Models.Events.Redemption
{
    public class BonusRedeemed : DomainEventBase
    {
        public Guid BonusId { get; set; }
        public Guid PlayerId { get; set; }
        public string BonusName { get; set; }
        public string PlayerName { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }

        public bool IssuedByCs { get; set; }
    }
}