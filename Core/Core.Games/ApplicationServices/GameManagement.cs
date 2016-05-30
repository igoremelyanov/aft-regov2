using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Events;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Validations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class GameManagement : MarshalByRefObject, IGameManagement
    {
        private readonly IEventBus _eventBus;
        private readonly IGameRepository _repository;
        private readonly IActorInfoProvider _actorInfoProvider;

        public GameManagement(
            IGameRepository repository,
            IEventBus eventBus,
            IActorInfoProvider actorInfoProvider)
        {
            _eventBus = eventBus;
            _repository = repository;
            _actorInfoProvider = actorInfoProvider;
        }

        
        [Permission(Permissions.Create, Module = Modules.GameManager)]
        public void CreateGame(GameDTO game)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = new AddGameValidator(_repository)
                    .Validate(game);

                if (!validationResult.IsValid)
                    throw new RegoValidationException(validationResult);

                var gameEntity = new Interface.Data.Game
                {
                    Id = game.Id ?? Guid.NewGuid(),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = _actorInfoProvider.Actor.UserName,
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
                _eventBus.Publish(new GameCreated
                {
                    Id = gameEntity.Id,
                    GameProviderId = gameEntity.GameProviderId,
                    Name = gameEntity.Name,
                    Code = gameEntity.Code,
                    Url = gameEntity.EndpointPath,
                    CreatedDate = gameEntity.CreatedDate,
                    CreatedBy = gameEntity.CreatedBy
                });
                scope.Complete();
            }
        }

        [Permission(Permissions.Update, Module = Modules.GameManager)]
        public void UpdateGame(GameDTO game)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult =
                    new EditGameValidator().Validate(game);

                if (!validationResult.IsValid)
                    throw new RegoValidationException(validationResult);

                var gameEntity = _repository.Games.Single(x => x.Id == game.Id);

                gameEntity.UpdatedDate = DateTime.UtcNow;
                gameEntity.UpdatedBy = _actorInfoProvider.Actor.UserName;
                gameEntity.Name = game.Name;
                gameEntity.Code = game.Code;
                gameEntity.Type = game.Type;
                gameEntity.GameProviderId = game.ProductId;
                gameEntity.IsActive = game.Status == "Active";
                gameEntity.EndpointPath = game.Url;

                _repository.SaveChanges();

                _eventBus.Publish(new GameUpdated
                {
                    Id = gameEntity.Id,
                    GameProviderId = gameEntity.GameProviderId,
                    Name = gameEntity.Name,
                    Code = gameEntity.Code,
                    Url = gameEntity.EndpointPath,
                    UpdatedDate = gameEntity.UpdatedDate.GetValueOrDefault(),
                    UpdatedBy = gameEntity.UpdatedBy
                });
                scope.Complete();
            }
        }

        [Permission(Permissions.Delete, Module = Modules.GameManager)]
        public void DeleteGame(Guid id)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var game = _repository.Games.Single(x => x.Id == id);
                _repository.Games.Remove(game);
                _eventBus.Publish(new GameDeleted
                {
                    Id = game.Id,
                    Name = game.Name,
                    Code = game.Code,
                    Url = game.EndpointPath,
                    CreatedDate = game.CreatedDate,
                    CreatedBy = game.CreatedBy
                });
                _repository.SaveChanges();
                scope.Complete();
            }
        }


        [Permission(Permissions.Create, Module = Modules.SupportedProducts)]
        [Permission(Permissions.Create, Module = Modules.BetLevels)]
        [Permission(Permissions.Update, Module = Modules.BetLevels)]
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
                    _eventBus.Publish(new BetLimitDeleted(x));
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
                        DateCreated = DateTimeOffset.UtcNow,
                        CreatedBy = _actorInfoProvider.Actor.UserName
                    };
                    _repository.BetLimits.Add(limit);
                    _eventBus.Publish(new BetLimitCreated(limit));
                });

                existingBetLimits.ToList().ForEach(x =>
                {
                    var newLimit = viewModel.BetLevels.Single(y => y.Id == x.Id);
                    x.Name = newLimit.Name;
                    x.Description = newLimit.Description;
                    x.Code = newLimit.Code;
                    x.DateUpdated = DateTimeOffset.UtcNow;
                    x.UpdatedBy = _actorInfoProvider.Actor.UserName;

                    _eventBus.Publish(new BetLimitUpdated(x));
                });
            }
            else
            {
                limitsForBrandProduct.ToList().ForEach(x => _repository.BetLimits.Remove(x));
            }

            _repository.SaveChanges();
        }

        [Permission(Permissions.Create, Module = Modules.ProductManager)]
        public void CreateGameProvider(GameProvider gameProvider)
        {
            gameProvider.IsActive = false;

            if (gameProvider.Id.Equals(Guid.Empty))
            {
                gameProvider.Id = Guid.NewGuid();
            }

            gameProvider.CreatedBy = _actorInfoProvider.Actor.UserName;
            gameProvider.CreatedDate = DateTimeOffset.UtcNow;

            if (gameProvider.GameProviderConfigurations == null)
            {
                gameProvider.GameProviderConfigurations = new List<GameProviderConfiguration>();
            }

            if (gameProvider.GameProviderConfigurations == null || !gameProvider.GameProviderConfigurations.Any())
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
            _eventBus.Publish(new ProductCreated(gameProvider));
        }

        [Permission(Permissions.Update, Module = Modules.ProductManager)]
        public void UpdateGameProvider(GameProvider gameProvider)
        {
            gameProvider.UpdatedBy = _actorInfoProvider.Actor.UserName;
            gameProvider.UpdatedDate = DateTimeOffset.UtcNow;

            _repository.GameProviders.AddOrUpdate(gameProvider);

            _repository.SaveChanges();
            _eventBus.Publish(new ProductUpdated(gameProvider));
        }

        public void CreateGameProviderCurrency(GameProviderCurrency gameProviderCurrency)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = new AddGameProviderCurrencyValidator(_repository).Validate(gameProviderCurrency);

                if (!validationResult.IsValid)
                {
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);
                }

                if (gameProviderCurrency.Id.Equals(Guid.Empty))
                {
                    gameProviderCurrency.Id = Guid.NewGuid();
                }

                _repository.GameProviderCurrencies.Add(gameProviderCurrency);
                _repository.SaveChanges();

                var gameProviderCurrencyCreated = new GameProviderCurrencyCreated(gameProviderCurrency);
                _eventBus.Publish(gameProviderCurrencyCreated);

                scope.Complete();
            }
        }

        [Permission(Permissions.Create, Module = Modules.ProductManager)]
        public Guid CreateLobby(Lobby lobby)
        {
            if (lobby.Id.Equals(Guid.Empty))
            {
                lobby.Id = Guid.NewGuid();
            }

            lobby.CreatedBy = _actorInfoProvider.Actor.UserName;
            lobby.CreatedDate = DateTimeOffset.UtcNow;
            
            _repository.Lobbies.Add(lobby);

            _repository.SaveChanges();
            _eventBus.Publish(new LobbyCreated(lobby));

            return lobby.Id;
        }

        [Permission(Permissions.Update, Module = Modules.ProductManager)]
        public void UpdateLobby(Lobby lobby)
        {
            lobby.UpdatedBy = _actorInfoProvider.Actor.UserName;
            lobby.UpdatedDate = DateTimeOffset.UtcNow;

            _repository.Lobbies.AddOrUpdate(lobby);

            _repository.SaveChanges();
            _eventBus.Publish(new LobbyUpdated(lobby));
        }

        [Permission(Permissions.Create, Module = Modules.ProductManager)]
        public Guid CreateGameGroup(GameGroup gameGroup)
        {
            if (gameGroup.Id.Equals(Guid.Empty))
            {
                gameGroup.Id = Guid.NewGuid();
            }

            gameGroup.CreatedBy = _actorInfoProvider.Actor.UserName;
            gameGroup.CreatedDate = DateTimeOffset.UtcNow;
            
            _repository.GameGroups.Add(gameGroup);

            _repository.SaveChanges();
            _eventBus.Publish(new GameGroupCreated(gameGroup));

            return gameGroup.Id;
        }

        [Permission(Permissions.Update, Module = Modules.ProductManager)]
        public void UpdateGameGroup(GameGroup gameGroup)
        {
            gameGroup.UpdatedBy = _actorInfoProvider.Actor.UserName;
            gameGroup.UpdatedDate = DateTimeOffset.UtcNow;

            _repository.GameGroups.AddOrUpdate(gameGroup);

            _repository.SaveChanges();
            _eventBus.Publish(new GameGroupUpdated(gameGroup));
        }

        [Permission(Permissions.Update, Module = Modules.ProductManager)]
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
                        DateAdded = DateTimeOffset.UtcNow,
                        AddedBy = _actorInfoProvider.Actor.UserName
                    });
                }

                gameGroup.UpdatedDate = DateTimeOffset.UtcNow;
                gameGroup.UpdatedBy = _actorInfoProvider.Actor.UserName;

                _repository.SaveChanges();

                _eventBus.Publish(new GamesToGameGroupAssigned(
                    gameGroup.Id,
                    gameGroup.Name,
                    gameGroup.GameGroupGames.Select(x => x.Id)));

                scope.Complete();
            }
        }

        [Permission(Permissions.Update, Module = Modules.ProductManager)]
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
                        AddedBy = _actorInfoProvider.Actor.UserName
                    });
                }
                
                _repository.SaveChanges();

                _eventBus.Publish(new LobbiesToBrandAssigned(
                    brand.Id,
                    brand.Name,
                    brand.BrandLobbies.Select(x => x.Id)));

                scope.Complete();
            }
        }

        [Permission(Permissions.Update, Module = Modules.ProductManager)]
        public void AssignBrandCredentials(BrandCredentialsData data)
        {
            var brand = _repository.Brands.Single(x => x.Id == data.BrandId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                brand.ClientId = data.ClientId;
                brand.ClientSecret = data.ClientSecret;
                
                _repository.SaveChanges();

                _eventBus.Publish(new BrandCredentialsUpdated()
                {
                    Id = brand.Id,
                    AggregateId = brand.Id,
                    ClientId = brand.ClientId,
                    ClientSecret = brand.ClientSecret,
                    UpdatedDate = DateTimeOffset.UtcNow.ToBrandOffset(brand.TimezoneId),
                    UpdatedBy = _actorInfoProvider.Actor.UserName
                });

                scope.Complete();
            }
        }

        [Permission(Permissions.Create, Module = Modules.ProductManager)]
        public Guid CreateBetLimitGroup(BetLimitGroup betLimitGroup)
        {
            if (betLimitGroup.Id.Equals(Guid.Empty))
            {
                betLimitGroup.Id = Guid.NewGuid();
            }

            betLimitGroup.CreatedBy = _actorInfoProvider.Actor.UserName;
            betLimitGroup.CreatedDate = DateTimeOffset.UtcNow;

            _repository.BetLimitGroups.Add(betLimitGroup);

            _repository.SaveChanges();
            _eventBus.Publish(new BetLimitGroupCreated(betLimitGroup));

            return betLimitGroup.Id;
        }

        [Permission(Permissions.Update, Module = Modules.ProductManager)]
        public Guid UpdateBetLimitGroup(BetLimitGroup betLimitGroup)
        {
            betLimitGroup.UpdatedBy = _actorInfoProvider.Actor.UserName;
            betLimitGroup.UpdatedDate = DateTimeOffset.UtcNow;

            _repository.BetLimitGroups.AddOrUpdate(betLimitGroup);

            _repository.SaveChanges();
            _eventBus.Publish(new BetLimitGroupUpdated(betLimitGroup));

            return betLimitGroup.Id;
        }
    }
}
