using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using PaymentMethod = AFT.RegoV2.Core.Common.Data.Payment.PaymentMethod;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class RiskProfileCheckConfigurationBuilder
    {
        public RiskProfileCheckDTO Configuration { get; private set; }

        public RiskProfileCheckConfigurationBuilder(Guid brandId,
            Guid licenseeId,
            string currency,
            IEnumerable<Guid> vipLevelIds)
        {
            Configuration = new RiskProfileCheckDTO
            {
                Id = Guid.NewGuid(),
                Brand = brandId,
                Licensee = licenseeId,
                Currency = currency,
                VipLevels = vipLevelIds
            };
        }

        public RiskProfileCheckConfigurationBuilder SetupWinLoss(decimal amount, ComparisonEnum @operator)
        {
            Configuration.HasWinLoss = true;
            Configuration.WinLossAmount = amount;
            Configuration.WinLossOperator = @operator;

            return this;
        }

        public RiskProfileCheckConfigurationBuilder SetupDepositCount(int amount, ComparisonEnum @operator)
        {
            Configuration.HasDepositCount = true;
            Configuration.TotalDepositCountAmount = amount;
            Configuration.TotalDepositCountOperator = @operator;

            return this;
        }

        public RiskProfileCheckConfigurationBuilder SetupAccountAge(int age, ComparisonEnum @operator)
        {
            Configuration.HasAccountAge = true;
            Configuration.AccountAge = age;
            Configuration.AccountAgeOperator = @operator;

            return this;
        }

        public RiskProfileCheckConfigurationBuilder SetupTotalWithdrawalCountAmount(int amount, ComparisonEnum @operator)
        {
            Configuration.HasWithdrawalCount = true;
            Configuration.TotalWithdrawalCountAmount = amount;
            Configuration.TotalWithdrawalCountOperator = @operator;

            return this;
        }

        public RiskProfileCheckConfigurationBuilder SetupFraudRiskLevel(IEnumerable<Guid> riskLevelIds)
        {
            Configuration.HasFraudRiskLevel = true;
            Configuration.RiskLevels = riskLevelIds;

            return this;
        }

        public RiskProfileCheckConfigurationBuilder SetupPaymentMethodCheck(IEnumerable<PaymentMethod> paymentMethods)
        {
            Configuration.HasPaymentMethodCheck = true;
            Configuration.PaymentMethods = paymentMethods.Select(o => (int)o);

            return this;
        }

        public RiskProfileCheckConfigurationBuilder SetupBonusCheck(IEnumerable<Guid> bonusIds)
        {
            Configuration.HasBonusCheck = true;
            Configuration.Bonuses = bonusIds;

            return this;
        }

        public RiskProfileCheckConfigurationBuilder SetupWithdrawalAverageChange(int amount, ComparisonEnum @operator)
        {
            Configuration.HasWithdrawalAverageChange = true;
            Configuration.WithdrawalAverageChangeOperator = @operator;
            Configuration.WithdrawalAverageChangeAmount = amount;

            return this;
        }

        public RiskProfileCheckConfigurationBuilder SetupWinningsToDepositIncrease(int amount, ComparisonEnum @operator)
        {
            Configuration.HasWinningsToDepositIncrease = true;
            Configuration.WinningsToDepositIncreaseOperator = @operator;
            Configuration.WinningsToDepositIncreaseAmount = amount;

            return this;
        }
    }
}
