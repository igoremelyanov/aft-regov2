using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Fraud.Events
{
    public class SignUpFraudTypeUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string FraudTypeName { get; set; }
        public string Remarks { get; set; }
        public string SystemAction { get; set; }
        public string[] RiskLevels { get; set; }
    }
}
