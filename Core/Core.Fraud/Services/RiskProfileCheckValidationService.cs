using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Interface;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Fraud.Services
{
    public class RiskProfileCheckValidationService : CommonWithdrawalValidationService, IRiskProfileCheckValidationService
    {
        private readonly IRiskProfileCheckQueries _riskProfileCheckQueries;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IWithdrawalVerificationLogsCommands _logsCommands;
        private readonly IGameRepository _gameRepository;
        private readonly IBonusApiProxy _bonusApiProxy;
        private RiskProfileConfiguration _configuration = null;

        public RiskProfileCheckValidationService(
            IRiskProfileCheckQueries riskProfileCheckQueries,
            IPaymentRepository paymentRepository,
            IRiskLevelQueries riskLevelQueries,
            IWithdrawalVerificationLogsCommands logsCommands,
            IWalletQueries walletQueries,
            IGameRepository gameRepository,
            IBonusApiProxy bonusApiProxy)
            : base(paymentRepository, riskLevelQueries, walletQueries)
        {
            _riskProfileCheckQueries = riskProfileCheckQueries;
            _paymentRepository = paymentRepository;
            _logsCommands = logsCommands;
            _gameRepository = gameRepository;
            _bonusApiProxy = bonusApiProxy;
        }

        public void Validate(Guid withdrawalId)
        {
            var withdrawal = _paymentRepository
                .OfflineWithdraws
                .Include(x => x.PlayerBankAccount)
                .Single(x => x.Id == withdrawalId);

            var bankAccount = _paymentRepository.PlayerBankAccounts
                .Include(x => x.Player)
                .Single(x => x.Id == withdrawal.PlayerBankAccount.Id);

            _configuration = _riskProfileCheckQueries
                .GetConfigurations()
                .Include(o => o.AllowedPaymentMethods)
                .Include(o => o.AllowedBonuses)
                .Include(o => o.AllowedRiskLevels)
                .Include(o => o.VipLevels)
                .ToList()
                .SingleOrDefault(x =>
                        x.BrandId == bankAccount.Player.BrandId
                        && x.VipLevels.Select(o => o.Id).Contains(bankAccount.Player.VipLevelId) &&
                        x.Currency == bankAccount.Player.CurrencyCode);

            if (_configuration == null)
                return;

            base.Validate(_configuration, withdrawalId);

            ValidatePaymentMethod(withdrawal);
            ValidateBonus(withdrawal, bankAccount.Player);
            ValidateWithdrawalAveragePercentageChange(withdrawal, bankAccount.Player.Id);
            ValidateWinningsToDepositPercentageIncrease(withdrawal, bankAccount.Player.Id);
        }

        private async void ValidateBonus(OfflineWithdraw withdrawal, Payment.Data.Player player)
        {
            if (!_configuration.HasBonusCheck)
                return;

            var latestDeposit = _paymentRepository.OfflineDeposits
                    .Where(o => o.Status == OfflineDepositStatus.Approved && o.PlayerId == player.Id)
                    .OrderByDescending(o => o.Approved.Value)
                    .First();

            var allowedBonuses = _configuration.AllowedBonuses.Select(o => o.Name);

            var bonusName = latestDeposit.BonusRedemptionId.HasValue ?
                (await _bonusApiProxy.GetBonusRedemptionAsync(latestDeposit.PlayerId, latestDeposit.BonusRedemptionId.Value)).Bonus.Name :
                string.Empty;

            var isSuccess = !string.IsNullOrEmpty(bonusName) && allowedBonuses.Contains(bonusName);

            var metadataForRule = GenerateFullRuleDescriptionAndValue(
                VerificationStep.RecentBonus,
                ComparisonEnum.Of,
                string.Join(",", allowedBonuses.ToArray()),
                bonusName);


            OnRuleFinish(isSuccess, withdrawal.Id, VerificationStep.RecentBonus, metadataForRule);
        }

        private void ValidatePaymentMethod(OfflineWithdraw withdrawal)
        {
            if (!_configuration.HasPaymentMethodCheck)
                return;

            var allowedPaymentMethods = _configuration.AllowedPaymentMethods.Select(o => o.Code);

            var isSuccess = allowedPaymentMethods.Contains(withdrawal.PaymentMethod.ToString());

            var metadataForRule = GenerateFullRuleDescriptionAndValue(
                VerificationStep.PaymentMethod,
                ComparisonEnum.Of,
                string.Join(",", allowedPaymentMethods.ToArray()),
                withdrawal.PaymentMethod.ToString());

            OnRuleFinish(isSuccess, withdrawal.Id, VerificationStep.PaymentMethod, metadataForRule);
        }

        private void ValidateWithdrawalAveragePercentageChange(OfflineWithdraw withdrawal, Guid playerId)
        {
            if (!_configuration.HasWithdrawalAveragePercentageCheck)
                return;

            var playersApprovedWithdraws = _paymentRepository.OfflineWithdraws
                    .Where(o => o.PlayerBankAccount.Player.Id == playerId && o.Status == WithdrawalStatus.Approved).ToList();

            if (!playersApprovedWithdraws.Any())
            {
                var metaForRule = GenerateFullRuleDescriptionAndValue(
                VerificationStep.WithdrawalAveragePercentageCheck,
                _configuration.WithdrawalAveragePercentageOperator,
                _configuration.WithdrawalAveragePercentage.ToString(CultureInfo.InvariantCulture),
                Convert.ToString(0));

                OnRuleFinish(true, withdrawal.Id, VerificationStep.WithdrawalAveragePercentageCheck, metaForRule);
                return;
            }

            var totalWithdrawalAmount = playersApprovedWithdraws.Sum(o => o.Amount);
            var totalWithdrawalCount = playersApprovedWithdraws.Count;
            var withdrawalAve = totalWithdrawalAmount / totalWithdrawalCount;
            var withdrawalDiff = withdrawal.Amount - withdrawalAve;
            var withdrawalAveChange = withdrawalDiff * 100 / withdrawalAve;
            var isSuccess = IsRulePassed(_configuration.WithdrawalAveragePercentageOperator,
                _configuration.WithdrawalAveragePercentage, withdrawalAveChange);


            var metadataForRule = GenerateFullRuleDescriptionAndValue(
                VerificationStep.WithdrawalAveragePercentageCheck,
                _configuration.WithdrawalAveragePercentageOperator,
                _configuration.WithdrawalAveragePercentage.ToString(CultureInfo.InvariantCulture),
                withdrawalAveChange.ToString(CultureInfo.InvariantCulture));

            OnRuleFinish(isSuccess, withdrawal.Id, VerificationStep.WithdrawalAveragePercentageCheck, metadataForRule);
        }

        private void ValidateWinningsToDepositPercentageIncrease(OfflineWithdraw withdrawal, Guid playerId)
        {
            if (!_configuration.HasWinningsToDepositPercentageIncreaseCheck)
                return;

            var playersApprovedWithdraws = _paymentRepository.OfflineWithdraws
                .Where(o => o != null)
                .Where(o => o.PlayerBankAccount.Player.Id == playerId && o.Status == WithdrawalStatus.Approved)
                .ToList();

            var playersApprovedDeposits = _paymentRepository.OfflineDeposits
                .Where(o => o.PlayerId == playerId && o.Status == OfflineDepositStatus.Approved);

            var totalWithdrawal = playersApprovedWithdraws.Sum(o => o.Amount);
            var totalDeposit = playersApprovedDeposits.Sum(o => o.Amount);
            var currentBalance = _gameRepository.Wallets.Where(o => o.PlayerId == playerId)
                .Sum(o => o.Balance); // Clarify waht should be added here Main or Total from the wallets
            var currentWinnings = totalDeposit - (totalWithdrawal + currentBalance + withdrawal.Amount);
            var winningsToDepositIncrease = (currentWinnings * 100 / totalDeposit);

            var isSuccess = IsRulePassed(_configuration.WinningsToDepositPercentageIncreaseOperator,
                _configuration.WinningsToDepositPercentageIncrease, winningsToDepositIncrease);

            var metadataForRule = GenerateFullRuleDescriptionAndValue(
                VerificationStep.WinningsToDepositPercentageIncrease,
                _configuration.WinningsToDepositPercentageIncreaseOperator,
                _configuration.WinningsToDepositPercentageIncrease.ToString(CultureInfo.InvariantCulture),
                winningsToDepositIncrease.ToString(CultureInfo.InvariantCulture));

            OnRuleFinish(isSuccess, withdrawal.Id, VerificationStep.WinningsToDepositPercentageIncrease, metadataForRule);
        }

        protected override void OnRuleFinish(bool result, Guid withdrawalId, VerificationStep step, CompleteRuleDescriptionAndActualValue metadata = null)
        {
            _logsCommands.LogWithdrawalVerificationStep(
                withdrawalId,
                result,
                VerificationType.RiskProfileCheck,
                step,
                metadata != null ? metadata.CompleteRuleDescription : "-",
                metadata != null ? metadata.RuleRequiredValues : "-",
                metadata != null ? metadata.ActualVerificationValue : "-"
            );

            var withdrawal = _paymentRepository.OfflineWithdraws
                .Single(o => o.Id == withdrawalId);

            withdrawal.RiskLevelStatus = RiskResultConverter.GetStatusForRpc(result, step);
            withdrawal.RiskLevelCheckDate = DateTimeOffset.UtcNow;

            _paymentRepository.SaveChanges();
        }
    }
}
