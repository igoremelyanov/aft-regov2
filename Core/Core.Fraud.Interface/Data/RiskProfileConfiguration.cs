using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;

namespace AFT.RegoV2.Core.Fraud.Interface.Data
{
    public class RiskProfileConfiguration : ICommonVerificationCheckConfiguration
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BrandId { get; set; }
        public virtual Interface.Data.Brand Brand { get; set; }
        public string Currency { get; set; }
        public ICollection<VipLevel> VipLevels { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public Guid CreatedBy { get; set; }

        #region AccountAge

        public bool HasAccountAge { get; set; }
        public ComparisonEnum AccountAgeOperator { get; set; }
        public int AccountAge { get; set; }

        #endregion

        #region TotalDepositCount
        public bool HasDepositCount { get; set; }
        public int TotalDepositCountAmount { get; set; }
        public ComparisonEnum TotalDepositCountOperator { get; set; }
        #endregion

        #region WithdrawalCount
        public bool HasWithdrawalCount { get; set; }
        public decimal TotalWithdrawalCountAmount { get; set; }
        public ComparisonEnum TotalWithdrawalCountOperator { get; set; }
        #endregion

        #region WinLoss
        public bool HasWinLoss { get; set; }
        public ComparisonEnum WinLossOperator { get; set; }
        public decimal WinLossAmount { get; set; }
        #endregion

        #region RiskLevels
        public bool HasFraudRiskLevel { get; set; }

        public ICollection<RiskLevel> AllowedRiskLevels { get; set; }
        #endregion

        #region PaymentMethods
        public ICollection<PaymentMethod> AllowedPaymentMethods { get; set; }

        public bool HasPaymentMethodCheck { get; set; }
        #endregion

        #region BonusCheck
        public bool HasBonusCheck { get; set; }
        public ICollection<Bonus> AllowedBonuses { get; set; }
        #endregion

        #region WithdrawalAverage
        public bool HasWithdrawalAveragePercentageCheck { get; set; }
        public ComparisonEnum WithdrawalAveragePercentageOperator { get; set; }
        public decimal WithdrawalAveragePercentage { get; set; }
        #endregion

        #region WinningsToDepositPercentageIncrease
        public bool HasWinningsToDepositPercentageIncreaseCheck { get; set; }
        public ComparisonEnum WinningsToDepositPercentageIncreaseOperator { get; set; }
        public decimal WinningsToDepositPercentageIncrease { get; set; }
        #endregion

        public RiskProfileConfiguration()
        {
            AllowedRiskLevels = new List<RiskLevel>();
            AllowedPaymentMethods = new List<PaymentMethod>();
            AllowedBonuses = new List<Bonus>();
            VipLevels = new List<VipLevel>();
        }
    }
}