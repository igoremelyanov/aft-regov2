using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Fraud.Interface.Events
{
    public class RiskProfileCheckConfigUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset DateUpdated { get; set; }
    }
}
