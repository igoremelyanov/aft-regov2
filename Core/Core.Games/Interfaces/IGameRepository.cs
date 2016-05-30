using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interface.Data;

namespace AFT.RegoV2.Core.Game.Interfaces
{
    public interface IGameRepository
    {
        IDbSet<Round>               Rounds { get; }
        IDbSet<GameAction>          GameActions { get; }
        IDbSet<Player>              Players { get; }
        IDbSet<GameProviderConfiguration>        GameProviderConfigurations { get; }
        IDbSet<BrandGameProviderConfiguration> BrandGameProviderConfigurations { get; }
        IDbSet<GameProvider>          GameProviders { get; }
        IDbSet<Interface.Data.Game> Games { get; }
        IDbSet<Interface.Data.Lobby> Lobbies { get; }
        IDbSet<BrandLobby> BrandLobbies { get; }
        IDbSet<GameGroup> GameGroups { get; }
        IDbSet<GameGroupGame> GameGroupGames { get; }

        IDbSet<Interface.Data.Brand> Brands { get; }
        IDbSet<Licensee>            Licensees { get; }

        IDbSet<GameProviderBetLimit>            BetLimits { get; }
        IDbSet<GameProviderLanguage>            GameProviderLanguages { get; }
        IDbSet<GameProviderCurrency>            GameProviderCurrencies { get; }

        IDbSet<VipLevel> VipLevels { get; }
        IDbSet<VipLevelGameProviderBetLimit>    VipLevelBetLimits { get; }

        IDbSet<GameCulture> Cultures { get; }
        IDbSet<GameCurrency>            Currencies { get; }

        IDbSet<BetLimitGroup> BetLimitGroups { get; }
        IDbSet<VipLevelBetLimitGroup> VipLevelBetLimitGroups { get; }

        Entities.Round             GetRound(string roundId);
        Entities.Round             GetRound(Expression<Func<Round, bool>> condition);
        Entities.Round             GetOrCreateRound(string roundId, Guid gameId, Guid playerId, Guid brandId);
        List<Entities.Round>       GetPlayerRounds(Guid playerId);

        GameAction GetGameActionByExternalTransactionId(string externalTransactionId, Guid gameProviderId);
        bool DoesBatchIdExist(string batchId, Guid gameProviderId);
        bool DoesGameActionExist(string externalTransactionId, Guid gameProviderId);

        List<GameProvider> GetGameProviderList();

        Task<Guid>      GetGameProviderIdByGameIdAsync(Guid gameId);

        IDbSet<Wallet> Wallets { get; }

        Entities.Wallet GetWalletWithUPDLock(Guid playerId);
        Task<Entities.Wallet> GetWalletWithUPDLockAsync(Guid playerId);

        int SaveChanges();
        Task<int> SaveChangesAsync();
        
    }
}
