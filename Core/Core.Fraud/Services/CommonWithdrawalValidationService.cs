using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Domain.Payment.Data;
using RiskLevelStatus = AFT.RegoV2.Core.Common.Data.RiskLevelStatus;

namespace AFT.RegoV2.Core.Fraud.Services
{
    public abstract class CommonWithdrawalValidationService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IRiskLevelQueries _riskLevelQueries;
        private readonly IWalletQueries _walletQueries;
        private ICommonVerificationCheckConfiguration _configuration;

        protected CommonWithdrawalValidationService(
            IPaymentRepository paymentRepository,
            IRiskLevelQueries riskLevelQueries,
            IWalletQueries walletQueries)
        {
            _paymentRepository = paymentRepository;
            _riskLevelQueries = riskLevelQueries;
            _walletQueries = walletQueries;
        }

        public void Validate(ICommonVerificationCheckConfiguration configuration, Guid withdrawalId)
        {
            var withdrawal = _paymentRepository
                .OfflineWithdraws
                .Include(x => x.PlayerBankAccount)
                .Single(x => x.Id == withdrawalId);

            var bankAccount = _paymentRepository.PlayerBankAccounts
                .Include(x => x.Player)
                .Single(x => x.Id == withdrawal.PlayerBankAccount.Id);

            _configuration = configuration;

            ValidateDepositCount(withdrawalId, bankAccount.Player.Id);
            ValidateAccountAge(withdrawalId, bankAccount.Player);
            ValidateTotalWithdrawalCount(withdrawalId, bankAccount.Player.Id);
            ValidatePlayersFraudRiskLevel(withdrawalId, bankAccount.Player.Id);
            ValidateWinLossRule(withdrawalId, bankAccount.Player.Id, withdrawal.Amount);
        }

        private void ValidateDepositCount(Guid withdrawalId, Guid playerId)
        {
            if (!_configuration.HasDepositCount)
                return;

            var isSuccess = true;

            var totalDepositAmount = _paymentRepository
                .OfflineDeposits
                .Count(x => x.PlayerId == playerId && x.Status == OfflineDepositStatus.Approved);

            isSuccess = IsRulePassed(_configuration.TotalDepositCountOperator, _configuration.TotalDepositCountAmount, totalDepositAmount);

            var metadataForRule = this.GenerateFullRuleDescriptionAndValue(
                VerificationStep.DepositCount,
                _configuration.TotalDepositCountOperator,
                _configuration.TotalDepositCountAmount.ToString(),
                totalDepositAmount.ToString());

            OnRuleFinish(isSuccess, withdrawalId, VerificationStep.DepositCount, metadataForRule);
        }

        private void ValidateAccountAge(Guid withdrawalId, Payment.Data.Player player)
        {
            if (!_configuration.HasAccountAge)
                return;

            var isSuccess = true;
            var daysFromRegistration = (player.DateRegistered - DateTimeOffset.Now).Days * -1;

            isSuccess = IsRulePassed(_configuration.AccountAgeOperator, _configuration.AccountAge, daysFromRegistration);

            var metadataForRule = this.GenerateFullRuleDescriptionAndValue(
                VerificationStep.AccountAge,
                _configuration.AccountAgeOperator,
                _configuration.AccountAge.ToString(),
                daysFromRegistration.ToString());

            OnRuleFinish(isSuccess, withdrawalId, VerificationStep.AccountAge, metadataForRule);
        }

        private void ValidateTotalWithdrawalCount(Guid withdrawalId, Guid playerId)
        {
            if (!_configuration.HasWithdrawalCount)
                return;

            var isSuccess = true;
            var widthdrawalCount = _paymentRepository.OfflineWithdraws
                    .Include(x => x.PlayerBankAccount)
                    .Count(x => x.PlayerBankAccount.Player.Id == playerId && x.Status == WithdrawalStatus.Approved);

            isSuccess = IsRulePassed(_configuration.TotalWithdrawalCountOperator, _configuration.TotalWithdrawalCountAmount, widthdrawalCount);

            var metadataForRule = GenerateFullRuleDescriptionAndValue(
                VerificationStep.WithdrawalCount,
                _configuration.TotalWithdrawalCountOperator,
                _configuration.TotalWithdrawalCountAmount.ToString(CultureInfo.InvariantCulture),
                widthdrawalCount.ToString());

            OnRuleFinish(isSuccess, withdrawalId, VerificationStep.WithdrawalCount, metadataForRule);
        }

        private void ValidatePlayersFraudRiskLevel(Guid withdrawalId, Guid playerId)
        {
            if (!_configuration.HasFraudRiskLevel)
                return;

            var isSuccess = true;

            var allPlayerRiskLevels = _riskLevelQueries
                .GetPlayerRiskLevels(playerId)
                .Where(x => x.RiskLevel.Status == RiskLevelStatus.Active)
                .ToList();

            var activeFraudlentRiskLevels = _configuration.AllowedRiskLevels
                .Where(frl => frl.Status == RiskLevelStatus.Active)
                .ToList();

            if (activeFraudlentRiskLevels.Any())
                isSuccess = !allPlayerRiskLevels.Any(x => activeFraudlentRiskLevels.Any(fRl => fRl.Id == x.RiskLevel.Id));

            var ruleValue = activeFraudlentRiskLevels.Any()
                ? string.Join(",", activeFraudlentRiskLevels.Select(rl => rl.Name))
                : "n/a";

            var actualValue = allPlayerRiskLevels.Any()
                ? string.Join(",", allPlayerRiskLevels.Select(prl => prl.RiskLevel.Name))
                : "n/a";

            var metadataForRule = GenerateFullRuleDescriptionAndValue(
                VerificationStep.FraudRiskLevel,
                ComparisonEnum.Of,
                ruleValue,
                actualValue);

            OnRuleFinish(isSuccess, withdrawalId, VerificationStep.FraudRiskLevel, metadataForRule);
        }

        private async void ValidateWinLossRule(Guid withdrawalId, Guid playerId, decimal amount)
        {
            if (!_configuration.HasWinLoss)
                return;

            var isSuccess = true;
            var totalWinLoss = await GetTotalWinLoss(playerId, amount);

            isSuccess = IsRulePassed(_configuration.WinLossOperator, _configuration.WinLossAmount, totalWinLoss);

            var metadataForRule = GenerateFullRuleDescriptionAndValue(
                VerificationStep.WinLoss,
                _configuration.WinLossOperator,
                _configuration.WinLossAmount.ToString(CultureInfo.InvariantCulture),
                totalWinLoss.ToString(CultureInfo.InvariantCulture));

            OnRuleFinish(isSuccess, withdrawalId, VerificationStep.WinLoss, metadataForRule);
        }

        protected abstract void OnRuleFinish(bool result, Guid withdrawalId, VerificationStep step, CompleteRuleDescriptionAndActualValue metadata = null);

        private async Task<decimal> GetTotalWinLoss(Guid playerId, decimal amount)
        {
            var totalDepositAmount = _paymentRepository
                .OfflineDeposits
                .Where(x => x.Status == OfflineDepositStatus.Approved)
                .Where(x => x.PlayerId == playerId)
                .Select(x => x.Amount)
                .Sum();

            var totalWithdrawalAmount = _paymentRepository
                .OfflineWithdraws
                .Include(x => x.PlayerBankAccount)
                .Include(x => x.PlayerBankAccount.Player)
                .Where(x => x.Status == WithdrawalStatus.Approved)
                .Where(x => x.PlayerBankAccount.Player.Id == playerId)
                .Select(x => x.Amount)
                .DefaultIfEmpty(0)
                .Sum();

            var mainBalance = (await _walletQueries.GetPlayerBalance(playerId)).Main;

            return totalDepositAmount - (totalWithdrawalAmount + mainBalance + amount);
        }

        protected bool IsRulePassed(ComparisonEnum op, decimal ruleAmount, decimal actualAmount)
        {
            switch (op)
            {
                case ComparisonEnum.Greater:
                    return ruleAmount < actualAmount;
                case ComparisonEnum.Less:
                    return ruleAmount > actualAmount;
                case ComparisonEnum.GreaterOrEqual:
                    return ruleAmount <= actualAmount;
                case ComparisonEnum.LessOrEqual:
                    return ruleAmount >= actualAmount;
            }

            return false;
        }

        protected bool IsRulePassed(ComparisonEnum op, int ruleAmount, int actualAmount)
        {
            return IsRulePassed(op, (decimal)ruleAmount, (decimal)actualAmount);
        }

        /// <summary>
        /// Based on the step, operation, criteria value and actual value, this method returns an object 
        /// that contains full rule description and actual value for that rule which will be stored into the DB.
        /// </summary>
        /// <param name="step">DepositCount, AccountAge, WithdrawalCount etc.</param>
        /// <param name="op">LessOrEqual, Is, Of etc.</param>
        /// <param name="ruleValue">Tha value for the rule as it is in the criteria</param>
        /// <param name="actualValue">Actual value for that rule coming from the WD request</param>
        /// <returns></returns>
        protected CompleteRuleDescriptionAndActualValue GenerateFullRuleDescriptionAndValue(
            VerificationStep step, ComparisonEnum op, string ruleValue, string actualValue)
        {
            var enumDescription = GetEnumDescription(step);
            var comparisonOperator = GetOperatorAsString(op);

            var result = new CompleteRuleDescriptionAndActualValue()
            {
                CompleteRuleDescription = string.Format("{0} {1}", enumDescription, comparisonOperator),
                RuleRequiredValues = ruleValue,
                ActualVerificationValue = actualValue
            };

            return result;
        }

        private string GetEnumDescription(VerificationStep step)
        {
            var type = typeof(VerificationStep);
            var memInfo = type.GetMember(step.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            return ((DescriptionAttribute)attributes[0]).Description;
        }

        private string GetOperatorAsString(ComparisonEnum op)
        {
            switch (op)
            {
                case ComparisonEnum.Greater:
                    return "greater than";
                case ComparisonEnum.GreaterOrEqual:
                    return "greater or equal to";
                case ComparisonEnum.Less:
                    return "less than";
                case ComparisonEnum.LessOrEqual:
                    return "less or equal to";
                case ComparisonEnum.Is:
                    return "must be";
                case ComparisonEnum.Of:
                    return "";
                case ComparisonEnum.For:
                    return "For";
                default:
                    return "";
            }
        }
    }

    public class CompleteRuleDescriptionAndActualValue
    {
        public string CompleteRuleDescription { get; set; }
        public string RuleRequiredValues { get; set; }
        public string ActualVerificationValue { get; set; }
    }
}
