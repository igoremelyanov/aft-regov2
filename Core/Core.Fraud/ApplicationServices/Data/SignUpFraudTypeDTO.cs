using System;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices.Data
{
    public class SignUpFraudTypeDTO
    {
        public Guid Id { get; set; }
        public string FraudTypeName { get; set; }
        public string Remarks { get; set; }
        public SystemAction SystemAction { get; set; }
        public Guid[] RiskLevels { get; set; }
    }
}
