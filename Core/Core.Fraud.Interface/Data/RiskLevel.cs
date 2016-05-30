using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.Data
{
    public class RiskLevelId
    {
        private readonly Guid _id;

        public RiskLevelId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(RiskLevelId id)
        {
            return id._id;
        }

        public static implicit operator RiskLevelId(Guid id)
        {
            return new RiskLevelId(id);
        }
    }

    public class RiskLevel
    {
        public Guid Id { get; set; }

        //public Guid LicenseeId { get; set; }
        public Guid BrandId { get; set; }
        public virtual Brand Brand { get; set; }

        public int Level { get; set; }
        public string Name { get; set; }

        public RiskLevelStatus Status { get; set; }
        public string Description { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
        public virtual ICollection<AutoVerificationCheckConfiguration> AutoVerificationCheckConfigurations { get; set; }
        public virtual ICollection<SignUpFraudType> SignUpFraudTypes { get; set; }
    }
}
