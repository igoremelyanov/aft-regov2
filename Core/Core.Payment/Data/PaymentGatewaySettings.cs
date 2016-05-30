using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Payment.Data
{
    public class PaymentGatewaySettings
    {
        public Guid Id { get; set; }

        public Guid BrandId { get; set; }
        public AFT.RegoV2.Core.Payment.Data.Brand Brand { get; set; }

        [Required,MinLength(2), MaxLength(100)]
        public string OnlinePaymentMethodName { get; set; }

        [Required]
        public string PaymentGatewayName { get; set; }

        [Required]
        public int Channel { get; set; }

        [Required,MinLength(1), MaxLength(100)]
        public string EntryPoint { get; set; }        

        [Required,MinLength(1), MaxLength(200)]
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