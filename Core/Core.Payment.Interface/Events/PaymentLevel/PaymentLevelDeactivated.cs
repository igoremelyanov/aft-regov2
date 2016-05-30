using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Events
{
    public class PaymentLevelDeactivated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string DeactivatedBy { get; set; }
        public DateTimeOffset DeactivatedDate { get; set; }
        public string Remarks { get; set; }
    }
}
