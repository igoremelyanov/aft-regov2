using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.Interface
{
    public static class RiskResultConverter
    {
        private static readonly List<VerificationStep> CommonVerificationSteps = new List<VerificationStep>
        {
            VerificationStep.AccountAge,
            VerificationStep.WithdrawalCount,
            VerificationStep.WinLoss,
            VerificationStep.DepositCount,
            VerificationStep.WithdrawalAveragePercentageCheck
        };

        /// <summary>
        /// The way we determine the actual status for that RPC rule is as follows:
        /// RiskProfileCheckValidationService:
        /// -ValidateBonus(Failed -> High)
        /// -ValidatePaymentMethod(Failed -> High)
        /// -ValidateWithdrawalAveragePercentageChange(Failed -> High)
        /// -ValidateWinningsToDepositPercentageIncrease(Failed -> High)
        /// -ValidateAccountAge(Failed -> Low)
        /// -ValidateTotalWithdrawalCount(Failed -> Low)
        /// -ValidatePlayersFraudRiskLevel(Failed -> Low)
        /// -ValidateWinLossRule(Failed -> Low)
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="step"></param>
        public static FraudRiskLevelStatus GetStatusForRpc(bool isSuccess, VerificationStep step)
        {
            if (isSuccess && CommonVerificationSteps.Contains(step))
                return FraudRiskLevelStatus.High;
            if (isSuccess && !CommonVerificationSteps.Contains(step))
                return FraudRiskLevelStatus.Low;
            if (!isSuccess && CommonVerificationSteps.Contains(step))
                return FraudRiskLevelStatus.Low;
            if (!isSuccess && !CommonVerificationSteps.Contains(step))
                return FraudRiskLevelStatus.High;

            return FraudRiskLevelStatus.Low;
        }
    }
}
