using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class DepositUnverified : DomainEventBase
    {
        public Guid DepositId { get; set; }
        public Guid PlayerId { get; set; }
        public OfflineDepositStatus Status { get; set; }
        public DateTimeOffset Unverified { get; set; }
        public string UnverifiedBy { get; set; }
        public string Remarks { get; set; }
        public UnverifyReasons UnverifyReason { get; set; }
        public DepositType DepositType { get; set; }
    }
}
