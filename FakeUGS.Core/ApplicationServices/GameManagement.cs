using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;
using FakeUGS.Core.Data;
using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.ApplicationServices
{
    public class GameManagement : MarshalByRefObject, IGameManagement
    {
        private readonly IEventBus _eventBus;
        private readonly IRepository _repository;

        public GameManagement(
            IRepository repository,
            IEventBus eventBus)
        {
            _eventBus = eventBus;
            _repository = repository;
        }

        
        public void CreateGame(GameDTO game)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var gameEntity = new Game
                {
                    Id = game.Id ?? Guid.NewGuid(),
                    CreatedDate = DateTime.UtcNow,
                    Name = game.Name,
                    Code = game.Code,
                    Type = game.Type,
                    GameProviderId = game.ProductId,
                    IsActive = game.Status == "Active",
                    EndpointPath = game.Url,
                    ExternalId = game.ExternalId
                };

                _repository.Games.Add(gameEntity);
                _repository.SaveChanges();
                scope.Complete();
            }
        }

        public void UpdateGame(GameDTO game)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var gameEntity = _repository.Games.Single(x => x.Id == game.Id);

                gameEntity.UpdatedDate = DateTime.UtcNow;
                gameEntity.Name = game.Name;
                gameEntity.Code = game.Code;
                gameEntity.Type = game.Type;
                gameEntity.GameProviderId = game.ProductId;
                gameEntity.IsActive = game.Status == "Active";
                gameEntity.EndpointPath = game.Url;

                _repository.SaveChanges();
                scope.Complete();
            }
        }

        public void DeleteGame(Guid id)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var game = _repository.Games.Single(x => x.Id == id);
                _repository.Games.Remove(game);
                _repository.SaveChanges();
                scope.Complete();
            }
        }

        public void UpdateProductSettings(BrandProductSettingsData viewModel)
        {
            var limitsForBrandProduct =
                    _repository
                        .BetLimits
                        .Where(x => x.BrandId == viewModel.BrandId && x.GameProviderId == viewModel.ProductId);

            if (viewModel.BetLevels != null)
            {
                var existingBetLimits = _repository.BetLimits.ToList().Where(x => viewModel.BetLevels.Any(y => y.Id == x.Id));
                var newBetLimits = viewModel.BetLevels.Where(x => x.Id == Guid.Empty);

                var limitsToDelete =
                    limitsForBrandProduct.ToList().Where(x => !viewModel.BetLevels.Any(y => y.Id == x.Id && x.Id != Guid.Empty));
                limitsToDelete.ToList().ForEach(x =>
                {
                    _repository.BetLimits.Remove(x);
                });
                newBetLimits.ToList().ForEach(x =>
                {
                    var limit = new GameProviderBetLimit
                    {
                        Id = Guid.NewGuid(),
                        BrandId = viewModel.BrandId,
                        GameProviderId = viewModel.ProductId,
                        Code = x.Code,
                        Name = x.Name,
                        Description = x.Description,
                        DateCreated = DateTimeOffset.UtcNow
                    };
                    _repository.BetLimits.Add(limit);
                });

                existingBetLimits.ToList().ForEach(x =>
                {
                    var newLimit = viewModel.BetLevels.Single(y => y.Id == x.Id);
                    x.Name = newLimit.Name;
                    x.Description = newLimit.Description;
                    x.Code = newLimit.Code;
                    x.DateUpdated = DateTimeOffset.UtcNow;
                });
            }
            else
            {
                limitsForBrandProduct.ToList().ForEach(x => _repository.BetLimits.Remove(x));
            }

            _repository.SaveChanges();
        }

        public void CreateGameProvider(GameProvider gameProvider)
        {
            gameProvider.IsActive = false;

            if (gameProvider.Id.Equals(Guid.Empty))
            {
                gameProvider.Id = Guid.NewGuid();
            }

            gameProvider.CreatedDate = DateTimeOffset.UtcNow;

            if (gameProvider.GameProviderConfigurations == null)
            {
                gameProvider.GameProviderConfigurations = new List<GameProviderConfiguration>();
            }

            if (gameProvider.GameProviderConfigurations.Count == 0)
            {
                gameProvider.GameProviderConfigurations = new Collection<GameProviderConfiguration>
                {
                    new GameProviderConfiguration
                    {
                        Id = Guid.NewGuid(),
                        GameProviderId = gameProvider.Id,
                        Name = "Default Configuration for " + gameProvider.Name
                    }
                };
            }

            _repository.GameProviders.Add(gameProvider);

            _repository.SaveChanges();
        }

        public void UpdateGameProvider(GameProvider gameProvider)
        {
            gameProvider.UpdatedDate = DateTimeOffset.UtcNow;

            _repository.GameProviders.AddOrUpdate(gameProvider);

            _repository.SaveChanges();
        }

        public void CreateGameProviderCurrency(GameProviderCurrency gameProviderCurrency)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                if (gameProviderCurrency.Id.Equals(Guid.Empty))
                {
                    gameProviderCurrency.Id = Guid.NewGuid();
                }

                _repository.GameProviderCurrencies.Add(gameProviderCurrency);
                _repository.SaveChanges();

                scope.Complete();
            }
        }

        public Guid CreateLobby(Lobby lobby)
        {
            if (lobby.Id.Equals(Guid.Empty))
            {
                lobby.Id = Guid.NewGuid();
            }

            lobby.CreatedDate = DateTimeOffset.UtcNow;
            
            _repository.Lobbies.Add(lobby);

            _repository.SaveChanges();

            return lobby.Id;
        }

        public void UpdateLobby(Lobby lobby)
        {
            lobby.UpdatedDate = DateTimeOffset.UtcNow;

            _repository.Lobbies.AddOrUpdate(lobby);

            _repository.SaveChanges();
        }

        public Guid CreateGameGroup(GameGroup gameGroup)
        {
            if (gameGroup.Id.Equals(Guid.Empty))
            {
                gameGroup.Id = Guid.NewGuid();
            }

            gameGroup.CreatedDate = DateTimeOffset.UtcNow;
            
            _repository.GameGroups.Add(gameGroup);

            _repository.SaveChanges();

            return gameGroup.Id;
        }

        public void UpdateGameGroup(GameGroup gameGroup)
        {
            gameGroup.UpdatedDate = DateTimeOffset.UtcNow;

            _repository.GameGroups.AddOrUpdate(gameGroup);

            _repository.SaveChanges();
        }

        public void AssignGamesToGameGroups(AssignGamesToGameGroupData data)
        {
            var gameGroup = _repository.GameGroups
                .Include(x => x.GameGroupGames)
                .Single(x => x.Id == data.GameGroupId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var gameGroupGamesToRemove = gameGroup.GameGroupGames
                    .Where(x => !data.GamesIds.Contains(x.Id))
                    .ToArray();

                foreach (var gameGroupGame in gameGroupGamesToRemove)
                {
                    gameGroup.GameGroupGames.Remove(gameGroupGame);
                }

                var gameGroupGameIdsToAdd = data.GamesIds
                    .Where(x => gameGroup.GameGroupGames.All(y => y.Id != x))
                    .ToArray();

                foreach (var gameGroupGameId in gameGroupGameIdsToAdd)
                {
                    var gameGroupGameToAdd = _repository.Games.Single(x => x.Id == gameGroupGameId);

                    gameGroup.GameGroupGames.Add(new GameGroupGame
                    {
                        Id = Guid.NewGuid(),
                        GameGroupId = gameGroup.Id,
                        GameGroup = gameGroup,
                        GameId = gameGroupGameToAdd.Id,
                        Game = gameGroupGameToAdd,
                        DateAdded = DateTimeOffset.UtcNow
                    });
                }

                gameGroup.UpdatedDate = DateTimeOffset.UtcNow;

                _repository.SaveChanges();

                scope.Complete();
            }
        }

        public void AssignLobbiesToBrand(AssignLobbiesToBrandData data)
        {
            var brand = _repository.Brands
                .Include(x => x.BrandLobbies)
                .Single(x => x.Id == data.BrandId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var brandLobbiesToRemove = brand.BrandLobbies
                    .Where(x => !data.LobbiesIds.Contains(x.Id))
                    .ToArray();

                foreach (var brandLobby in brandLobbiesToRemove)
                {
                    brand.BrandLobbies.Remove(brandLobby);
                }

                var brandLobbyIdsToAdd = data.LobbiesIds
                    .Where(x => brand.BrandLobbies.All(y => y.Id != x))
                    .ToArray();

                foreach (var brandLobbyId in brandLobbyIdsToAdd)
                {
                    var brandLobbyToAdd = _repository.Lobbies.Single(x => x.Id == brandLobbyId);

                    brand.BrandLobbies.Add(new BrandLobby
                    {
                        Id = Guid.NewGuid(),
                        BrandId = brand.Id,
                        Brand = brand,
                        LobbyId = brandLobbyToAdd.Id,
                        Lobby = brandLobbyToAdd,
                        DateAdded = DateTimeOffset.UtcNow.ToBrandOffset(brand.TimezoneId),
                    });
                }
                
                _repository.SaveChanges();


                scope.Complete();
            }
        }

        public void AssignBrandCredentials(BrandCredentialsData data)
        {
            var brand = _repository.Brands.Single(x => x.Id == data.BrandId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                brand.ClientId = data.ClientId;
                brand.ClientSecret = data.ClientSecret;
                
                _repository.SaveChanges();

                scope.Complete();
            }
        }
    }
}
