using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interface.Exceptions;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Shared;

using PlatformType = AFT.UGS.Core.BaseModels.Enums.PlatformType;
using Round = AFT.RegoV2.Core.Game.Entities.Round;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public sealed class GameQueries : MarshalByRefObject, IGameQueries
    {
        private readonly IGameRepository _gameRepository;
        private readonly IGameWalletQueries _walletQueries;

        public GameQueries(
            IGameRepository gameRepository,
            IGameWalletQueries walletQueries)
        {
            _gameRepository = gameRepository;
            _walletQueries = walletQueries;
        }

        PlayableBalanceInfo IGameQueries.GetPlayableBalance(Guid playerId)
        {
            var playerBalance = _walletQueries.GetPlayerBalance(playerId);

            return new PlayableBalanceInfo
            {
                Balance = playerBalance.Amount,
                CurrencyCode = playerBalance.CurrencyCode
            };
        }

        async Task<PlayableBalanceInfo> IGameQueries.GetPlayableBalanceAsync(Guid playerId)
        {
            var playerBalance = await _walletQueries.GetPlayerBalanceAsync(playerId);

            return new PlayableBalanceInfo
            {
                Balance = playerBalance.Amount,
                CurrencyCode = playerBalance.CurrencyCode
            };
        }

        Dictionary<Guid, decimal> IGameQueries.GetPlayableBalances(IEnumerable<Guid> playerIds)
        {
            var players = playerIds.ToList();

            return _walletQueries.GetPlayersBalanceAsync(players);
        }


        List<Round> IGameQueries.GetRoundHistory(Guid playerId, Guid gameId, int recordCount)
        {
            return _gameRepository
                .GetPlayerRounds(playerId)
                .Where(round => round.Data.GameId == gameId)
                .OrderByDescending(x => x.Data.CreatedOn)
                .Take(recordCount)
                .ToList();
        }

        Round IGameQueries.GetRoundByGameActionId(Guid gameActionId)
        {
            return new Round(_gameRepository.Rounds
                .Include(round => round.Brand)
                .Include(round => round.GameActions)
                .Single(round => round.Id == _gameRepository.GameActions.FirstOrDefault(tx => tx.Id == gameActionId).Round.Id));
        }

        Player IGameQueries.GetPlayerData(Guid playerId)
        {
            return _gameRepository.Players.Single(player => player.Id == playerId);
        }

        Interface.Data.Brand IGameQueries.GetBrand(string brandCode)
        {
            return _gameRepository.Brands.Single(x => x.Code == brandCode);
        }

        void IGameQueries.ValidateBatchIsUnique(string batchId, Guid gameProviderId)
        {
            if (_gameRepository.DoesBatchIdExist(batchId, gameProviderId))
            {
                throw new DuplicateBatchException();
            }
        }

        async Task<bool> IGameQueries.ValidateSecurityKey(Guid gameProviderId, string securityKey)
        {
            return
                await
                    _gameRepository.GameProviderConfigurations.AnyAsync(
                        x => x.GameProviderId == gameProviderId && 
                            x.SecurityKey == securityKey);
        }

        async Task<GameProviderConfiguration> IGameQueries.GetGameProviderConfigurationAsync(Guid brandId, Guid gameProviderId)
        {
            var bgpc = await
                _gameRepository.BrandGameProviderConfigurations
                    .Include(x => x.GameProviderConfiguration)
                    .SingleAsync(
                    x => x.BrandId == brandId && x.GameProviderId == gameProviderId);
            
            return bgpc.GameProviderConfiguration;
        }

        async Task<List<GameProviderConfiguration>> IGameQueries.GetGameProviderConfigurationListAsync(Guid gameProviderId)
        {
            return await
                _gameRepository.GameProviderConfigurations
                    .Where(x => x.GameProviderId == gameProviderId).ToListAsync();
        }

        [Permission(Permissions.View, Module = Modules.GameManager)]
        IEnumerable<GameDTO> IGameQueries.GetGameDtos()
        {
            return _gameRepository.Games.Select(x => new GameDTO()
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Type = x.Type,
                Url = x.EndpointPath,
                Status = x.IsActive ? "Active" : "Inactive",
                ProductId = x.GameProviderId,
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
                UpdatedBy = x.UpdatedBy,
                UpdatedDate = x.UpdatedDate
            });
        }

        GameDTO IGameQueries.GetGameDto(Guid gameId)
        {
            return _gameRepository
                .Games
                .Where(x => x.Id == gameId)
                .Select(x => new GameDTO()
            {
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                UpdatedBy = x.UpdatedBy,
                UpdatedDate = x.UpdatedDate,
                Type = x.Type,
                ProductId = x.GameProviderId,
                Status = x.IsActive ? "Active" : "Inactive",
                Url = x.EndpointPath
            }).FirstOrDefault();
        }

        IEnumerable<BetLimitDTO> IGameQueries.GetBetLimits(Guid gameProviderId, Guid brandId)
        {
            return _gameRepository
                .BetLimits
                .Where(x => x.GameProviderId == gameProviderId && x.BrandId == brandId)
                .ToArray()
                .Select(x => GetBetLimitDto(x.Id));
        }

        public BetLimitDTO GetBetLimitDto(Guid id)
        {
            var betLimit = _gameRepository.BetLimits.SingleOrDefault(x => x.Id == id);

            if (betLimit == null) return null;

            var betLimitDto = new BetLimitDTO
            {
                Id = betLimit.Id,
                GameProviderId = betLimit.GameProviderId,
                LimitId = betLimit.Code,
                Name = betLimit.Name,
                FullName = betLimit.Code + " - " + betLimit.Name,
                Description = betLimit.Description
            };

            return betLimitDto;
        }

        IEnumerable<GameProviderDTO> IGameQueries.GetGameProviders(Guid brandId)
        {
            var brand =
                _gameRepository.Brands
                    .Include(x => x.BrandGameProviderConfigurations.Select(y => y.GameProvider))
                    .Single(x => x.Id == brandId);

            return brand
                .BrandGameProviderConfigurations
                .Select(x => new GameProviderDTO
                {
                    Id = x.GameProviderId,
                    Name = x.GameProvider.Name,
                    Code = x.GameProvider.Code
                });
        }

        [Permission(Permissions.View, Module = Modules.ProductManager)]
        IEnumerable<GameProviderDTO> IGameQueries.GetGameProviderDtos()
        {
            return _gameRepository.GameProviders
                .Select(x => new GameProviderDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    Category = x.Category,
                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToArray();
        }

        IEnumerable<GameProvider> IGameQueries.GetGameProviders()
        {
            return _gameRepository.GameProviders;
        }

        IEnumerable<GameProvider> IGameQueries.GetGameProvidersWithGames(Guid brandId)
        {
            var configurations = _gameRepository
                .BrandGameProviderConfigurations
                .Include(x => x.GameProvider)
                .Include(x => x.GameProvider.Games)
                .Where(x => x.BrandId == brandId);

            return configurations.Select(x => x.GameProvider).Include(x => x.Games);
        }

        async Task<Interface.Data.Game> IGameQueries.GetGameByIdAsync(Guid gameId)
        {
            return await _gameRepository.Games.SingleAsync(x => x.Id == gameId);
        }

        public Interface.Data.Game GetGameByExternalGameId(string externalGameId)
        {
            return _gameRepository.Games.FirstOrDefault(x => x.ExternalId == externalGameId);
        }


        async Task<Player> IGameQueries.GetPlayerDataAsync(Guid playerId)
        {
            return await _gameRepository.Players.FirstOrDefaultAsync(x => x.Id == playerId);
        }
        async Task<Player> IGameQueries.GetPlayerByUsernameAsync(string username)
        {
            return await _gameRepository.Players.FirstOrDefaultAsync(x => x.Name == username);
        }
        async Task<string> IGameQueries.GetPlayerBetLimitCodeOrNullAsync(Guid vipLevelId, Guid gameProviderId, string currency)
        {
            return await GetPlayerBetLimitCodeQuery(vipLevelId, gameProviderId, currency).SingleOrDefaultAsync();
        }

        private IQueryable<string> GetPlayerBetLimitCodeQuery(Guid vipLevelId, Guid gameProviderId, string currency)
        {
            return _gameRepository.VipLevelBetLimits
                .Where(x => x.VipLevelId == vipLevelId 
                            &&  x.GameProviderId == gameProviderId 
                            &&  x.CurrencyCode == currency)
                .Select(x =>x.BetLimit.Code);
        }

        async Task<string> IGameQueries.GetBrandCodeAsync(Guid brandId)
        {
            return await _gameRepository.Brands.Where(x => x.Id == brandId).Select(b => b.Code).SingleOrDefaultAsync();
        }

        public IEnumerable<GameAction> GetWinLossGameActions(Guid gameProviderId)
        {
            var gameActions = _gameRepository.GameActions
                .Include(o => o.Round)
                .Include(o => o.Round.Game)
                .Where(o => o.Round.Game.GameProviderId == gameProviderId)
                .Where(o => o.GameActionType == GameActionType.Won
                    || o.GameActionType==GameActionType.Lost);

            return gameActions;
        }

        [Permission(Permissions.View, Module = Modules.BetLevels)]
        public IEnumerable<BrandProductData> GetBrandProducts()
        {
            return _gameRepository.VipLevelBetLimits
                    .Select(x => new BrandProductData
                    {
                        BrandId = x.VipLevel.BrandId,
                        GameProviderId = x.GameProviderId,
                        BrandName = x.VipLevel.Brand.Name,
                        LicenseeId = x.VipLevel.Brand.Licensee.Id,
                        GameProviderName = x.GameProvider.Name,
                        CreatedBy = x.GameProvider.CreatedBy,
                        DateCreated = x.GameProvider.CreatedDate,
                        DateUpdated = x.GameProvider.UpdatedDate,
                        UpdatedBy = x.GameProvider.UpdatedBy
                    });
        }

        public IEnumerable<object> GetAssignedProductsData(IEnumerable<Guid> productIds)
        {
            return _gameRepository
                .GameProviders
                .Where(x => productIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Name });
        }

       
        public string GetProductName(Guid gameId)
        {
            var product = _gameRepository.GameProviderConfigurations.FirstOrDefault(x => x.Id == gameId);
            if (product != null)
                return product.Name;

            return string.Empty;
        }

        async Task<string> IGameQueries.GetMappedGameProviderCultureCodeAsync(Guid gameProviderId, string innerCultureCode)
        {
            var mappedCultureCode = await (from gpl in _gameRepository.GameProviderLanguages
                join gp in _gameRepository.GameProviders on gpl.GameProviderId equals gp.Id
                where gp.Id == gameProviderId && gpl.CultureCode == innerCultureCode
                select gpl.GameProviderCultureCode).SingleOrDefaultAsync();

            return mappedCultureCode ?? innerCultureCode;
        }

        async Task<string> IGameQueries.GetMappedGameProviderCurrencyCodeAsync(Guid gameProviderId, string innerCurrencyCode)
        {
            var mappedCurrencyCode = await (from gpc in _gameRepository.GameProviderCurrencies
                                           join gp in _gameRepository.GameProviders on gpc.GameProviderId equals gp.Id
                                           where gp.Id == gameProviderId && gpc.CurrencyCode == innerCurrencyCode
                                           select gpc.GameProviderCurrencyCode).SingleOrDefaultAsync();

            return mappedCurrencyCode ?? innerCurrencyCode;
        }

        public Lobby GetLobby(Guid brandId, bool isForMobile)
        {
            var platformType = isForMobile ? PlatformType.Mobile : PlatformType.Desktop;
            var brandLobbies = _gameRepository.BrandLobbies.Include(x => x.Lobby).Where(x => x.BrandId == brandId);
            // doing that we making assumption that we have only two lobbies for brand - one for mobile, and one for desktop
            return brandLobbies.First(x => x.Lobby.PlatformType == platformType).Lobby;
        }

        public BetLimitGroup GetBetLimitGroupByVipLevel(Guid? vipLevelId)
        {
            var vipLevelBetLimitGroup = _gameRepository.VipLevelBetLimitGroups.SingleOrDefault(x => x.VipLevelId == vipLevelId);
            if (vipLevelBetLimitGroup == null)
            {
                return null;
            }

            return _gameRepository.BetLimitGroups.Single(x => x.Id == vipLevelBetLimitGroup.BetLimitGroupId);
        }

        public BetLimitGroup GetBetLimitGroup(Guid betLimitGroupId)
        {
            return _gameRepository.BetLimitGroups.SingleOrDefault(x => x.Id == betLimitGroupId);
        }
    }

    public class PlayerNotFoundException : RegoException
    {
        public PlayerNotFoundException(string message) : base(message) { }
    }

    public class PlayerWalletNotFoundException : RegoException
    {
        public PlayerWalletNotFoundException(string message) : base(message) { }
    }
}