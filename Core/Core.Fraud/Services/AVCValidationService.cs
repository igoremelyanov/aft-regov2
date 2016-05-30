using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Fraud.Extensions;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;
using OfflineWithdraw = AFT.RegoV2.Core.Payment.Data.OfflineWithdraw;

namespace AFT.RegoV2.Core.Fraud.Services
{
    public class AVCValidationService : CommonWithdrawalValidationService, IAVCValidationService
    {
        #region Fields

        private readonly IAVCConfigurationQueries _avcConfigurationQueries;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IGameQueries _gameQueries;
        private readonly IWithdrawalVerificationLogsCommands _logsCommands;
        private readonly IWithdrawalVerificationLogsQueues _withdrawalVerificationLogsQueues;
        private readonly IEventBus _eventBus;
        private AutoVerificationCheckConfiguration _configuration;
        private readonly IPlayerIdentityValidator _identityValidator;

        #endregion

        public bool Failed { get; private set; }

        #region Constructors

        public AVCValidationService(
            IAVCConfigurationQueries avcConfigurationQueries,
            IPaymentRepository paymentRepository,
            IRiskLevelQueries riskLevelQueries,
            IGameQueries gameQueries,
            IWalletQueries walletQueries,
            IWithdrawalVerificationLogsCommands logsCommands,
            IWithdrawalVerificationLogsQueues withdrawalVerificationLogsQueues,
            IEventBus eventBus, IPlayerIdentityValidator identityValidator)
            : base(paymentRepository, riskLevelQueries, walletQueries)
        {
            _avcConfigurationQueries = avcConfigurationQueries;
            _paymentRepository = paymentRepository;
            _gameQueries = gameQueries;
            _logsCommands = logsCommands;
            _withdrawalVerificationLogsQueues = withdrawalVerificationLogsQueues;
            _eventBus = eventBus;
            _identityValidator = identityValidator;
        }

        #endregion

        #region Public methods

        public void Validate(Guid withdrawalId)
        {
            var withdrawal = _paymentRepository
                .OfflineWithdraws
                .Include(x => x.PlayerBankAccount)
                .Single(x => x.Id == withdrawalId);

            var bankAccount =
                _paymentRepository.PlayerBankAccounts
                    .Include(x => x.Player)
                    .Include(x => x.Player.Brand)
                    .FirstOrDefault(x => x.Id == withdrawal.PlayerBankAccount.Id);

            _configuration = _avcConfigurationQueries
                .GetAutoVerificationCheckConfigurations()
                .SingleOrDefault(
                    x =>
                        x.BrandId == bankAccount.Player.BrandId
                        && x.VipLevels.Select(o => o.Id).Contains(bankAccount.Player.VipLevelId)
                        && x.Currency == bankAccount.Player.CurrencyCode
                        && x.Status == AutoVerificationCheckStatus.Active);

            var eventCreated = DateTimeOffset.Now.ToBrandOffset(bankAccount.Player.Brand.TimezoneId);

            if (_configuration == null)
            {
                withdrawal.AutoVerificationCheckDate = DateTimeOffset.UtcNow;
                withdrawal.Status = WithdrawalStatus.Verified;

                _eventBus.Publish(new WithdrawalVerified(
                    withdrawalId,
                    withdrawal.Amount,
                    eventCreated,
                    withdrawal.PlayerBankAccount.Player.Id,
                    withdrawal.PlayerBankAccount.Player.Id,
                    WithdrawalStatus.Verified,
                    "Automatic auto verification check succeeded!",
                    withdrawal.TransactionNumber,
                    "System")
                {
                    EventCreated = eventCreated
                });

                _paymentRepository.SaveChanges();
                return;
            }

            withdrawal.AutoVerificationCheckDate = DateTimeOffset.UtcNow;

            if (_configuration.HasWithdrawalExemption && PlayerIsAllowedForExemption(bankAccount.Player, withdrawal.AutoVerificationCheckDate))
            {
                ApplyExemptionForThatWithdrawalRequest(bankAccount.Player, withdrawalId, withdrawal);
                return;
            }

            base.Validate(_configuration, withdrawalId);

            ValidateTotalDepositAmount(withdrawalId, bankAccount.Player.Id);
            ValidatePlayersWinningsRules(withdrawalId, bankAccount.Player.Id);
            ValidatePaymentLevel(withdrawalId, bankAccount.Player);
            ValidateHasCompleteDocuments(withdrawalId, bankAccount.Player, bankAccount.Player.BrandId);
            ValidateHasOfflineWithdrawalExeption(withdrawalId, bankAccount.Player, withdrawal.AutoVerificationCheckDate);

            if (_withdrawalVerificationLogsQueues.HasFailedCriteras(withdrawalId, VerificationType.AutoVerification))
            {
                withdrawal.AutoVerificationCheckStatus = CommonVerificationStatus.Failed;
                withdrawal.Status = WithdrawalStatus.AutoVerificationFailed;

                eventCreated = DateTimeOffset.Now.ToBrandOffset(bankAccount.Player.Brand.TimezoneId);

                _eventBus.Publish(new WithdrawalUnverified(
                    withdrawalId,
                    withdrawal.Amount,
                    eventCreated,
                    withdrawal.PlayerBankAccount.Player.Id,
                    withdrawal.PlayerBankAccount.Player.Id,
                    WithdrawalStatus.Unverified,
                    "Automatic auto verification check failed!",
                    withdrawal.TransactionNumber,
                    "System")
                {
                    EventCreated = eventCreated
                });
            }
            else
            {
                withdrawal.AutoVerificationCheckStatus = CommonVerificationStatus.Passed;
                withdrawal.Status = WithdrawalStatus.Verified;

                _eventBus.Publish(new WithdrawalVerified(
                    withdrawalId,
                    withdrawal.Amount,
                    eventCreated,
                    withdrawal.PlayerBankAccount.Player.Id,
                    withdrawal.PlayerBankAccount.Player.Id,
                    WithdrawalStatus.Verified,
                    "Automatic auto verification check succeeded!",
                    withdrawal.TransactionNumber,
                    "System")
                {
                    EventCreated = eventCreated
                });
            }

            _paymentRepository.SaveChanges();
        }

        #endregion

        #region Private methods

        private void ApplyExemptionForThatWithdrawalRequest(Payment.Data.Player player, Guid withdrawalId, OfflineWithdraw withdrawal)
        {
            withdrawal.AutoVerificationCheckStatus = CommonVerificationStatus.Passed;
            withdrawal.Status = WithdrawalStatus.Verified;

            try
            {
                using (var scope = CustomTransactionScope.GetTransactionScope())
                {
                    var playerToBeUpdated = _paymentRepository.Players.FirstOrDefault(pl => pl.Id == player.Id);

                    if (playerToBeUpdated != null)
                        playerToBeUpdated.ExemptLimit -= 1;

                    _paymentRepository.SaveChanges();

                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("There was a problem while decresing player's exemtion limit. For more information:{0}", ex.Message));
            }

            _eventBus.Publish(new WithdrawalVerified(
                withdrawalId,
                withdrawal.Amount,
                DateTime.Now,
                withdrawal.PlayerBankAccount.Player.Id,
                withdrawal.PlayerBankAccount.Player.Id,
                WithdrawalStatus.Verified,
                string.Format("Withdrawal exemption was applied for:{0}!", withdrawal.PlayerBankAccount.Player.GetFullName()),
                withdrawal.TransactionNumber,
                "System"));
        }

        private bool PlayerIsAllowedForExemption(Payment.Data.Player player, DateTimeOffset? autoVerificationCheckDate)
        {
            return player.ExemptWithdrawalFrom <= autoVerificationCheckDate &&
               player.ExemptWithdrawalTo >= autoVerificationCheckDate &&
               player.ExemptLimit > 0 &&
               player.ExemptWithdrawalVerification == true;
        }

        private DateTimeOffset? GetEndDate(WinningRule rule)
        {
            var now = DateTimeOffset.Now;
            switch (rule.Period)
            {
                case PeriodEnum.Last7Days:
                case PeriodEnum.Last14Days:
                case PeriodEnum.CurrentYear:
                case PeriodEnum.FromSignUp:
                    return now;
                case PeriodEnum.CustomDate:
                    return rule.EndDate;
            }
            return null;
        }

        private DateTimeOffset? GetStartDate(WinningRule rule, Guid playerId)
        {
            var now = DateTimeOffset.Now;
            switch (rule.Period)
            {
                case PeriodEnum.Last7Days:
                    return now.AddDays(-7);
                case PeriodEnum.Last14Days:
                    return now.AddDays(-14);
                case PeriodEnum.CurrentYear:
                    return new DateTime(now.Year, 1, 1);
                case PeriodEnum.FromSignUp:
                    var player = _paymentRepository.Players.Single(o => o.Id == playerId);
                    return player.DateRegistered.LocalDateTime;
                case PeriodEnum.CustomDate:
                    return rule.StartDate;
            }
            return null;
        }

        private void ValidatePlayersWinningsRules(Guid withdrawalId, Guid playerId)
        {
            if (!_configuration.HasWinnings)
                return;

            var allWinningRules = _configuration.WinningRules.ToList();

            foreach (var rule in allWinningRules)
            {
                var startDate = GetStartDate(rule, playerId);
                var endDate = GetEndDate(rule);

                var actualTransactions = _gameQueries.GetWinLossGameActions(rule.ProductId)
                    .Where(o => o.Round.PlayerId == playerId)
                    .Where(o => o.Timestamp >= startDate && o.Timestamp <= endDate);

                var wonAmount = actualTransactions.Where(o => o.GameActionType == GameActionType.Won).Sum(o => o.Amount);
                var lostAmount = actualTransactions.Where(o => o.GameActionType == GameActionType.Lost).Sum(o => o.Amount);
                var winningsAmount = wonAmount - lostAmount;

                var productForThatRule = _gameQueries.GetGameProviders().FirstOrDefault(x => x.Id == rule.ProductId);

                var productName = productForThatRule != null ? productForThatRule.Name : string.Empty;

                var metadataForRule = GenerateFullRuleDescriptionAndValue(
                VerificationStep.HasWinnings,
                ComparisonEnum.For,
                rule.WinningRuleDescription(productName),
                winningsAmount.ToString(CultureInfo.InvariantCulture));

                if (!IsRulePassed(rule.Comparison, rule.Amount, winningsAmount))
                {
                    OnRuleFinish(false, withdrawalId, VerificationStep.HasWinnings, metadataForRule);
                    break;
                }
                OnRuleFinish(true, withdrawalId, VerificationStep.HasWinnings, metadataForRule);
            }
        }

        private void ValidateTotalDepositAmount(Guid withdrawalId, Guid playerId)
        {
            if (!_configuration.HasTotalDepositAmount)
                return;

            var isSuccess = true;

            var totalDepositAmount = _paymentRepository.OfflineDeposits
                    .Where(x => x.Status == OfflineDepositStatus.Approved && x.PlayerId == playerId)
                    .Sum(x => x.Amount);

            isSuccess = IsRulePassed(_configuration.TotalDepositAmountOperator, _configuration.TotalDepositAmount, totalDepositAmount);


            var metadataForRule = GenerateFullRuleDescriptionAndValue(
                VerificationStep.TotalDepositAmount,
                _configuration.TotalDepositAmountOperator,
                _configuration.TotalDepositAmount.ToString(CultureInfo.InvariantCulture),
                totalDepositAmount.ToString(CultureInfo.InvariantCulture));

            OnRuleFinish(isSuccess, withdrawalId, VerificationStep.TotalDepositAmount, metadataForRule);
        }

        private void ValidatePaymentLevel(Guid withdrawalId, Payment.Data.Player player)
        {
            var playerPaymentLevel = _paymentRepository
                .PlayerPaymentLevels
                .Include(x => x.PaymentLevel)
                .FirstOrDefault(elem => elem.PlayerId == player.Id);

            if (!_configuration.HasPaymentLevel || playerPaymentLevel == null)
                return;

            var avcAllowedPaymentLevels = string.Join(",", _configuration
                .PaymentLevels.Select(pl => pl.Name).ToArray());

            var metadataForRule = GenerateFullRuleDescriptionAndValue(
                VerificationStep.PaymentLevel,
                ComparisonEnum.Of,
                avcAllowedPaymentLevels,
                playerPaymentLevel.PaymentLevel.Name);

            var avcContainsPlayersPaymentLevel = _configuration
                .PaymentLevels
                .Any(x => x.Id == playerPaymentLevel.PaymentLevel.Id);

            if (!avcContainsPlayersPaymentLevel)
                OnRuleFinish(false, withdrawalId, VerificationStep.PaymentLevel, metadataForRule);
            else
                OnRuleFinish(true, withdrawalId, VerificationStep.PaymentLevel, metadataForRule);
        }

        private void ValidateHasCompleteDocuments(Guid withdrawalId, Payment.Data.Player player, Guid brandId)
        {
            if (!_configuration.HasCompleteDocuments)
                return;

            var ruleResult = true;
            //Kristian: I leave it like this because the validation doesn't have a return code or value 
            //but it directly throws an exception if one of the needed documents is not present for that player.
            try
            {
                _identityValidator.Validate(player.Id, Common.Data.Player.TransactionType.Withdraw);
            }
            catch
            {
                ruleResult = false;
            }

            var metadataForRule = GenerateFullRuleDescriptionAndValue(
                VerificationStep.HasDocuments,
                ComparisonEnum.Is,
                _configuration.HasCompleteDocuments.ToString(),
                ruleResult.ToString());

            OnRuleFinish(ruleResult, withdrawalId, VerificationStep.HasDocuments, metadataForRule);
        }

        private void ValidateHasOfflineWithdrawalExeption(Guid withdrawalId, Payment.Data.Player player, DateTimeOffset? autoVerificationCheckDate)
        {
            if (!_configuration.HasWithdrawalExemption)
                return;

            var ruleResult = PlayerIsAllowedForExemption(player, autoVerificationCheckDate);

            var metadataForRule = GenerateFullRuleDescriptionAndValue(
                VerificationStep.WithdrawalExemption,
                ComparisonEnum.Is,
                _configuration.HasCompleteDocuments.ToString(),
                ruleResult.ToString());

            OnRuleFinish(true, withdrawalId, VerificationStep.HasDocuments, metadataForRule);
        }

        #endregion

        protected override void OnRuleFinish(bool result, Guid withdrawalId, VerificationStep step, CompleteRuleDescriptionAndActualValue metadata = null)
        {
            _logsCommands.LogWithdrawalVerificationStep(
                withdrawalId,
                result,
                VerificationType.AutoVerification,
                step,
                metadata.CompleteRuleDescription,
                metadata.RuleRequiredValues,
                metadata.ActualVerificationValue);

            if (!result)
                Failed = true;
        }
    }
}
