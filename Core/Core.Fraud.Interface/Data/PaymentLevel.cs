using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Fraud.Interface.Data
{
    public class PaymentLevel
    {
        public class PaymentLevelId
        {
            private readonly Guid _id;

            public PaymentLevelId(Guid id)
            {
                _id = id;
            }

            public static implicit operator Guid(PaymentLevelId id)
            {
                return id._id;
            }

            public static implicit operator PaymentLevelId(Guid id)
            {
                return new PaymentLevelId(id);
            }
        }

        public PaymentLevel()
        {
        }

        public Guid Id { get; set; }

        public Guid BrandId { get; set; }
        public virtual Brand Brand { get; set; }

        public virtual string CurrencyCode { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public PaymentLevelStatus Status { get; set; }
        public virtual ICollection<AutoVerificationCheckConfiguration> AutoVerificationCheckConfigurations { get; set; }
    }
}
