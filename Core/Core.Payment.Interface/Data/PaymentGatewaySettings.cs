using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class PaymentGatewaySettings
    {
        public Guid Id { get; set; }

        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }
        public Guid LicenseeId { get; set; }

        public string OnlinePaymentMethodName { get; set; }

        public string PaymentGatewayName { get; set; }

        public int Channel { get; set; }

        public string EntryPoint { get; set; }

        public string Remarks { get; set; }

        public Status Status { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
        public string ActivatedBy { get; set; }
        public DateTimeOffset? DateActivated { get; set; }
        public string DeactivatedBy { get; set; }
        public DateTimeOffset? DateDeactivated { get; set; }

        public virtual ICollection<PaymentLevel> PaymentLevels { get; set; }
    }
}