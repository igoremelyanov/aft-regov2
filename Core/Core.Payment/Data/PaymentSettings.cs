using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;

namespace AFT.RegoV2.Core.Payment.Data
{    
    public class PaymentSettings
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public Brand Brand { get; protected set; }

        public PaymentType PaymentType { get; set; }

        [Required]
        public string VipLevel { get; set; }

        [Required, StringLength(3)]
        public string CurrencyCode { get; set; }

        public PaymentMethod PaymentGatewayMethod { get; set; }

        public string PaymentMethod { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinAmountPerTransaction { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxAmountPerTransaction { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxAmountPerDay { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxTransactionPerDay { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxTransactionPerWeek { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxTransactionPerMonth { get; set; }

        public Status Enabled { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string EnabledBy { get; set; }
        public DateTime? EnabledDate { get; set; }
        public string DisabledBy { get; set; }
        public DateTime? DisabledDate { get; set; }
    }
}