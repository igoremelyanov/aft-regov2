﻿using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;

namespace AFT.RegoV2.Core.Fraud.Interface.Data
{
    public class AutoVerificationCheckConfiguration : ICommonVerificationCheckConfiguration
    {
        private Guid _id;

        #region Properties

        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public Guid BrandId { get; set; }
        public virtual Brand Brand { get; set; }
        public string Currency { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public Guid CreatedBy { get; set; }

        public bool HasFraudRiskLevel { get; set; }
        public virtual ICollection<RiskLevel> AllowedRiskLevels { get; set; }

        //Payment level criteria
        public bool HasPaymentLevel { get; set; }
        public virtual ICollection<PaymentLevel> PaymentLevels { get; set; }

        public ICollection<VipLevel> VipLevels { get; set; }

        public bool HasWithdrawalExemption { get; set; }
        public bool HasNoRecentBonus { get; set; }

        //WinLoss criteria
        public bool HasWinLoss { get; set; }
        public decimal WinLossAmount { get; set; }
        public ComparisonEnum WinLossOperator { get; set; }

        //Total deposit count criteria
        public bool HasDepositCount { get; set; }
        public int TotalDepositCountAmount { get; set; }
        public ComparisonEnum TotalDepositCountOperator { get; set; }

        //Total withdrawal count criteria
        public bool HasWithdrawalCount{ get; set; }
        public decimal TotalWithdrawalCountAmount { get; set; }
        public ComparisonEnum TotalWithdrawalCountOperator { get; set; }

        //Total withdrawal count criteria
        public bool HasAccountAge { get; set; }
        public int AccountAge { get; set; }
        public ComparisonEnum AccountAgeOperator { get; set; }

        //Total Deposit amount criteria
        public bool HasTotalDepositAmount { get; set; }
        public decimal TotalDepositAmount { get; set; }
        public ComparisonEnum TotalDepositAmountOperator { get; set; }
      
        public bool HasWinnings { get; set; }
        public virtual ICollection<WinningRule> WinningRules { get; set; }

        public bool HasCompleteDocuments { get; set; }

        //Status-related properties
        public AutoVerificationCheckStatus Status { get; set; }
        public string ActivatedBy { get; set; }
        public DateTimeOffset? DateActivated { get; set; }
        public string DeactivatedBy { get; set; }
        public DateTimeOffset? DateDeactivated { get; set; }

        #endregion

        #region Constructors

        public AutoVerificationCheckConfiguration()
        {
            AllowedRiskLevels = new List<RiskLevel>();
            WinningRules = new List<WinningRule>();
            PaymentLevels = new List<PaymentLevel>();
            VipLevels = new List<VipLevel>();
        }

        #endregion
    }
}