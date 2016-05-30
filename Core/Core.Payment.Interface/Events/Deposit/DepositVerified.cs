using System;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class DepositVerified : DomainEventBase
    {
        public Guid DepositId { get; set; }
        public Guid PlayerId { get; set; }
        public string ReferenceCode { get; set; }
        public DateTimeOffset Verified { get; set; }
        public string VerifiedBy { get; set; }
        public string Remarks { get; set; }
        public DepositType DepositType { get; set; }
        public Guid? BankAccountId { get; set; }
    }
}
