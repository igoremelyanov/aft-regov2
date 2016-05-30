using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Attributes;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interface.Exceptions;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using Round = AFT.RegoV2.Core.Game.Entities.Round;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public sealed class GameCommands : MarshalByRefObject, IGameCommands
    {
        private readonly IEventBus _eventBus;
        private readonly IGameRepository _repository;
        private readonly IGameWalletOperations _walletOperations;

        public GameCommands(
            IGameRepository repository,
            IGameWalletOperations walletOperations,
            IEventBus eventBus)
        {
            _eventBus = eventBus;
            _walletOperations = walletOperations;
            _repository = repository;
        }

        /// <summary>
        /// Places a bet
        /// </summary>
        async Task<Guid> IGameCommands.PlaceBetAsync([NotNull] GameActionData gameActionData,
            [NotNull] GameActionContext context, Guid playerId)
        {

            var player = await ValidatePlayer(playerId);
            await ValidateBrand(player.BrandId);
            var gameProviderId = await ValidateGameProvider(context.GameProviderCode);
            ValidateTransactionIsUnique(gameActionData.ExternalTransactionId, gameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var game = GetGameByExternalGameId(gameActionData.ExternalGameId);

                var round = _repository.GetOrCreateRound(gameActionData.RoundId, game.Id, player.Id, player.BrandId);

                await
                    _walletOperations.PlaceBetAsync(playerId, game.Id, round.Data.Id, gameActionData.Amount,
                        gameActionData.ExternalTransactionId);

                var placeBetGameActionId = round.Place(gameActionData, context);

                _repository.Rounds.AddOrUpdate(x => x.ExternalRoundId, round.Data);
                _repository.SaveChanges();

                round.Events.ForEach(ev => _eventBus.Publish(ev));

                scope.Complete();

                return placeBetGameActionId;
            }
        }


        /// <summary>
        /// Registers a Win bet action
        /// </summary>
        /// <param name="gameActionData"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        async Task<Guid> IGameCommands.WinBetAsync([NotNull] GameActionData gameActionData,
            [NotNull] GameActionContext context)
        {
            var gameProviderId = await ValidateGameProvider(context.GameProviderCode);

            ValidateTransactionIsUnique(gameActionData.ExternalTransactionId, gameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var round = GetRound(gameActionData);

                await
                    _walletOperations.WinBetAsync(round.Data.PlayerId, round.Data.GameId, round.Data.Id,
                        gameActionData.Amount, gameActionData.ExternalTransactionId);

                var winGameActionId = round.Win(gameActionData, context);

                await _repository.SaveChangesAsync();

                round.Events.ForEach(ev => _eventBus.Publish(ev));

                scope.Complete();

                return winGameActionId;
            }
        }

        /// <summary>
        /// Registers a Lost bet action
        /// </summary>
        /// <param name="gameActionData"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        async Task<Guid> IGameCommands.LoseBetAsync([NotNull] GameActionData gameActionData,
            [NotNull] GameActionContext context)
        {
            if (gameActionData.Amount != 0)
            {
                throw new LoseBetAmountMustBeZeroException();
            }
            var gameProviderId = await ValidateGameProvider(context.GameProviderCode);

            ValidateTransactionIsUnique(gameActionData.ExternalTransactionId, gameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var round = GetRound(gameActionData);

                await _walletOperations.LoseBetAsync(round.Data.PlayerId, round.Data.GameId, round.Data.Id, gameActionData.ExternalTransactionId);

                var loseGameActionId = round.Lose(gameActionData, context);

                await _repository.SaveChangesAsync();

                round.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();

                return loseGameActionId;
            }
        }

        /// <summary>
        /// Registers a Tie bet action
        /// </summary>
        /// <param name="gameActionData"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        async Task<Guid> IGameCommands.TieBetAsync([NotNull] GameActionData gameActionData,
            [NotNull] GameActionContext context)
        {
            var gameProviderId = await ValidateGameProvider(context.GameProviderCode);

            ValidateTransactionIsUnique(gameActionData.ExternalTransactionId, gameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var round = GetRound(gameActionData);

                await
                    _walletOperations.TieBetAsync(round.Data.PlayerId, round.Data.GameId, round.Data.Id,
                        gameActionData.Amount, gameActionData.ExternalTransactionId);

                var tieGameActionId = round.Tie(gameActionData, context);

                await _repository.SaveChangesAsync();

                round.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();

                return tieGameActionId;
            }
        }

        //
        // The idea of the "Free bet" is that game provider can let players win a bet without actually placing it
        //
        async Task<Guid> IGameCommands.FreeBetAsync([NotNull] GameActionData gameActionData,
            [NotNull] GameActionContext context, Guid? playerId)
        {
            var gameProviderId = await ValidateGameProvider(context.GameProviderCode);

            ValidateTransactionIsUnique(gameActionData.ExternalTransactionId, gameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var game = GetGameByExternalGameId(gameActionData.ExternalGameId);

                var player = playerId == null ? null : await _repository.Players.SingleAsync(x => x.Id == playerId);
                var round = player == null
                    ? GetRound(gameActionData)
                    : _repository.GetOrCreateRound(gameActionData.RoundId, game.Id, player.Id, player.BrandId);

                if (gameActionData.Amount > 0)
                {
                    await
                        _walletOperations.FreeBetAsync(round.Data.PlayerId, round.Data.GameId, round.Data.Id,
                            gameActionData.Amount, gameActionData.ExternalTransactionId);
                }

                var freeBetGameActionId = round.Free(gameActionData, context);

                _repository.Rounds.AddOrUpdate(x => x.ExternalRoundId, round.Data);

                _repository.SaveChanges();

                round.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();

                return freeBetGameActionId;
            }
        }

        /// <summary>
        /// Adjusts a game action made previously
        /// </summary>
        /// <param name="gameActionData"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Guid IGameCommands.AdjustTransaction([NotNull] GameActionData gameActionData, [NotNull] GameActionContext context)
        {
            var gameProviderId = ValidateGameProvider(context.GameProviderCode).GetAwaiter().GetResult();

            ValidateTransactionIsUnique(gameActionData.ExternalTransactionId, gameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var round = GetRound(gameActionData);

                var adjustmentGameActionId = AdjustRound(round, gameActionData, context);

                _repository.SaveChanges();

                round.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();

                return adjustmentGameActionId;
            }
        }

        /// <summary>
        /// Cancels a game action
        /// </summary>
        /// <param name="gameActionData"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        async Task<Guid> IGameCommands.CancelTransactionAsync([NotNull] GameActionData gameActionData,
            [NotNull] GameActionContext context)
        {
            var gameProviderId = await ValidateGameProvider(context.GameProviderCode);

            ValidateTransactionIsUnique(gameActionData.ExternalTransactionId, gameProviderId);

            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                var round = GetRound(gameActionData);

                var gameActionToCancel = round.GetGameActionByReferenceId(gameActionData.TransactionReferenceId);

                if ( gameActionToCancel == null)
                    throw new GameActionNotFoundException();

                var amount = -gameActionToCancel.Amount;
                if (gameActionToCancel.Amount != amount)
                {
                    // TODO: raise an administrative event (amounts don't match when cancelling a transaction)   
                }

                await
                    _walletOperations.CancelBetAsync(round.Data.PlayerId, round.Data.GameId, round.Data.Id,
                        gameActionData.ExternalTransactionId, gameActionData.TransactionReferenceId);

                var cancelGameActionId = CancelRound(round, gameActionData, context);

                _repository.SaveChanges();

                round.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();

                return cancelGameActionId;
            }
        }

        private async Task<Guid> ValidateGameProvider(string gameProviderCode)

        {

            var gameProvider = await _repository.GameProviders.SingleAsync(x => x.Code == gameProviderCode);

            return gameProvider.Id;
        }

        private async Task ValidateBrand(Guid brandId)
        {
            if (await _repository.Brands.AnyAsync(x => x.Id == brandId) == false)
                throw new RegoException("Brand not found. " + brandId);
        }

        private async Task<Player> ValidatePlayer(Guid playerId)
        {
            return await _repository.Players.SingleAsync(x => x.Id == playerId);
        }

        private Round GetRound(GameActionData gameActionData)
        {
            var round = _repository.GetRound(gameActionData.RoundId);
            if (round == null)
                throw new RoundNotFoundException();
            return round;
        }

        private Guid AdjustRound(Round round, GameActionData gameActionData, GameActionContext context)
        {
            return round.Adjust(gameActionData, context);
        }

        private Guid CancelRound(Round round, GameActionData gameActionData, GameActionContext context)
        {
            return round.Cancel(gameActionData, context);
        }

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

        private Interface.Data.Game GetGameByExternalGameId(string externalGameId)
        {
            var game = _repository.Games.SingleOrDefault(x => x.ExternalId == externalGameId);
            if (game == null)
                throw new RegoException("Game with id=" + externalGameId + " not found.");

            return game;
        }
    }
}