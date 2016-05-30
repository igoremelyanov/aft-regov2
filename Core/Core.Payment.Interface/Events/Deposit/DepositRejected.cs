using System;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class DepositRejected : DomainEventBase
    {
        public Guid DepositId { get; set; }
        public Guid PlayerId { get; set; }
        public string ReferenceCode { get; set; }
        public DateTimeOffset Rejected { get; set; }
        public string RejectedBy { get; set; }
        public string Remarks { get; set; }        
        public DepositType DepositType { get; set; }
    }
}
