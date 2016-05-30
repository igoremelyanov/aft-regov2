using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using Microsoft.Practices.Unity;
using Wallet = AFT.RegoV2.Bonus.Core.Entities.Wallet;

namespace AFT.RegoV2.Bonus.Core.EventHandlers
{
    public class GameSubscriber
    {
        private readonly IUnityContainer _container;
        private const string NoGameFormatter = "Game does not exist: {0}";

        public GameSubscriber(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(GameCreated @event)
        {
            var repository = _container.Resolve<IBonusRepository>();

            repository.Games.Add(new Game
            {
                Id = @event.Id,
                ProductId = @event.GameProviderId
            });
            repository.SaveChanges();
        }

        public void Handle(GameUpdated @event)
        {
            var repository = _container.Resolve<IBonusRepository>();
            var game = repository.Games.SingleOrDefault(g => g.Id == @event.Id);
            if (game == null)
                throw new RegoException(string.Format(NoGameFormatter, @event.Id));

            game.ProductId = @event.GameProviderId;
            repository.SaveChanges();
        }

        public void Handle(GameDeleted @event)
        {
            var repository = _container.Resolve<IBonusRepository>();
            var game = repository.Games.SingleOrDefault(g => g.Id == @event.Id);
            if (game == null)
                throw new RegoException(string.Format(NoGameFormatter, @event.Id));

            repository.RemoveGameContributionsForGame(@event.Id);
            repository.Games.Remove(game);
            repository.SaveChanges();
        }

        public void Handle(BetPlaced @event)
        {
            EngageWalletChanges(
                @event.PlayerId,
                (wallet, _) => wallet.PlaceBet(@event.Amount, @event.RoundId, @event.GameId, @event.GameActionId)
                );
        }

        public void Handle(BetPlacedFree @event)
        {
            EngageWalletChanges(
                @event.PlayerId,
                (wallet, repository) =>
                {
                    var transaction = wallet.FreeBet(@event.Amount, @event.RoundId, @event.GameId, @event.GameActionId);
                    HandlePositiveTurnover(@event, repository, wallet, transaction);
                });
        }

        public void Handle(BetTied @event)
        {
            EngageWalletChanges(
                @event.PlayerId,
                (wallet, repository) =>
                {
                    var transaction = wallet.WinBet(@event.RoundId, @event.Amount, @event.GameActionId);
                    transaction.Type = TransactionType.BetTied;
                    HandlePositiveTurnover(@event, repository, wallet, transaction);
                });
        }

        public void Handle(BetCancelled @event)
        {
            EngageWalletChanges(
               @event.PlayerId,
               (wallet, repository) =>
               {
                   var cancelTransaction = wallet.Data.Transactions.Single(x => x.GameActionId == @event.RelatedGameActionId);
                   var transaction = wallet.CancelBet(cancelTransaction.Id, @event.GameActionId);
                   if (@event.Turnover < 0m)
                       HandleNegativeTurnover(@event, repository, transaction);
               });
        }

        public void Handle(BetAdjusted @event)
        {
            EngageWalletChanges(@event.PlayerId,
                (wallet, _) =>
                {
                    var adjustTransactionId = wallet.Data.Transactions.Single(x => x.GameActionId == @event.RelatedGameActionId).Id;
                    wallet.AdjustTransaction(adjustTransactionId, @event.Amount, @event.GameActionId);
                });
        }

        public void Handle(BetWon @event)
        {
            EngageWalletChanges(
               @event.PlayerId,
               (wallet, repository) =>
               {
                   var transaction = wallet.WinBet(@event.RoundId, @event.Amount, @event.GameActionId);
                   HandlePositiveTurnover(@event, repository, wallet, transaction);
               });
        }

        public void Handle(BetLost @event)
        {
            EngageWalletChanges(
               @event.PlayerId,
               (wallet, repository) =>
               {
                   var transaction = wallet.LoseBet(@event.RoundId, @event.GameActionId);
                   HandlePositiveTurnover(@event, repository, wallet, transaction);
               });
        }


        private void HandlePositiveTurnover(GameActionEventBase @event, IBonusRepository repository, Wallet wallet, Transaction transaction)
        {
            var eventBus = _container.Resolve<IEventBus>();
            var bonusCommands = _container.Resolve<BonusCommands>();

            var player = repository.GetLockedPlayer(@event.PlayerId);
            var redemptionsWithActiveRollover = player.GetRedemptionsWithActiveRollover(wallet.Data.Template.Id);
            var turnoverLeftToDistribute = @event.Turnover;
            foreach (var redemption in redemptionsWithActiveRollover)
            {
                var handledAmount = redemption.FulfillRollover(turnoverLeftToDistribute, transaction);
                turnoverLeftToDistribute -= handledAmount;

                if (redemption.RolloverLeft == 0m)
                {
                    redemption.CompleteRollover();
                    bonusCommands.WageringFulfilled(redemption);
                }

                if (turnoverLeftToDistribute == 0m)
                {
                    if (redemption.WageringThresholdIsMet(wallet.TotalBalance) &&
                        // Check if rollover is still Active, 'cos it can become Completed several lines before
                        redemption.Data.RolloverState == RolloverStatus.Active)
                    {
                        redemption.ZeroOutRollover(transaction);
                        bonusCommands.WageringFulfilled(redemption);
                    }

                    redemption.Events.ForEach(eventBus.Publish);
                    break;
                }

                redemption.Events.ForEach(eventBus.Publish);
            }

            player.Data.AccumulatedWageringAmount += @event.Turnover;
            if (player.CompletedReferralRequirements())
            {
                var referrer = repository.GetLockedPlayer(player.Data.ReferredBy.Value);
                bonusCommands.ProcessFirstBonusRedemptionOfType(referrer, BonusType.ReferFriend);
                player.CompleteReferralRequirements();
            }
        }

        private void HandleNegativeTurnover(GameActionEventBase @event, IBonusRepository repository, Transaction transaction)
        {
            var eventBus = _container.Resolve<IEventBus>();

            var player = repository.GetLockedPlayer(@event.PlayerId);
            var bonusRedemptions = player.GetRedemptionsWithActiveRollover()
                .Where(br => br.Data.Contributions.Select(c => c.Transaction.Id).Contains(transaction.RelatedTransactionId.Value));

            foreach (var bonusRedemption in bonusRedemptions)
            {
                bonusRedemption.FulfillRollover(@event.Turnover, transaction);
                bonusRedemption.Events.ForEach(eventBus.Publish);
            }
        }

        private void EngageWalletChanges(Guid playerId, Action<Wallet, IBonusRepository> eventHandler)
        {
            var repository = _container.Resolve<IBonusRepository>();
            var eventBus = _container.Resolve<IEventBus>();

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallet = repository.GetLockedWallet(playerId);

                eventHandler(wallet, repository);

                wallet.Events.ForEach(eventBus.Publish);

                repository.SaveChanges();
                scope.Complete();
            }
        }
    }
}