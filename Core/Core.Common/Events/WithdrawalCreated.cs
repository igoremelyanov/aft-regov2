using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class WithdrawalCreated : DomainEventBase
    {
        public Guid PlayerId { get; set; }
        public string TransactionNumber { get; set; }
        public decimal Amount { get; set; }
    }
}