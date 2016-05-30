﻿using System;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class TransferSettingsId
    {
        private readonly Guid _id;

        public TransferSettingsId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(TransferSettingsId id)
        {
            return id._id;
        }

        public static implicit operator TransferSettingsId(Guid id)
        {
            return new TransferSettingsId(id);
        }
    }

    public class TransferSettings
    {
        public Guid Id { get; set; }

        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }
        
        public TransferFundType TransferType { get; set; }

        public Guid VipLevelId { get; set; }
        public VipLevel VipLevel { get; set; }

        public string CurrencyCode { get; set; }

        public string WalletId { get; set; }

        public decimal MinAmountPerTransaction { get; set; }
        
        public decimal MaxAmountPerTransaction { get; set; }

        public decimal MaxAmountPerDay { get; set; }

        public int MaxTransactionPerDay { get; set; }

        public int MaxTransactionPerWeek { get; set; }

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
