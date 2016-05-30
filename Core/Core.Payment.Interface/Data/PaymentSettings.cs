using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{    
    public class PaymentSettings
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public Brand Brand { get; protected set; }

        public PaymentType PaymentType { get; set; }

        public string VipLevel { get; set; }

        public string CurrencyCode { get; set; }

        public PaymentMethod PaymentGatewayMethod { get; set; }

        public string PaymentMethod { get; set; }

        public decimal MinAmountPerTransaction { get; set; }

        public decimal MaxAmountPerTransaction { get; set; }

        public decimal MaxAmountPerDay { get; set; }
        
        public int MaxTransactionPerDay { get; set; }

        public int MaxTransactionPerWeek { get; set; }

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

    public class PaymentSettingTransferObj
    {
        public object Brand { get; set; }
        public string PaymentMethod { get; set; }
        public string VipLevel { get; set; }
        public string CurrencyCode { get; set; }
        public decimal MinAmountPerTransaction { get; set; }
        public decimal MaxAmountPerTransaction { get; set; }
        public decimal MaxAmountPerDay { get; set; }
        public int MaxTransactionPerDay { get; set; }
        public int MaxTransactionPerWeek { get; set; }
        public int MaxTransactionPerMonth { get; set; }
        public Guid Id { get; set; }
        public string PaymentType { get; set; }
    }
}