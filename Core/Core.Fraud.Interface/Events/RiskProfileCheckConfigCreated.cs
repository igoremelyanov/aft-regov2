using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Fraud.Interface.Events
{
    public class RiskProfileCheckConfigCreated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
    }
}
