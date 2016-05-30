using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Fraud.Interface.Data
{
    public class RiskProfileCheckDTO
    {
        public Guid Id { get; set; }
        public Guid Brand { get; set; }
        public Guid Licensee { get; set; }
        public string Currency { get; set; }
        public IEnumerable<Guid> VipLevels { get; set; }

        public bool HasWinLoss { get; set; }
        public decimal WinLossAmount { get; set; }
        public ComparisonEnum WinLossOperator { get; set; }

        public bool HasDepositCount { get; set; }
        public int TotalDepositCountAmount { get; set; }
        public ComparisonEnum TotalDepositCountOperator { get; set; }

        public bool HasAccountAge { get; set; }
        public int AccountAge { get; set; }
        public ComparisonEnum AccountAgeOperator { get; set; }

        public bool HasWithdrawalCount { get; set; }
        public decimal TotalWithdrawalCountAmount { get; set; }
        public ComparisonEnum TotalWithdrawalCountOperator { get; set; }

        public bool HasFraudRiskLevel { get; set; }
        public IEnumerable<Guid> RiskLevels { get; set; }

        public bool HasPaymentMethodCheck { get; set; }
        public IEnumerable<int> PaymentMethods { get; set; }

        public bool HasBonusCheck { get; set; }
        public IEnumerable<Guid> Bonuses { get; set; }

        public bool HasWithdrawalAverageChange { get; set; }
        public ComparisonEnum WithdrawalAverageChangeOperator { get; set; }
        public decimal WithdrawalAverageChangeAmount { get; set; }

        public bool HasWinningsToDepositIncrease { get; set; }
        public ComparisonEnum WinningsToDepositIncreaseOperator { get; set; }
        public decimal WinningsToDepositIncreaseAmount { get; set; }

        public RiskProfileCheckDTO()
        {
            RiskLevels = new List<Guid>();
            Bonuses = new List<Guid>();
            PaymentMethods = new List<int>();
            VipLevels = new List<Guid>();
        }
    }
}
