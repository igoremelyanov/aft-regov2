using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.Commands;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using BonusRedemption = AFT.RegoV2.Bonus.Core.Data.BonusRedemption;
using Player = AFT.RegoV2.Bonus.Core.Entities.Player;

namespace AFT.RegoV2.Bonus.Core.ApplicationServices
{
    public class BonusCommands
    {
        private readonly IBonusRepository _repository;
        private readonly BonusQueries _bonusQueries;
        private readonly IEventBus _eventBus;
        private readonly IBrandOperations _brandOperations;
        private readonly IServiceBus _serviceBus;

        public BonusCommands(
            IBonusRepository repository,
            BonusQueries bonusQueries,
            IEventBus eventBus,
            IBrandOperations brandOperations,
            IServiceBus serviceBus)
        {
            _repository = repository;
            _bonusQueries = bonusQueries;
            _eventBus = eventBus;
            _brandOperations = brandOperations;
            _serviceBus = serviceBus;
        }

        public Guid ApplyForBonus(DepositBonusApplication model)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = _bonusQueries.GetValidationResult(model);
                if (validationResult.IsValid == false)
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);

                var player = _repository.GetLockedPlayer(model.PlayerId);
                var redemptionParams = new RedemptionParams
                {
                    TransferAmount = model.Amount,
                    TransferExternalId = model.DepositId,
                    TransferWalletTemplateId = player.Data.Brand.WalletTemplates.Single(wt => wt.IsMain).Id
                };
                var bonusId = model.BonusId ?? _repository.GetLockedBonus(model.BonusCode).Data.Id;
                var redemption = RedeemBonus(model.PlayerId, bonusId, redemptionParams);
                redemption.Events.ForEach(_eventBus.Publish);
                scope.Complete();

                return redemption.Data.Id;
            }
        }
        public void ApplyForBonus(FundInBonusApplication model)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = _bonusQueries.GetValidationResult(model);
                if (validationResult.IsValid == false)
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);

                var redemptionParams = new RedemptionParams { TransferAmount = model.Amount, TransferWalletTemplateId = model.DestinationWalletTemplateId };
                var bonusId = model.BonusId ?? _repository.GetLockedBonus(model.BonusCode).Data.Id;
                ActivateFundInBonus(model.PlayerId, bonusId, redemptionParams);
                scope.Complete();
            }
        }
        public void ClaimBonusRedemption(ClaimBonusRedemption model)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = _bonusQueries.GetValidationResult(model);
                if (validationResult.IsValid == false)
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);

                var redemption = _repository.GetBonusRedemption(model.PlayerId, model.RedemptionId);
                ClaimBonusRedemption(redemption);
                _repository.SaveChanges();

                redemption.Events.ForEach(_eventBus.Publish);
                scope.Complete();
            }
        }
        public void CancelBonusRedemption(CancelBonusRedemption model)
        {
            var playerId = model.PlayerId;
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = _bonusQueries.GetValidationResult(model);
                if (validationResult.IsValid == false)
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);

                var redemption = _repository.GetBonusRedemption(playerId, model.RedemptionId);
                var transaction = redemption.Cancel();

                _brandOperations.FundOut(playerId, transaction.TotalAmount, redemption.Data.Player.CurrencyCode, transaction.Id.ToString());

                _repository.SaveChanges();

                redemption.Events.ForEach(_eventBus.Publish);
                scope.Complete();
            }
        }
        public void IssueBonusByCs(IssueBonusByCs model)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = _bonusQueries.GetValidationResult(model);
                if (validationResult.IsValid == false)
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);

                var transaction =
                    _repository.Players.Single(p => p.Id == model.PlayerId)
                        .Wallets.SelectMany(w => w.Transactions)
                        .Single(t => t.Id == model.TransactionId);
                var redemptionParams = new RedemptionParams
                {
                    IsIssuedByCs = true,
                    TransferAmount = transaction.TotalAmount
                };
                var redemption = RedeemBonus(model.PlayerId, model.BonusId, redemptionParams);
                ProcessBonusRedemptionLifecycle(redemption);
                _repository.SaveChanges();

                redemption.Events.ForEach(_eventBus.Publish);
                scope.Complete();
            }
        }

        internal void ProcessFirstBonusRedemptionOfType(Player player, BonusType type)
        {
            var redemptionData =
                player.BonusesRedeemed
                .OrderBy(r => r.CreatedOn)
                .FirstOrDefault(r =>
                        r.Bonus.Template.Info.TemplateType == type &&
                        r.ActivationState == ActivationStatus.Pending);
            if (redemptionData != null)
            {
                var redemption = new Entities.BonusRedemption(redemptionData);
                ProcessBonusRedemptionLifecycle(redemption);
                redemption.Events.ForEach(_eventBus.Publish);
            }
        }
        internal void ProcessHighDepositBonus(Player player)
        {
            SendHighDepositBonusSmsNotifications(player);
            while (_bonusQueries.GetQualifiedBonuses(player, BonusType.HighDeposit).Any())
            {
                var bonusData = _bonusQueries.GetQualifiedBonuses(player, BonusType.HighDeposit).First();
                var redemption = RedeemBonus(player.Data.Id, bonusData.Id);
                ProcessBonusRedemptionLifecycle(redemption);
                redemption.Events.ForEach(_eventBus.Publish);
            }
        }
        internal void ActivateFundInBonus(Guid playerId, Guid bonusId, RedemptionParams redemptionParams)
        {
            var redemption = RedeemBonus(playerId, bonusId, redemptionParams);
            ProcessBonusRedemptionLifecycle(redemption);
            _repository.SaveChanges();

            redemption.Events.ForEach(_eventBus.Publish);
        }
        internal Entities.BonusRedemption RedeemBonus(Guid playerId, Guid bonusId, RedemptionParams redemptionParams = null)
        {
            var bonus = _repository.GetLockedBonus(bonusId);
            var player = _repository.GetLockedPlayer(playerId);

            var redemption = player.Redeem(bonus, redemptionParams);
            _repository.SaveChanges();

            return redemption;
        }
        internal void ProcessBonusRedemptionLifecycle(Entities.BonusRedemption redemption)
        {
            if (redemption.Data.Bonus.Template.Wagering.IsAfterWager)
            {
                if (redemption.Data.Bonus.Template.Info.Mode != IssuanceMode.ManualByPlayer || redemption.Data.ActivationState == ActivationStatus.Pending)
                {
                    var qualificationErrors = redemption.QualifyForActivation();
                    if (qualificationErrors.Any() == false)
                    {
                        redemption.IssueWagering();
                        SendWageringRequirementNotifications(redemption.Data);
                    }
                    else
                    {
                        redemption.Negate(qualificationErrors);
                    }
                }
            }
            else
            {
                ProceedToClaim(redemption);
            }
        }
        internal void WageringFulfilled(Entities.BonusRedemption redemption)
        {
            if (redemption.Data.Bonus.Template.Wagering.IsAfterWager)
            {
                ProceedToClaim(redemption);
            }

            redemption.IssueUnlock();
        }

        private void ProceedToClaim(Entities.BonusRedemption redemption)
        {
            var qualificationErrors = redemption.QualifyForActivation();
            if (qualificationErrors.Any() == false)
            {
                redemption.MakeClaimable();

                if (redemption.Data.Bonus.Template.Info.Mode != IssuanceMode.ManualByPlayer)
                {
                    ClaimBonusRedemption(redemption);
                }
            }
            else
            {
                redemption.Negate(qualificationErrors);
            }
        }

        private void ClaimBonusRedemption(Entities.BonusRedemption redemption)
        {
            var qualificationErrors = redemption.QualifyForClaiming();
            if (qualificationErrors.Any() == false)
            {
                var transaction = redemption.Claim();
                _brandOperations.FundIn(redemption.Data.Player.Id, transaction.TotalAmount, redemption.Data.Player.CurrencyCode, transaction.Id.ToString());

                if (redemption.Data.Bonus.Template.Wagering.IsAfterWager == false)
                    redemption.IssueWagering();

                SendWageringRequirementNotifications(redemption.Data);
                SendBonusIssuedNotifications(redemption.Data);
            }
            else
            {
                redemption.Negate(qualificationErrors);
            }
        }

        #region Notifications
        private void SendHighDepositBonusSmsNotifications(Player player)
        {
            var highDepositBonuses = _bonusQueries.GetCurrentVersionBonuses(player.Data.Brand.Id)
                .Where(x => x.Template.Info.TemplateType == BonusType.HighDeposit)
                .ToList()
                .Select(b => new Entities.Bonus(b));

            foreach (var bonus in highDepositBonuses)
            {
                var bonusRewardThreshold = bonus.CalculateRewardThreshold(player);

                if (bonusRewardThreshold == null)
                    continue;

                var model = new HighDepositReminderModel
                {
                    Currency = player.Data.CurrencyCode,
                    BonusAmount = bonusRewardThreshold.BonusAmount,
                    RemainingAmount = bonusRewardThreshold.RemainingAmount,
                    DepositAmountRequired = bonusRewardThreshold.DepositAmountRequired
                };

                _serviceBus.PublishMessage(new SendPlayerAMessage
                {
                    PlayerId = player.Data.Id,
                    MessageType = MessageType.HighDepositReminder,
                    MessageDeliveryMethod = MessageDeliveryMethod.Sms,
                    Model = model
                });
            }
        }

        private void SendBonusIssuedNotifications(BonusRedemption redemption)
        {
            redemption.Bonus.Template.Notification.Triggers
                .Where(t => t.MessageType == MessageType.BonusIssued)
                .ForEach(
                    t => _serviceBus.PublishMessage(new SendPlayerAMessage
                    {
                        PlayerId = redemption.Player.Id,
                        MessageType = MessageType.BonusIssued,
                        MessageDeliveryMethod = t.TriggerType == TriggerType.Email ? MessageDeliveryMethod.Email : MessageDeliveryMethod.Sms,
                        Model = new BonusIssuedModel { Amount = redemption.Amount }
                    }));
        }

        private void SendWageringRequirementNotifications(BonusRedemption redemption)
        {
            redemption.Bonus.Template.Notification.Triggers
                .Where(t => t.MessageType == MessageType.BonusWageringRequirement)
                .ForEach(
                    t => _serviceBus.PublishMessage(new SendPlayerAMessage
                    {
                        PlayerId = redemption.Player.Id,
                        MessageType = MessageType.BonusWageringRequirement,
                        MessageDeliveryMethod = t.TriggerType == TriggerType.Email ? MessageDeliveryMethod.Email : MessageDeliveryMethod.Sms,
                        Model = new BonusWageringRequirementModel
                        {
                            RequiredWagerAmount = redemption.Rollover,
                            BonusAmount = redemption.Amount,
                            IsAfterWager = redemption.Bonus.Template.Wagering.IsAfterWager
                        }
                    }));
        }
        #endregion
    }
}