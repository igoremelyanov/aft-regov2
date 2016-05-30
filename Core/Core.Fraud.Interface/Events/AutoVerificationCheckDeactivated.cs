using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Fraud
{
    public class AutoVerificationCheckDeactivated : DomainEventBase
    {
        public string DeactivatedBy { get; set; }
        public DateTimeOffset? DateDeactivated { get; set; }
    }
}
