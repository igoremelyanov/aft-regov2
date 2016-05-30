using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Threading.Tasks;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.Interfaces
{
    public interface IRepository
    {
        IDbSet<Wallet> Wallets { get; set; }
        IDbSet<Brand> Brands { get; set; }
        IDbSet<GameCurrency> Currencies { get; set; }
        IDbSet<GameCulture> Cultures { get; set; }
        IDbSet<Licensee> Licensees { get; set; }
        IDbSet<Lobby> Lobbies { get; set; }
        IDbSet<Player> Players { get; set; }

        IDbSet<Round> Rounds { get; set; }
        IDbSet<GameAction> GameActions { get; set; }
        IDbSet<GameProvider> GameProviders { get; set; }
        IDbSet<Game> Games { get; set; }

        IDbSet<GameProviderConfiguration> GameProviderConfigurations { get; }
        IDbSet<VipLevelGameProviderBetLimit> VipLevelBetLimits { get; }
        IDbSet<GameProviderBetLimit> BetLimits { get; }
        IDbSet<GameProviderLanguage> GameProviderLanguages { get; }
        IDbSet<GameProviderCurrency> GameProviderCurrencies { get; }

        IDbSet<GameGroup> GameGroups { get; }
        IDbSet<GameGroupGame> GameGroupGames { get; }

        Entities.Wallet GetWalletWithUPDLock(Guid playerId);
        Task<Entities.Wallet> GetWalletWithUPDLockAsync(Guid playerId);

        Entities.Round GetRound(string roundId);
        Entities.Round GetRound(Expression<Func<Round, bool>> condition);
        Entities.Round GetOrCreateRound(string roundId, Guid gameId, Guid playerId, Guid brandId);

        GameAction GetGameActionByExternalTransactionId(string externalTransactionId, Guid gameProviderId);
        bool DoesGameActionExist(string externalTransactionId, Guid gameProviderId);


        List<Entities.Round> GetPlayerRounds(Guid playerId);
        bool DoesBatchIdExist(string batchId, Guid gameProviderId);

        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
