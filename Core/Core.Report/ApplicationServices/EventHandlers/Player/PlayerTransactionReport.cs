using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Events.Wallet;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Core.Game.Interface.Data;
using TransactionType = AFT.RegoV2.Bonus.Core.Models.Enums.TransactionType;

namespace AFT.RegoV2.ApplicationServices.Report.EventHandlers
{
    public class PlayerTransactionReportEventHandlers
    {
        private readonly Func<PlayerQueries> _playerQueriesFactory;
        private readonly Func<IAdminQueries> _adminQueriesFactory;
        private readonly Func<GameQueries> _gameQueriesFactory;
        private readonly Func<IReportRepository> _reportRepositoryFactory;

        public PlayerTransactionReportEventHandlers(
            Func<PlayerQueries> playerQueriesFactory,
            Func<IAdminQueries> adminQueriesFactory,
            Func<GameQueries> gameQueriesFactory,
            Func<IReportRepository> reportRepositoryFactory)
        {
            _playerQueriesFactory = playerQueriesFactory;
            _adminQueriesFactory = adminQueriesFactory;
            _gameQueriesFactory = gameQueriesFactory;
            _reportRepositoryFactory = reportRepositoryFactory;
        }

        public void Handle(BonusWalletBalanceChanged processedEvent)
        {

            if ( !IsValidForStatsEvent(processedEvent))
                return;

            var reportRepository = _reportRepositoryFactory();
            var record = reportRepository.PlayerTransactionRecords.FirstOrDefault(r => r.TransactionId == processedEvent.TransactionId.ToString());

            if (record != null)
                throw new RegoException($"Player transaction record {processedEvent.TransactionId} already exists");

            record = new PlayerTransactionRecord
            {
                // wallet transaction id
                TransactionId = processedEvent.TransactionId.ToString(),
                // related wallet's transaction id
                RelatedTransactionId = processedEvent.RelatedTransactionId,

                PlayerId = processedEvent.Wallet.PlayerId,

                CreatedOn = processedEvent.CreatedOn,
                PerformedBy = GetNamePerformedBy(processedEvent.PerformedBy),

                RoundId = processedEvent.RoundId,
                GameId = processedEvent.GameId,

                Type = processedEvent.Type.ToString(),

                Balance = processedEvent.Wallet.Balance,
                MainBalance = processedEvent.Wallet.Balance,

                MainBalanceAmount = processedEvent.MainBalanceAmount,
                BonusBalanceAmount = processedEvent.BonusBalanceAmount,

                CurrencyCode = processedEvent.Wallet.CurrencyCode,

                Description = GetDescription(processedEvent),
                TransactionNumber = processedEvent.TransactionNumber
            };

            reportRepository.PlayerTransactionRecords.Add(record);
            reportRepository.SaveChanges();
        }

        private static bool IsValidForStatsEvent(BonusWalletBalanceChanged @event)
        {
            // we don't handle Bet Lost events in the stats. AFTREGO-4445
            return @event.Type != TransactionType.BetLost;
        }

        private string GetNamePerformedBy(Guid? id)
        {
            //todo: until DomainEventBase changed
            if (!id.HasValue)
                return "System";

            var player = _playerQueriesFactory().GetPlayer(id.Value);
            if (player != null)
                return "Player";

            var user = _adminQueriesFactory().GetAdminById(id.Value);
            if (user != null)
                return "Admin: " + user.Username;

            return id.ToString();
        }

        private string GetDescription(BonusWalletBalanceChanged processedEvent)
        {
            var gameId = processedEvent.GameId;
            if (!gameId.HasValue)
                return string.Empty;

            var game = ((IGameQueries)_gameQueriesFactory()).GetGameDto(gameId.Value);
            var gameName = game == null ? "Game" : game.Name;
            var gameActionDescription = GetGameActionDescription(processedEvent);
            return $"{gameName}, {gameActionDescription}";
        }

        private string GetGameActionDescription(BonusWalletBalanceChanged processedEvent)
        {
            switch (processedEvent.Type)
            {
                case Bonus.Core.Models.Enums.TransactionType.BetPlaced:
                    return "Place Bet";
                case Bonus.Core.Models.Enums.TransactionType.BetFree:
                    return "Free Bet";
                case Bonus.Core.Models.Enums.TransactionType.BetWon:
                    return "Win Bet";
                case Bonus.Core.Models.Enums.TransactionType.BetLost:
                    return "Lose Bet";
                case Bonus.Core.Models.Enums.TransactionType.BetCancelled:
                    return "Cancel Tx";
                case Bonus.Core.Models.Enums.TransactionType.BetWonAdjustment:
                    return "Win Bet Adjustment, " + (processedEvent.MainBalanceAmount + processedEvent.BonusBalanceAmount).ToString("F") + " ";
                default:
                    return string.Empty;
            }
        }
    }
}
