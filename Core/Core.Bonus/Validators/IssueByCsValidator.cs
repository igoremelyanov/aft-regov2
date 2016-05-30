using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Resources;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using FluentValidation;
using Player = AFT.RegoV2.Bonus.Core.Data.Player;

namespace AFT.RegoV2.Bonus.Core.Validators
{
    internal class IssueByCsValidator : AbstractValidator<IssueBonusByCs>
    {
        public IssueByCsValidator(BonusQueries queries, IBonusRepository repository, IBrandOperations brandOperations)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;
            Func<Guid, Data.Bonus> bonusGetter = bonusId => queries.GetCurrentVersionBonuses().SingleOrDefault(b => b.Id == bonusId);
            Func<Guid, Player> playerGetter = playerId => repository.Players.SingleOrDefault(b => b.Id == playerId);
            Func<Guid, Guid, Transaction> transactionGetter = (playerId, transactionId) =>
                playerGetter(playerId).Wallets.SelectMany(w => w.Transactions).SingleOrDefault(t => t.Id == transactionId);

            RuleFor(model => model.BonusId)
                .Must(id => bonusGetter(id) != null)
                .WithMessage(ValidatorMessages.BonusDoesNotExist);

            RuleFor(model => model.PlayerId)
                .Must(id => playerGetter(id) != null)
                .WithMessage(ValidatorMessages.PlayerDoesNotExist);

            When(model => playerGetter(model.PlayerId) != null && bonusGetter(model.BonusId) != null, () =>
            {
                RuleFor(model => model.TransactionId)
                    .Must((model, transactionId) => transactionGetter(model.PlayerId, transactionId) != null)
                    .WithMessage(ValidatorMessages.TransactionDoesNotExist);

                When(model => transactionGetter(model.PlayerId, model.TransactionId) != null, () => RuleFor(model => model.TransactionId)
                    .Must((model, transactionId) =>
                    {
                        var transaction = transactionGetter(model.PlayerId, transactionId);
                        var bonus = bonusGetter(model.BonusId);

                        if (bonus.Template.Info.TemplateType == BonusType.FundIn)
                        {
                            return transaction.Type == TransactionType.FundIn;
                        }
                        if (bonus.Template.Info.TemplateType == BonusType.FirstDeposit || bonus.Template.Info.TemplateType == BonusType.ReloadDeposit)
                        {
                            return transaction.Type == TransactionType.Deposit;
                        }

                        return true;
                    })
                    .WithMessage(ValidatorMessages.TransactionTypeDoesNotMatchBonusType)
                    .Must((model, transactionId) =>
                    {
                        var qualifiedBonuses = queries.GetManualByCsQualifiedBonuses(model.PlayerId);
                        var theQualifedBonus = qualifiedBonuses.SingleOrDefault(b => b.Id == model.BonusId);
                        if (theQualifedBonus == null)
                            return false;
                        var qualifiedTransactions = queries.GetManualByCsQualifiedTransactions(model.PlayerId, model.BonusId);
                        if (qualifiedTransactions.Select(tr => tr.Id).Contains(model.TransactionId) == false)
                            return false;

                        return true;
                    })
                    .WithMessage(ValidatorMessages.PlayerIsNotQualifiedForBonus)
                    .Must((model, transactionId) =>
                    {
                        var bonus = bonusGetter(model.BonusId);
                        if (bonus.Template.Wagering.HasWagering == false)
                            return true;

                        var bonusWallet = playerGetter(model.PlayerId).Wallets.Single(w => w.Transactions.Select(t => t.Id).Contains(transactionId));
                        var transaction = transactionGetter(model.PlayerId, transactionId);

                        return bonusWallet.Main >= transaction.TotalAmount;
                    })
                    .WithMessage(ValidatorMessages.PlayerHasNoFundsToLockLeft)
                    .Must((model, transactionId) =>
                    {
                        var bonus = bonusGetter(model.BonusId);
                        if (bonus.Template.Wagering.HasWagering == false)
                            return true;

                        var player = playerGetter(model.PlayerId);
                        var balance = brandOperations.GetPlayerBalance(model.PlayerId, player.CurrencyCode);
                        return balance >= bonus.Template.Wagering.Threshold;
                    })
                    .WithMessage(ValidatorMessages.PlayerBalanceIsLessThanWageringThreshold));
            });
        }
    }
}