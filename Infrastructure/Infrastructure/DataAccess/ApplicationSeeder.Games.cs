using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AFT.UGS.Core.BaseModels.Enums;

namespace AFT.RegoV2.Infrastructure.DataAccess
{
    public partial class ApplicationSeeder
    {
        public void AddGameProviderWithConfiguration(Guid providerId, string name, string code, bool isActive, Guid configurationId, string configurationName,
            string endpoint, string configurationType, string clientId, string clientSecret)
        {
            if (_gameRepository.GameProviders.Any(x => x.Id == providerId || x.Code == code))
                return;

            var gameProvider = new GameProvider
            {
                Id = providerId,
                Name = name,
                Code = code
            };

            var configuration = new GameProviderConfiguration
            {
                Id = configurationId,
                GameProviderId = providerId,
                Name = configurationName,
                AuthorizationClientId = clientId,
                AuthorizationSecret = clientSecret,
                SecurityKey = clientId + "_SECURITY_KEY",
                Endpoint = endpoint,
                Type = configurationType,
                IsActive = true
            };

            gameProvider.GameProviderConfigurations = new Collection<GameProviderConfiguration>
            {
                configuration
            };

            _gameManagement.CreateGameProvider(gameProvider);

            if (isActive)
            {
                gameProvider.IsActive = true;
                _gameManagement.UpdateGameProvider(gameProvider);
            }
        }

        public void AddGameProviderCurrency(Guid gameProviderId, string currencyCode, string gameProviderCurrencyCode)
        {
            if (_gameRepository.GameProviderCurrencies.Any(x => x.GameProviderId == gameProviderId &&
                    x.CurrencyCode == currencyCode &&
                    x.GameProviderCurrencyCode == gameProviderCurrencyCode))
                return;

            var gameProviderCurrency = new GameProviderCurrency
            {
                GameProviderId = gameProviderId,
                CurrencyCode = currencyCode,
                GameProviderCurrencyCode = gameProviderCurrencyCode,
            };

            _gameManagement.CreateGameProviderCurrency(gameProviderCurrency);
        }

        private Guid AddGameProviderGame(Guid gameId, Guid gameProviderId, string name, string code, string endpointPath, string externalGameId)
        {
            if (_gameRepository.Games.Any(x => x.Id == gameId || x.Code == code))
                return gameId;

            _gameManagement.CreateGame(new GameDTO
            {
                Id = gameId,
                ProductId = gameProviderId,
                ExternalId = externalGameId,
                Name = name,
                Code = code,
                Url = endpointPath,
                Status = "Active",
            });

            return gameId;
        }

        private void AddBetLimits(Guid brand138Id, Guid providerMockGpId)
        {
            var brand = _brandRepository.Brands.FirstOrDefault(x => x.Id == brand138Id);
            if (brand == null)
            {
                return;
            }

            var betLimitsCount = _gameRepository.BetLimits.Count(x => x.BrandId == brand.Id && x.GameProviderId == providerMockGpId);
            if (betLimitsCount == 0)
            {
                _gameManagement.UpdateProductSettings(new BrandProductSettingsData
                {
                    BrandId = brand138Id,
                    ProductId = providerMockGpId,
                    BetLevels = new[]
                    {
                        new BetLevelData {Code = "10", Name = "BetLevel1"},
                        new BetLevelData {Code = "20", Name = "BetLevel2"},
                        new BetLevelData {Code = "30", Name = "BetLevel3"},
                    }
                });
            }
        }

        private Guid AddBetLimitGroup(string name, int externalId)
        {
            var betLimitGroup = _gameRepository.BetLimitGroups.FirstOrDefault(x => x.Name == name);
            if (betLimitGroup != null)
                return betLimitGroup.Id;

            return _gameManagement.CreateBetLimitGroup(new BetLimitGroup
            {
                Id = Guid.NewGuid(),
                Name = name,
                ExternalId = externalId
            });
        }

        private void AddLobby(Guid id, string name, string clientId, string secret, PlatformType platformType, bool isActive = true)
        {
            var lobby = _gameRepository.Lobbies.FirstOrDefault(x => x.Id == id);
            if (lobby != null)
                return;

            _gameManagement.CreateLobby(new Lobby
            {
                Id = id,
                Name = name,
                ClientId = clientId,
                ClientSecret = secret,
                PlatformType = platformType,
                IsActive = true
            });
        }

        private void AssignLobbiesToBrand(Guid brandId, Guid[] lobbiesIds)
        {
            var brand = _gameRepository.Brands.Include(x => x.BrandLobbies).FirstOrDefault(x => x.Id == brandId);
            if (brand == null)
                return;

            if (brand.BrandLobbies.Select(x => x.LobbyId).ToArray().ScrambledEquals(lobbiesIds))
            {
                return;
            }

            _gameManagement.AssignLobbiesToBrand(new AssignLobbiesToBrandData
            {
                BrandId = brandId,
                LobbiesIds = lobbiesIds
            });
        }

        private void AddGameGroupWithGames(Guid gameGroupId, Guid lobbyId, string name, string code, Guid[] gamesIds)
        {
            var gameGroup = _gameRepository.GameGroups.FirstOrDefault(x => x.Id == gameGroupId);
            if (gameGroup != null)
                return;

            _gameManagement.CreateGameGroup(new GameGroup
            {
                Id = gameGroupId,
                Name = name,
                Code = code,
                LobbyId = lobbyId,
            });

            _gameManagement.AssignGamesToGameGroups(new AssignGamesToGameGroupData
            {
                GameGroupId = gameGroupId,
                GamesIds = gamesIds
            });
        }
    }
}
