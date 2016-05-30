using System.Data.Entity;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud
{
    public interface IFraudRepository
    {
        IDbSet<Interface.Data.Brand> Brands { get; }
        IDbSet<VipLevel> VipLevels { get; }
        IDbSet<RiskLevel> RiskLevels { get; }
        IDbSet<PlayerRiskLevel> PlayerRiskLevels { get; }
        IDbSet<WagerConfiguration> WagerConfigurations { get; }
        IDbSet<WithdrawalVerificationLog> WithdrawalVerificationLogs { get; }
        IDbSet<AutoVerificationCheckConfiguration> AutoVerificationCheckConfigurations { get; }
        IDbSet<WinningRule> WinningRules { get; }
        IDbSet<PaymentLevel> PaymentLevels { get; }
        IDbSet<Interface.Data.Bonus> Bonuses { get; }
        IDbSet<PaymentMethod> PaymentMethods { get; }
        IDbSet<RiskProfileConfiguration> RiskProfileConfigurations { get; }
        IDbSet<Data.Player> Players { get; } 
        IDbSet<Data.MatchingResult> MatchingResults { get; } 
        IDbSet<DuplicateMechanismConfiguration> DuplicateMechanismConfigurations { get; }
        IDbSet<SignUpFraudType> SignUpFraudTypes { get; }
        IDbSet<MatchingCriteria> MatchingCriterias { get; }
        int SaveChanges();
    }
}