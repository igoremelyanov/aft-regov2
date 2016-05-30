using System.Data.Entity;
using AFT.RegoV2.Core.Fraud;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeFraudRepository : IFraudRepository
    {
        #region Fields

        private readonly FakeDbSet<Brand> _brands = new FakeDbSet<Brand>();
        private readonly FakeDbSet<VipLevel> _vipLevels = new FakeDbSet<VipLevel>();
        private readonly FakeDbSet<WithdrawalVerificationLog> _withdrawalVerificationLogs = new FakeDbSet<WithdrawalVerificationLog>();
        private readonly FakeDbSet<RiskLevel> _riskLevels = new FakeDbSet<RiskLevel>();
        private readonly FakeDbSet<PlayerRiskLevel> _playerRiskLevels = new FakeDbSet<PlayerRiskLevel>();
        private readonly FakeDbSet<WagerConfiguration> _wagerConfigurations = new FakeDbSet<WagerConfiguration>();
        private readonly FakeDbSet<AutoVerificationCheckConfiguration> _autoVerificationCheckConfigurations = new FakeDbSet<AutoVerificationCheckConfiguration>();
        private readonly FakeDbSet<RiskProfileConfiguration> _riskProfileCheckConfigurations = new FakeDbSet<RiskProfileConfiguration>();
        private readonly FakeDbSet<WinningRule> _winningRules = new FakeDbSet<WinningRule>();
        private readonly FakeDbSet<PaymentLevel> _paymentLevels = new FakeDbSet<PaymentLevel>();
        private readonly FakeDbSet<PaymentMethod> _paymentMethods = new FakeDbSet<PaymentMethod>();
        private readonly FakeDbSet<Core.Fraud.Interface.Data.Bonus> _bonuses = new FakeDbSet<Core.Fraud.Interface.Data.Bonus>();
        private readonly FakeDbSet<Player> _players = new FakeDbSet<Player>();
        private readonly FakeDbSet<MatchingResult> _matchingResults = new FakeDbSet<MatchingResult>();
        private readonly FakeDbSet<SignUpFraudType> _signUpFraudTypes = new FakeDbSet<SignUpFraudType>();
        private readonly FakeDbSet<MatchingCriteria> _matchingCriterias = new FakeDbSet<MatchingCriteria>();
        private readonly FakeDbSet<DuplicateMechanismConfiguration> _duplicateMechanismConfigurations = new FakeDbSet<DuplicateMechanismConfiguration>();
        #endregion

        #region IFraudRepository Members

        public IDbSet<MatchingCriteria> MatchingCriterias
        {
            get { return _matchingCriterias; }
        }

        public IDbSet<Brand> Brands
        {
            get { return _brands; }
        }

        public IDbSet<WithdrawalVerificationLog> WithdrawalVerificationLogs
        {
            get { return _withdrawalVerificationLogs; }
        }

        public IDbSet<RiskLevel> RiskLevels
        {
            get { return _riskLevels; }
        }

        public IDbSet<VipLevel> VipLevels
        {
            get { return _vipLevels; }
        }

        public IDbSet<SignUpFraudType> SignUpFraudTypes
        {
            get { return _signUpFraudTypes; }
        }

        public IDbSet<RiskProfileConfiguration> RiskProfileConfigurations
        {
            get { return _riskProfileCheckConfigurations; }
        }

        public IDbSet<Core.Fraud.Interface.Data.Bonus> Bonuses
        {
            get { return _bonuses; }
        }

        public IDbSet<PaymentMethod> PaymentMethods
        {
            get { return _paymentMethods; }
        }

        public IDbSet<PlayerRiskLevel> PlayerRiskLevels
        {
            get { return _playerRiskLevels; }
        }

        public IDbSet<AutoVerificationCheckConfiguration> AutoVerificationCheckConfigurations
        {
            get { return _autoVerificationCheckConfigurations; }
        }

        public IDbSet<DuplicateMechanismConfiguration> DuplicateMechanismConfigurations
        {
            get { return _duplicateMechanismConfigurations; }
        }

        public IDbSet<WagerConfiguration> WagerConfigurations
        {
            get { return _wagerConfigurations; }
        }

        public IDbSet<WinningRule> WinningRules
        {
            get { return _winningRules; }
        }

        public IDbSet<PaymentLevel> PaymentLevels
        {
            get { return _paymentLevels; }
        }

        public IDbSet<Player> Players
        {
            get { return _players; }
        }

        public IDbSet<MatchingResult> MatchingResults
        {
            get { return _matchingResults; }
        }

        public int SaveChanges()
        {
            return 0;
        }

        #endregion
    }
}