using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

using AFT.RegoV2.Core.Common.Attributes;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AFT.UGS.Core.BaseModels.Bus;
using FakeUGS.Core.Data;
using FakeUGS.Core.Exceptions;
using FakeUGS.Core.Interfaces;
using FakeUGS.Core.Providers;

namespace FakeUGS.Core.ApplicationServices
{
    public sealed class GameCommands : MarshalByRefObject, IGameCommands
    {
        private readonly IGameEventsProcessor _gameEventsProcessor;
        private readonly IRepository _repository;
        private readonly IGameWalletOperations _walletOperations;
        private readonly IGameWalletQueries _walletQueries;

        public GameCommands(
            IRepository repository,
            IGameWalletOperations walletCommands,
            IGameWalletQueries walletQueries,
            IGameEventsProcessor gameEventsProcessor)
        {
            _gameEventsProcessor = gameEventsProcessor;
            _walletOperations = walletCommands;
            _walletQueries = walletQueries;
            _repository = repository;
        }
        
        /// <summary>
        /// Places a bet
        /// </summary>
        async Task<Guid> IGameCommands.PlaceBetAsync([NotNull] GameActionData actionData, [NotNull] GameActionContext context, Guid playerId)
        {

            var player = await _repository.Players.SingleAsync(x => x.Id == playerId);

            await ValidateBrand(player.BrandId);

            var gameProviderId = await ValidateGameProviderAsync(context.GameProviderCode);
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, gameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var game = GetGameByExternalGameId(actionData.ExternalGameId);

                var round = _repository.GetOrCreateRound(actionData.RoundId, game.Id, player.Id, player.BrandId);

                var walletTransactionId = await _walletOperations.PlaceBetAsync(player.Id, round.Data.GameId, round.Data.Id, actionData.Amount);

                var placeBetGameActionId = round.Place(
                    actionData.Amount, 
                    actionData.Description, 
                    walletTransactionId,
                    actionData.ExternalTransactionId,
                    actionData.ExternalBetId,
                    actionData.TransactionReferenceId);


                _repository.Rounds.AddOrUpdate(x => x.ExternalRoundId, round.Data);
                _repository.SaveChanges();

                await _gameEventsProcessor.Process(round.Events, context.PlayerToken, round.Data.BrandCode, actionData, context.GameProviderCode);
                
                scope.Complete();

                return placeBetGameActionId;
            }

        }

        private async Task<Guid> ValidateGameProviderAsync(string gameProviderCode)
        {
            var gameProvider = await _repository.GameProviders.SingleAsync(x => x.Code == gameProviderCode);

            return gameProvider.Id;
        }

        private async Task ValidateBrand(Guid brandId)
        {
            if ( await _repository.Brands.AnyAsync(x => x.Id == brandId) == false)
                throw new RegoException("Brand not found. " + brandId);
        }

        async Task<Guid> IGameCommands.WinBetAsync([NotNull]GameActionData actionData, [NotNull]GameActionContext context)
        {
            var gameProviderId = await ValidateGameProviderAsync(context.GameProviderCode);
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, gameProviderId);


            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var round = GetRound(actionData);

                var walletTransactionId = await _walletOperations.WinBetAsync(round.Data.PlayerId, round.Data.GameId, round.Data.Id, actionData.Amount);

                var winGameActionId = round.Win(
                    actionData.Amount, 
                    actionData.Description, 
                    walletTransactionId, 
                    actionData.ExternalTransactionId, 
                    actionData.ExternalBetId,
                    actionData.TransactionReferenceId,
                    actionData.BatchId);

                await _repository.SaveChangesAsync();

                await _gameEventsProcessor.Process(round.Events, context.PlayerToken, round.Data.BrandCode, actionData, context.GameProviderCode);

                scope.Complete();

                return winGameActionId;
            }
        }

        async Task<Guid> IGameCommands.LoseBetAsync([NotNull]GameActionData actionData, [NotNull]GameActionContext context)
        {
            if (actionData.Amount != 0)
            {
                throw new LoseBetAmountMustBeZeroException();
            }
            var gameProviderId = await ValidateGameProviderAsync(context.GameProviderCode);
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, gameProviderId);


            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var round = GetRound(actionData);

                await _walletOperations.LoseBetAsync(round.Data.PlayerId, round.Data.GameId, round.Data.Id);

                var loseGameActionId = round.Lose(
                    actionData.Description, 
                    actionData.ExternalTransactionId,
                    actionData.ExternalBetId,
                    actionData.TransactionReferenceId, 
                    actionData.BatchId);

                await _repository.SaveChangesAsync();

                await _gameEventsProcessor.Process(round.Events, context.PlayerToken, round.Data.BrandCode, actionData, context.GameProviderCode);

                scope.Complete();

                return loseGameActionId;
            }
        }

        async Task<Guid> IGameCommands.TieBetAsync([NotNull] GameActionData actionData,
            [NotNull] GameActionContext context)
        {
            var gameProviderId = await ValidateGameProviderAsync(context.GameProviderCode);
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, gameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var round = GetRound(actionData);

                var walletTransactionId = await _walletOperations.TieBetAsync(round.Data.PlayerId, round.Data.GameId, round.Data.Id, actionData.Amount);

                var tieGameActionId = round.Tie(
                    actionData.Description,
                    walletTransactionId,
                    actionData.ExternalTransactionId,
                    actionData.ExternalBetId,
                    actionData.TransactionReferenceId,
                    actionData.BatchId);

                await _repository.SaveChangesAsync();

                await _gameEventsProcessor.Process(round.Events, context.PlayerToken, round.Data.BrandCode, actionData, context.GameProviderCode);

                scope.Complete();

                return tieGameActionId;
            }
        }

        
        //
        // The idea of the "Free bet" is that game provider can let players win a bet without actually placing it
        //
        async Task<Guid> IGameCommands.FreeBetAsync([NotNull]GameActionData actionData, [NotNull]GameActionContext context, Guid? playerId)
        {
            var gameProviderId = await ValidateGameProviderAsync(context.GameProviderCode);
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, gameProviderId);


            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var game = GetGameByExternalGameId(actionData.ExternalGameId);

                var player = playerId == null ? null : await _repository.Players.SingleAsync(x => x.Id == playerId);
                var round = player == null
                    ? GetRound(actionData)
                    : _repository.GetOrCreateRound(actionData.RoundId, game.Id, player.Id, player.BrandId);

                var walletTransactionId = await _walletOperations.FreeBetAsync(round.Data.PlayerId, round.Data.GameId, round.Data.Id, actionData.Amount);

                var freeBetGameActionId = round.Free(
                    actionData.Amount, 
                    actionData.Description, 
                    walletTransactionId, 
                    actionData.ExternalTransactionId,
                    actionData.ExternalBetId);

                _repository.Rounds.AddOrUpdate(x => x.ExternalRoundId, round.Data);

                _repository.SaveChanges();

                await _gameEventsProcessor.Process(round.Events, context.PlayerToken, round.Data.BrandCode, actionData, context.GameProviderCode);

                scope.Complete();

                return freeBetGameActionId;
            }
        }
        async Task<Guid> IGameCommands.AdjustTransaction([NotNull]GameActionData actionData, [NotNull]GameActionContext context)
        {
            var gameProviderId = await ValidateGameProviderAsync(context.GameProviderCode);
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, gameProviderId);


            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var round = GetRound(actionData);

                var gameActionToAdjust = round.GetGameActionByReferenceId(actionData.TransactionReferenceId);

                if ( gameActionToAdjust == null) 
                    throw new GameActionNotFoundException();

                var walletTransactionId = _walletOperations.AdjustBetTransaction(round.Data.PlayerId, round.Data.GameId,
                    gameActionToAdjust.Id, actionData.Amount);

                var adjustmentGameActionId = AdjustRound(round, actionData, walletTransactionId);

                _repository.SaveChanges();

                await _gameEventsProcessor.Process(round.Events, context.PlayerToken, round.Data.BrandCode, actionData, context.GameProviderCode);

                scope.Complete();

                return adjustmentGameActionId;
            }
        }

        async Task<Guid> IGameCommands.CancelTransactionAsync([NotNull]GameActionData actionData, [NotNull]GameActionContext context)
        {
            var gameProviderId = await ValidateGameProviderAsync(context.GameProviderCode);
            ValidateTransactionIsUnique(actionData.ExternalTransactionId, gameProviderId);


            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var round = GetRound(actionData);

                var gameActionToCancel = round.GetGameActionByReferenceId(actionData.TransactionReferenceId);

                if (gameActionToCancel == null)
                    throw new GameActionNotFoundException();

                var walletTransactionId = await _walletOperations.CancelBetAsync(round.Data.PlayerId, round.Data.GameId, gameActionToCancel.WalletTransactionId);

                var amount = -gameActionToCancel.Amount;
                if (gameActionToCancel.Amount != amount)
                {
                    // TODO: raise an administrative event (amounts don't match when cancelling a transaction)   
                }

                var cancelGameActionId = CancelRound(round, amount, actionData, walletTransactionId);

                _repository.SaveChanges();

                await _gameEventsProcessor.Process(round.Events, context.PlayerToken, round.Data.BrandCode, actionData, context.GameProviderCode);

                scope.Complete();

                return cancelGameActionId;
            }
        }

        private Entities.Round GetRound(GameActionData actionData)
        {
            var round = _repository.GetRound(actionData.RoundId);
            if (round == null)
                throw new RoundNotFoundException();
            return round;
        }

        private Guid AdjustRound(Entities.Round round, GameActionData actionData, Guid walletTxId)
        {
            return round.Adjust(
                    actionData.Amount,
                    actionData.Description,
                    walletTxId,
                    actionData.ExternalTransactionId,
                    actionData.ExternalBetId,
                    actionData.TransactionReferenceId,
                    actionData.BatchId);
        }

        private Guid CancelRound(Entities.Round round, decimal amount, GameActionData actionData, Guid walletTxId)
        {
            return round.Cancel(
                amount,
                actionData.Description,
                walletTxId,
                actionData.ExternalTransactionId,
                actionData.ExternalBetId,
                actionData.TransactionReferenceId,
                actionData.BatchId);
        }

        // private

        private void ValidateTransactionIsUnique(string transactionId, Guid gameProviderId)
        {
            if (_repository.DoesGameActionExist(transactionId, gameProviderId))
            {
                var dupTx = FindGameAction(transactionId, gameProviderId);
                throw new DuplicateGameActionException(dupTx.Id);
            }
        }


        private GameAction FindGameAction(string externalTransactionId, Guid gameProviderId)
        {
            return _repository.GetGameActionByExternalTransactionId(externalTransactionId, gameProviderId);
        }

        private Game GetGameByExternalGameId(string externalGameId)
        {
            var game = _repository.Games.SingleOrDefault(x => x.ExternalId == externalGameId);
            if (game == null)
                throw new RegoException("Game with id=" + externalGameId + " not found.");

            return game;

        }
    }
}