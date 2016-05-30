using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.Data
{
    public class SignUpFraudType
    {
        public Guid Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }
        public SystemAction SystemAction { get; set; }
        public virtual ICollection<RiskLevel> RiskLevels { get; set; }
        [Required]
        [MaxLength(200)]
        public string Remarks { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
    }
}
