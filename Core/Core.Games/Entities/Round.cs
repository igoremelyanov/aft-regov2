using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Interface.Exceptions;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interface.Events;
// ReSharper disable MergeConditionalExpression

namespace AFT.RegoV2.Core.Game.Entities
{
    public class Round
    {
        public Interface.Data.Round Data { get; }
        public List<IDomainEvent> Events { get; }

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
            Events = new List<IDomainEvent>();
        }

        public Round(Interface.Data.Round data) : this()
        {
            Data = data;
        }

        public Round(string externalBetId, Guid gameId, Guid playerId, Interface.Data.Brand brand) : this()
        {
            Data = new Interface.Data.Round
            {
                Id = Guid.NewGuid(),
                Status = RoundStatus.New,
                ExternalRoundId = externalBetId,
                PlayerId = playerId,
                GameId = gameId,
                BrandId = brand.Id,
                Brand = brand,
                CreatedOn = DateTimeOffset.UtcNow.ToBrandOffset(brand.TimezoneId),
                GameActions = new List<GameAction>()
            };
        }

        public Guid Place(GameActionData gameActionData, GameActionContext context)
        {
            if (gameActionData.TransactionReferenceId != null)
                ValidateAsSecondaryAction(gameActionData.TransactionReferenceId);

            ValidatePositiveAmount(gameActionData.Amount);

            var gameActionTransactionId = GenerateGameAction<BetPlaced>(GameActionType.Placed, gameActionData, context);

            Data.Status = RoundStatus.Open;

            return gameActionTransactionId;
        }


        public Guid Win(GameActionData gameActionData, GameActionContext context)
        {
            ValidateAsSecondaryAction(gameActionData.TransactionReferenceId);

            ValidatePositiveAmount(gameActionData.Amount);

            var gameActionTransactionId = GenerateGameAction<BetWon>(GameActionType.Won, gameActionData, context);

            CloseRound();

            return gameActionTransactionId;
        }

        public Guid Free(GameActionData gameActionData, GameActionContext context)
        {
            if (gameActionData.TransactionReferenceId != null)
                ValidateAsSecondaryAction(gameActionData.TransactionReferenceId);

            ValidateNonNegativeAmount(gameActionData.Amount);

            var gameActionTransactionId = GenerateGameAction<BetPlacedFree>(GameActionType.Free, gameActionData, context);

            CloseRound();

            return gameActionTransactionId;
        }

        public Guid Adjust(GameActionData gameActionData, GameActionContext context)
        {
            ValidateAsSecondaryAction(gameActionData.TransactionReferenceId);

            return GenerateGameAction<BetAdjusted>(GameActionType.Adjustment, gameActionData, context);
        }

        public Guid Lose(GameActionData gameActionData, GameActionContext context)
        {
            ValidateAsSecondaryAction(gameActionData.TransactionReferenceId);


            var gameActionTransactionId = GenerateGameAction<BetLost>(GameActionType.Lost, gameActionData, context);

            CloseRound();
            
            return gameActionTransactionId;
        }

        public Guid Tie(GameActionData gameActionData, GameActionContext context)
        {
            ValidateAsSecondaryAction(gameActionData.TransactionReferenceId);

            var gameActionTransactionId = GenerateGameAction<BetTied>(GameActionType.Tied, gameActionData, context);

            CloseRound();

            return gameActionTransactionId;
        }

        public Guid Cancel(GameActionData gameActionData, GameActionContext context)
        {
            ValidateAsSecondaryAction(gameActionData.TransactionReferenceId);

            return GenerateGameAction<BetCancelled>(GameActionType.Cancel, gameActionData, context);
        }

        private Guid GenerateGameAction<TEvent>(GameActionType gameActionType, GameActionData gameActionData, GameActionContext context)
            where
                TEvent : GameActionEventBase, new()
        {
            if (gameActionData.ExternalTransactionId == null)
                throw new ArgumentNullException("externalTransactionId");

            var gameActionId = Guid.NewGuid();

            var amount = gameActionData.Amount;

            if (gameActionType == GameActionType.Placed)
                amount = -amount;

            Data.GameActions.Add(new GameAction
            {
                Id = gameActionId,
                ExternalTransactionId = gameActionData.ExternalTransactionId,
                ExternalBetId = gameActionData.ExternalBetId,
                ExternalTransactionReferenceId = gameActionData.TransactionReferenceId,
                Round = Data,
                GameActionType = gameActionType,
                Amount = amount,
                Description = gameActionData.Description,
                WalletTransactionId = gameActionData.WalletTransactionId,
                Timestamp = DateTimeOffset.UtcNow.ToBrandOffset(Data.Brand.TimezoneId),
                Context = context
            });

            var relatedGameAction = GetGameActionByReferenceId(gameActionData.TransactionReferenceId);
            
            Events.Add(new TEvent
            {
                Amount = gameActionData.Amount,
                GameActionId = gameActionId,
                BrandId = Data.BrandId,
                GameId = Data.GameId,
                PlayerId = Data.PlayerId,
                RoundId = Data.Id,
                Turnover = context.TurnoverContribution,
                Ggr = context.GgrContribution,
                UnsettledBets = context.UnsettledBetsContribution,
                RelatedGameActionId = relatedGameAction == null? (Guid?) null : relatedGameAction.Id,
                CreatedOn = DateTimeOffset.UtcNow.ToBrandOffset(Data.Brand.TimezoneId)
            });
            return gameActionId;
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

        public GameAction GetGameActionByReferenceId(string transactionReferenceId)
        {
            return Data.GameActions.SingleOrDefault(x => x.ExternalTransactionId == transactionReferenceId);
        }

    }



}