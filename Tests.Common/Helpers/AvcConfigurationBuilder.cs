using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class AvcConfigurationBuilder
    {
        public AVCConfigurationDTO Configuration { get; private set; }

        public AvcConfigurationBuilder(Guid brandId, Guid[] vipLevelIds, string currency)
        {
            Configuration = new AVCConfigurationDTO
            {
                Id = Guid.NewGuid(),
                Brand = brandId,
                VipLevels = vipLevelIds,
                Currency = currency
            };
        }

        public AvcConfigurationBuilder SetupAccountAge(int age, ComparisonEnum @operator)
        {
            Configuration.HasAccountAge = true;
            Configuration.AccountAge = age;
            Configuration.AccountAgeOperator = @operator;

            return this;
        }

        public AvcConfigurationBuilder SetupWinLoss(decimal amount, ComparisonEnum @operator)
        {
            Configuration.HasWinLoss = true;
            Configuration.WinLossAmount = amount;
            Configuration.WinLossOperator = @operator;

            return this;
        }

        public AvcConfigurationBuilder SetupCompleteDocuments()
        {
            Configuration.HasCompleteDocuments = true;

            return this;
        }

        public AvcConfigurationBuilder SetupDepositCount(int amount, ComparisonEnum @operator)
        {
            Configuration.HasDepositCount = true;
            Configuration.TotalDepositCountAmount = amount;
            Configuration.TotalDepositCountOperator = @operator;

            return this;
        }

        public AvcConfigurationBuilder SetupFraudRiskdLevels(IEnumerable<Guid> riskLevels)
        {
            Configuration.HasFraudRiskLevel = true;
            Configuration.RiskLevels = riskLevels;

            return this;
        }

        public AvcConfigurationBuilder SetupNoRecentBonus(bool allowWithdrawalExemption)
        {
            Configuration.HasNoRecentBonus = true;
            Configuration.HasWithdrawalExemption = allowWithdrawalExemption;

            return this;
        }

        public AvcConfigurationBuilder SetupPaymentLevels(IEnumerable<Guid> paymentLevels)
        {
            Configuration.HasPaymentLevel = true;
            Configuration.PaymentLevels = paymentLevels;

            return this;
        }

        public AvcConfigurationBuilder SetupTotalDepositAmount(int amount, ComparisonEnum @operator)
        {
            Configuration.HasTotalDepositAmount = true;
            Configuration.TotalDepositAmount = amount;
            Configuration.TotalDepositAmountOperator = @operator;

            return this;
        }

        public AvcConfigurationBuilder SetupTotalWithdrawalCountAmount(int amount, ComparisonEnum @operator)
        {
            Configuration.HasWithdrawalCount = true;
            Configuration.TotalWithdrawalCountAmount = amount;
            Configuration.TotalWithdrawalCountOperator = @operator;

            return this;
        }

        public AvcConfigurationBuilder SetupWinnings(IEnumerable<WinningRuleDTO> winningRules)
        {
            Configuration.HasWinnings = true;
            Configuration.WinningRules = winningRules;

            return this;
        }
    }
}