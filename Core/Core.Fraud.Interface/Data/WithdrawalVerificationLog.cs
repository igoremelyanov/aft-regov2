using System;
using System.ComponentModel;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.Data
{
    public class WithdrawalVerificationLog
    {
        public Guid Id { get; set; }
        public Guid WithdrawalId { get; set; }
        public VerificationStep VerificationStep { get; set; }
        public bool IsSuccess { get; set; }
        public VerificationType VerificationType { get; set; }
        public string Status { get; set; }

        public string VerificationRule { get; set; }
        public string RuleRequiredValue { get; set; }
        public string CurrentValue { get; set; }
    }

    public enum VerificationStep
    {
        [Description("Fraud risk level")]
        FraudRiskLevel,
        [Description("Withdrawal exemption")]
        WithdrawalExemption,
        [Description("No recent bonus criteria")]
        NoRecentBonus,
        [Description("Winloss")]
        WinLoss,
/*        [Description("Fund-out criteria")]
        FundOut,*/
        [Description("Deposit count")]
        DepositCount,
        [Description("Withdrawal count")]
        WithdrawalCount,
        [Description("Account age")]
        AccountAge,
        [Description("Total deposit amount")]
        TotalDepositAmount,
        [Description("Has winnings")]
        HasWinnings,
        [Description("Payment method criteria")]
        PaymentMethod,
        [Description("Payment level")]
        PaymentLevel,
        [Description("Recent bonus criteria")]
        RecentBonus,
        [Description("Withdrawal average percentage check criteria")]
        WithdrawalAveragePercentageCheck,
        [Description("Winnings to deposit percentage increase criteria")]
        WinningsToDepositPercentageIncrease,
        [Description("Has documents")]
        HasDocuments
    }

    public enum VerificationType
    {
        AutoVerification,
        RiskProfileCheck,
        VerificatoinQueue
    }
}
