using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Payment.Data
{
    public class TransferSettings
    {
        public Guid Id { get; set; }

        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }

        [Required]
        public TransferFundType TransferType { get; set; }

        public Guid VipLevelId { get; set; }
        public VipLevel VipLevel { get; set; }

        [Required, StringLength(3)]
        public string CurrencyCode { get; set; }

        [Required]
        public string WalletId { get; set; }

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

        public bool Enabled { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
        public string EnabledBy { get; set; }
        public DateTimeOffset? EnabledDate { get; set; }
        public string DisabledBy { get; set; }
        public DateTimeOffset? DisabledDate { get; set; }
    }
}