using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interfaces;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class GamesTestHelper
    {
        private readonly IGameCommands _gameCommands;
        private readonly IGameRepository _gamesRepository;
        private readonly IGameManagement _gameManagement;
        private readonly IGameQueries _gameQueries;

        public GamesTestHelper(IGameCommands gameCommands, IGameRepository gameRepository, IGameManagement gameManagement, IGameQueries gameQueries)
        {
            _gameCommands = gameCommands;
            _gameManagement = gameManagement;
            _gameQueries = gameQueries;
            _gamesRepository = gameRepository;
        }

        public async Task PlaceAndWinBet(decimal amountPlaced, decimal amountWon, Guid playerId, string gameProviderCode, string gameId)
        {
            var placeBetTxId = Guid.NewGuid().ToString();
            var actualBetId = await PlaceBet(amountPlaced, playerId, gameProviderCode, gameId, transactionId:placeBetTxId);
            await WinBet(actualBetId, amountWon, placeBetTxId, gameProviderCode);
        }

        public async Task PlaceAndWinBet(decimal amountPlaced, decimal amountWon, Guid playerId)
        {
            var gameId = GetMainWalletExternalGameId(playerId);
            var gameProviderCode = GetGameProviderCodeByGameExternalId(gameId);

            await PlaceAndWinBet(amountPlaced, amountWon, playerId, gameProviderCode, gameId);
        }

        public async Task WinBet(string roundId, decimal amountWon, string placeBetTxId, string gameProviderCode)
        {
            await _gameCommands.WinBetAsync(
                GameActionData.NewGameActionData(roundId, 
                amountWon, 
                "CAD", 
                transactionReferenceId: placeBetTxId), 
                new GameActionContext
                {
                    GameProviderCode = gameProviderCode
                });
        }

        public async Task PlaceAndLoseBet(decimal amount, Guid playerId, string gameProviderCode, string gameId)
        {
            var placeBetTxId = Guid.NewGuid().ToString();
            var actualBetId = await PlaceBet(amount, playerId, gameProviderCode, gameId, transactionId: placeBetTxId);
            await LoseBet(actualBetId, placeBetTxId, gameProviderCode);
        }

        public async Task PlaceAndLoseBet(decimal amount, Guid playerId)
        {
            var gameId = GetMainWalletExternalGameId(playerId);
            var gameProviderCode = GetGameProviderCodeByGameExternalId(gameId);

            await PlaceAndLoseBet(amount, playerId, gameProviderCode, gameId);
        }

        public async Task LoseBet(string roundId, string placeBetTxId, string gameProviderCode)
        {
            await _gameCommands.LoseBetAsync(
                GameActionData.NewGameActionData(roundId, 
                0, 
                "CAD", 
                transactionReferenceId: placeBetTxId), 
                new GameActionContext
                {
                    GameProviderCode = gameProviderCode
                });
        }

        public async Task PlaceAndCancelBet(decimal amount, Guid playerId, string gameProviderCode, string gameId)
        {
            var gameActionId = Guid.NewGuid().ToString();

            var actualBetId = await PlaceBet(amount, playerId, gameProviderCode, gameId, transactionId: gameActionId);
            CancelBet(actualBetId, amount, gameActionId, gameProviderCode);
        }

        public async Task PlaceAndCancelBet(decimal amount, Guid playerId)
        {
            var gameId = GetMainWalletExternalGameId(playerId);
            var gameProviderCode = GetGameProviderCodeByGameExternalId(gameId);

            await PlaceAndCancelBet(amount, playerId, gameProviderCode, gameId);
        }

        private void CancelBet(string roundId, decimal amount, string transactionIdToCancel, string gameProviderCode)
        {
            var newBet = GameActionData.NewGameActionData(roundId, amount, "CAD");
            newBet.TransactionReferenceId = transactionIdToCancel;

            _gameCommands.CancelTransactionAsync(newBet, new GameActionContext()
            {
                GameProviderCode = gameProviderCode
            });
        }

        public async Task<string> PlaceBet(decimal amount, Guid playerId, string gameProviderCode, string gameId, string roundId = null, string transactionId = null)
        {
            roundId = roundId ?? Guid.NewGuid().ToString();
            await _gameCommands.PlaceBetAsync(
                GameActionData.NewGameActionData(roundId, amount, "CAD",
                gameId,
                transactionId),
                new GameActionContext
                {
                    GameProviderCode = gameProviderCode
                }, playerId);

            return roundId;
        }

        public async Task<string> PlaceBet(decimal amount, Guid playerId, string roundId = null, string transactionId = null)
        {
            var gameId = GetMainWalletExternalGameId(playerId);
            var gameProviderCode = GetGameProviderCodeByGameExternalId(gameId);

            return await PlaceBet(amount, playerId, gameProviderCode, gameId, roundId, transactionId);
        }

        public GameProvider CreateGameProvider()
        {
            var gameProviderId = Guid.NewGuid();
            var gameProviderCode = Guid.NewGuid().ToString();
            var gameProvider = new GameProvider
            {
                Id = gameProviderId,
                Name = TestDataGenerator.GetRandomString(),
                IsActive = true,
                Code = gameProviderCode,
                GameProviderConfigurations = new Collection<GameProviderConfiguration>()
                {
                    new GameProviderConfiguration
                    {
                        Id = Guid.NewGuid(),
                        Endpoint = "http://localhost"
                    }    
                },
                Games = new List<Game>
                {
                    CreateGame(gameProviderId, TestDataGenerator.GetRandomString(), TestDataGenerator.GetRandomString())
                }
            };
            _gamesRepository.GameProviders.Add(gameProvider);
            _gamesRepository.SaveChanges();
            return gameProvider;
        }

        public GameProviderBetLimit CreateBetLevel(GameProvider gameProvider, Guid brandId)
        {
            var betLevel = new GameProviderBetLimit
            {
                Id = Guid.NewGuid(),
                GameProviderId = gameProvider.Id,
                BrandId = brandId,
                Code = new Random().Next(100000).ToString(),
                DateCreated = DateTimeOffset.UtcNow
            };
            _gamesRepository.BetLimits.Add(betLevel);
            return betLevel;
        }

        public Game CreateGame(Guid gameProviderId, string name, string externalGameId)
        {
            var game = new Game
            {
                Id = Guid.NewGuid(),
                GameProviderId = gameProviderId,
                ExternalId = externalGameId,
                Name = name,
                EndpointPath = "/Game/Index"
            };
            _gamesRepository.Games.Add(game);
            return game;
        }

        public Guid GetGameProviderIdByGameExternalId(string gameExternalId)
        {
            return _gamesRepository.Games.Single(x => x.ExternalId == gameExternalId).GameProviderId;
        }

        public string GetGameProviderCodeByGameExternalId(string gameExternalId)
        {
            return _gamesRepository.GameProviders.Single(x => x.Id == GetGameProviderIdByGameExternalId(gameExternalId)).Code;
        }

        public string GetMainWalletExternalGameId(Guid playerId)
        {
            return GetMainWalletGame(playerId).ExternalId;
        }

        public Game GetMainWalletGame(Guid playerId)
        {
            return _gamesRepository.GameProviders.First().Games.First();
        }

        public BetLimitGroup CreateBetLimitGroup(string name, int externalGameId = 1, bool skipCreationIfAlreadyExists = true)
        {
            if (skipCreationIfAlreadyExists)
            {
                var existedGroup = _gamesRepository.BetLimitGroups.SingleOrDefault(x => x.Name == name);
                if (existedGroup != null)
                {
                    return existedGroup;
                }
            }

            var betLimitGroup = new BetLimitGroup
            {
                Id = Guid.NewGuid(),
                Name = name,
                ExternalId = externalGameId,
            };

            _gameManagement.CreateBetLimitGroup(betLimitGroup);

            return _gameQueries.GetBetLimitGroup(betLimitGroup.Id);
        }
    }
}