using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;
using Round = AFT.RegoV2.Core.Game.Entities.Round;

namespace AFT.RegoV2.Core.Game.Interfaces
{
    public interface IGameQueries : IApplicationService
    {
        IEnumerable<GameDTO>        GetGameDtos();
        GameDTO                     GetGameDto(Guid gameId);

        IEnumerable<BetLimitDTO>    GetBetLimits(Guid gameProviderId, Guid gameId);
        BetLimitDTO                 GetBetLimitDto(Guid id);
        Round                       GetRoundByGameActionId(Guid gameActiondId);
        List<Round>                 GetRoundHistory(Guid playerId, Guid gameId, int recordCount);

        Task<string>                GetPlayerBetLimitCodeOrNullAsync(Guid vipLevelId, Guid gameProviderId, string currency);
        Task<Player>                GetPlayerDataAsync(Guid playerId);
        

        IEnumerable<GameProvider>     GetGameProviders();
        IEnumerable<GameProviderDTO>  GetGameProviderDtos();
        IEnumerable<GameProviderDTO>  GetGameProviders(Guid brandId);
        
        PlayableBalanceInfo         GetPlayableBalance(Guid playerId);
        Task<PlayableBalanceInfo>   GetPlayableBalanceAsync(Guid playerId);
        Dictionary<Guid, decimal>   GetPlayableBalances(IEnumerable<Guid> playerIds);

        Player                      GetPlayerData(Guid playerId);
        IEnumerable<object>         GetAssignedProductsData(IEnumerable<Guid> productIds);


        Interface.Data.Brand GetBrand(string brandCode);
        Task<string>                GetBrandCodeAsync(Guid brandId);
        IEnumerable<BrandProductData> GetBrandProducts();

        void                        ValidateBatchIsUnique(string batchId, Guid gameProviderId);
        IEnumerable<GameAction>     GetWinLossGameActions(Guid gameProviderId);

        IEnumerable<GameProvider> GetGameProvidersWithGames(Guid brandId);

        Task<Interface.Data.Game> GetGameByIdAsync(Guid gameId);

        Task<GameProviderConfiguration> GetGameProviderConfigurationAsync(Guid brandId, Guid gameProviderId);
        Task<List<GameProviderConfiguration>> GetGameProviderConfigurationListAsync(Guid gameProviderId);

        Interface.Data.Game GetGameByExternalGameId(string externalGameId);

        Task<Player> GetPlayerByUsernameAsync(string username);

        Task<string> GetMappedGameProviderCultureCodeAsync(Guid gameProviderId, string playerCultureCode);

        Task<string> GetMappedGameProviderCurrencyCodeAsync(Guid gameProviderId, string playerCurrencyCode);

        Task<bool> ValidateSecurityKey(Guid gameProviderId, string securityKey);

        Lobby GetLobby(Guid brandId, bool isForMobile);
        BetLimitGroup GetBetLimitGroup(Guid betLimitGroupId);
        BetLimitGroup GetBetLimitGroupByVipLevel(Guid? vipLevelId);
    }
}