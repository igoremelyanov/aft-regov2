using System;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class DepositApproved : DomainEventBase
    {
        public Guid DepositId { get; set; }
        public Guid PlayerId { get; set; }
        public string ReferenceCode { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Fee { get; set; }
        public DateTimeOffset Approved { get; set; }
        public string ApprovedBy { get; set; }
        public string Remarks { get; set; }
        public decimal DepositWagering { get; set; }
        public DepositType DepositType { get; set; }
        public DateTimeOffset Deposited { get; set; }
    }
}
