using System;

using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Game.Interface.Events
{
    public class FundedOut : DomainEventBase
    {
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public Guid PlayerId { get; set; }
    }
}
