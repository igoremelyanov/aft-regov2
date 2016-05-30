using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Fraud.Interface.Events
{
    public class AutoVerificationCheckActivated : DomainEventBase
    {
        public string ActivatedBy { get; set; }
        public DateTimeOffset? DateActivated { get; set; }
    }
}
