using System.Collections.Generic;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.ApplicationServices
{
    public interface ICommonVerificationCheckConfiguration
    {
        bool HasAccountAge { get; }
        ComparisonEnum AccountAgeOperator { get; }
        int AccountAge { get; }
        bool HasDepositCount { get; }
        int TotalDepositCountAmount { get; }
        ComparisonEnum TotalDepositCountOperator { get; }
        bool HasWithdrawalCount { get; set; }
        decimal TotalWithdrawalCountAmount { get; set; }
        ComparisonEnum TotalWithdrawalCountOperator { get; set; }
        bool HasFraudRiskLevel { get; set; }
        bool HasWinLoss { get; set; }
        ComparisonEnum WinLossOperator { get; set; }
        decimal WinLossAmount { get; set; }
        ICollection<RiskLevel> AllowedRiskLevels { get; set; }
    }
}