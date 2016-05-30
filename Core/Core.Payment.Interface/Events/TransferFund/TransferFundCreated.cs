using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class TransferFundCreated : DomainEventBase
    {
        public Guid PlayerId { get; set; }
        public string TransactionNumber { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
        public string BonusCode { get; set; }
        public Guid DestinationWalletStructureId { get; set; }
        public TransferFundType Type { get; set; }
        public TransferFundStatus Status { get; set; }
        public string Description { get; set; }
    }
}