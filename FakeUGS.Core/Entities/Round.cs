using System;
using System.Collections.Generic;
using System.Linq;
using FakeUGS.Core.Data;
using FakeUGS.Core.Exceptions;
using FakeUGS.Core.Extensions;
using AFT.UGS.Core.BaseModels.Bus;

namespace FakeUGS.Core.Entities
{
    public class Round
    {
        private readonly List<BusEvent> _events;
        public List<BusEvent> Events => _events;

        public Data.Round Data { get; }

        public decimal Amount
        {
            get { return -Data.GameActions.Where(x => x.GameActionType == GameActionType.Placed).Sum(x => x.Amount); }
        }

        public decimal WonAmount
        {
            get
            {
                return Data.GameActions.Where(x =>
                    x.GameActionType == GameActionType.Won || x.GameActionType == GameActionType.Free
                    ).Sum(x => x.Amount);
            }
        }

        public decimal AdjustedAmount
        {
            get
            {
                return Data.GameActions.Where(x =>
                    x.GameActionType == GameActionType.Adjustment || x.GameActionType == GameActionType.Cancel)
                    .Sum(x => x.Amount);
            }
        }

        public Round()
        {
            _events = new List<BusEvent>();
        }

        public Round(Data.Round data) : this()
        {
            Data = data;
        }

        public Round(string externalBetId, Guid gameId, Guid playerId, Brand brand) : this()
        {
            Data = new Data.Round
            {
                Id = Guid.NewGuid(),
                Status = RoundStatus.New,
                ExternalRoundId = externalBetId,
                PlayerId = playerId,
                GameId = gameId,
                BrandId = brand.Id,
                BrandCode = brand.Code,
                Brand = brand,
                CreatedOn = DateTimeOffset.UtcNow.ToBrandOffset(brand.TimezoneId),
                GameActions = new List<GameAction>()
            };
        }

        public Guid Place(decimal amount, 
            string description, 
            Guid walletTransactionId, 
            string externalTransactionId, 
            string externalBetId,
            string externalTransactionReferenceId = null)
        {
            if (externalTransactionReferenceId != null)
                ValidateAsSecondaryAction(externalTransactionReferenceId);

            ValidatePositiveAmount(amount);

            var gameAction = CreateGameAction(amount, 
                GameActionType.Placed, 
                description, 
                walletTransactionId, 
                externalTransactionId, 
                externalBetId,
                externalTransactionReferenceId, null);

            Data.Status = RoundStatus.Open;

            CreateGameActionEvent(gameAction);

            return gameAction.Id;
        }


        public Guid Win(decimal amount, 
            string description, 
            Guid walletTransactionId, 
            string externalTransactionId,
            string externalBetId,
            string externalTransactionReferenceId,
            string batchId = null)
        {
            ValidateAsSecondaryAction(externalTransactionReferenceId);

            ValidatePositiveAmount(amount);

            var gameAction = CreateGameAction(amount, 
                GameActionType.Won, 
                description, 
                walletTransactionId, 
                externalTransactionId,
                externalBetId,
                externalTransactionReferenceId,
                batchId);

            CreateGameActionEvent(gameAction);

            CloseRound();

            return gameAction.Id;
        }

        public Guid Free(decimal amount, 
            string description, 
            Guid walletTransactionId, 
            string externalTransactionId,
            string externalBetId,
            string externalTransactionReferenceId = null,
            string batchId = null            )
        {
            if (externalTransactionReferenceId != null)
                ValidateAsSecondaryAction(externalTransactionReferenceId);

            ValidateNonNegativeAmount(amount);

            var gameAction = CreateGameAction(amount, 
                GameActionType.Free, 
                description, 
                walletTransactionId, 
                externalTransactionId,
                externalBetId,
                externalTransactionReferenceId,
                batchId);

            CreateGameActionEvent(gameAction);

            CloseRound();

            return gameAction.Id;
        }

        public Guid Adjust(decimal amount, 
            string description, 
            Guid walletTransactionId, 
            string externalTransactionId,
            string externalBetId,
            string externalTransactionReferenceId, 
            string batchId = null)
        {
            ValidateAsSecondaryAction(externalTransactionReferenceId);

            var gameAction = CreateGameAction(amount, 
                GameActionType.Adjustment, 
                description, 
                walletTransactionId, 
                externalTransactionId,
                externalBetId,
                externalTransactionReferenceId, 
                batchId);

            CreateGameActionEvent(gameAction);
            return gameAction.Id;
        }

        public Guid Lose(string description, 
            string externalTransactionId,
            string externalBetId,
            string externalTransactionReferenceId, 
            string batchId = null)
        {
            ValidateAsSecondaryAction(externalTransactionReferenceId);

            var gameAction = CreateGameAction(0, 
                GameActionType.Lost, 
                description, 
                Guid.Empty, 
                externalTransactionId,
                externalBetId,
                externalTransactionReferenceId,
                batchId);

            CloseRound();

            CreateGameActionEvent(gameAction);
            return gameAction.Id;
        }

        public Guid Tie(string description,
            Guid walletTransactionId,
            string externalTransactionId,
            string externalBetId,
            string externalTransactionReferenceId,
            string batchId = null)
        {
            ValidateAsSecondaryAction(externalTransactionReferenceId);

            var gameAction = CreateGameAction(0,
                GameActionType.Tied,
                description,
                walletTransactionId,
                externalTransactionId,
                externalBetId,
                externalTransactionReferenceId,
                batchId);

            CloseRound();

            CreateGameActionEvent(gameAction);
            return gameAction.Id;

        }

        public Guid Cancel(decimal amount, 
            string description, 
            Guid walletTransactionId, 
            string externalTransactionId,
            string externalBetId,
            string externalTransactionReferenceId, 
            string batchId = null)
        {
            ValidateAsSecondaryAction(externalTransactionReferenceId);

            var gameAction = CreateGameAction(amount, 
                GameActionType.Cancel, 
                description, walletTransactionId,
                externalTransactionId,
                externalBetId,
                externalTransactionReferenceId, 
                batchId);

            CreateGameActionEvent(gameAction);
            return gameAction.Id;

        }

        private GameAction CreateGameAction(decimal amount, 
            GameActionType gameActionType, 
            string description, 
            Guid walletTransactionId, 
            string externalTransactionId,
            string externalBetId,
            string externalTransactionReferenceId,
            string batchId)
        {
            if (externalTransactionId == null)
                throw new ArgumentNullException(nameof(externalTransactionId));
            
            if (gameActionType == GameActionType.Placed)
                amount = -amount;

            var relatedGameAction =
                Data.GameActions.SingleOrDefault(x => x.ExternalTransactionId == externalTransactionReferenceId);

            var relatedGameActionId = Guid.Empty;

            if (relatedGameAction != null)
                relatedGameActionId = relatedGameAction.Id;

            var gameAction = new GameAction
            {
                Id = Guid.NewGuid(),
                ExternalTransactionId = externalTransactionId,
                ExternalBetId = externalBetId,
                ExternalTransactionReferenceId = externalTransactionReferenceId,
                RelatedGameActionId = relatedGameActionId,
                Round = Data,
                GameActionType = gameActionType,
                Amount = amount,
                Description = description,
                WalletTransactionId = walletTransactionId,
                ExternalBatchId = batchId,
                Timestamp = DateTimeOffset.UtcNow.ToBrandOffset(Data.Brand.TimezoneId)
            };
            Data.GameActions.Add(gameAction);

            return gameAction;
        }

        private void CloseRound()
        {
            Data.Status = RoundStatus.Closed;
            Data.ClosedOn = DateTimeOffset.UtcNow.ToBrandOffset(Data.Brand.TimezoneId);
        }

        private static void ValidatePositiveAmount(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be a positive number.");
        }

        private static void ValidateNonNegativeAmount(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be either 0 or a positive number.");
        }

        private void ValidateAsSecondaryAction(string extTxRefId)
        {
            if (Data.GameActions.All(x => x.ExternalTransactionId != extTxRefId))
                throw new GameActionNotFoundException();
        }

        public GameAction GetGameActionByReferenceId(string externalTransactionReferenceId)
        {
            return  Data.GameActions.SingleOrDefault(x => x.ExternalTransactionId == externalTransactionReferenceId);
        }

        private void CreateGameActionEvent(GameAction gameAction)
        {
            Events.Add(new GameEvent
            {
                amount = gameAction.Amount,
                type = MapGameActionType(gameAction),
                externaltxid = gameAction.ExternalTransactionId,
                externaltxrefid = gameAction.ExternalTransactionReferenceId,
                externalbetid= gameAction.ExternalBetId,
                userid = Data.PlayerId.ToString(),
                unsettledbets = 0,
                ggr = 0,
                turnover = gameAction.GameActionType == GameActionType.Cancel ? -Amount : Amount,
                istestplayer = true, // todo: lookup the actual player's "test" status
            });
        }

        private BusEventType MapGameActionType(GameAction gameAction)
        {
            if ( gameAction.GameActionType == GameActionType.Placed)
                return BusEventType.BetPlaced;

            if (gameAction.GameActionType == GameActionType.Free)
                return BusEventType.BetFree;

            if (gameAction.GameActionType == GameActionType.Adjustment)
                return BusEventType.BetAdjusted;

            if (gameAction.GameActionType == GameActionType.Cancel)
                return BusEventType.GameActionCancelled;

            if (gameAction.GameActionType == GameActionType.Won)
                return BusEventType.BetWon;

            if (gameAction.GameActionType == GameActionType.Lost)
                return BusEventType.BetLost;

            if (gameAction.GameActionType == GameActionType.Tied)
                return BusEventType.BetTied;

            throw new Exception("This cannot happen.");
        }
    }
}