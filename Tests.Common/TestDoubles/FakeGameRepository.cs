using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using Player = AFT.RegoV2.Core.Game.Interface.Data.Player;
using VipLevel = AFT.RegoV2.Core.Game.Interface.Data.VipLevel;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeGameRepository : IGameRepository
    {
        private readonly FakeDbSet<Round> _bets = new FakeDbSet<Round>();
        private readonly FakeDbSet<GameAction> _betTransactions = new FakeDbSet<GameAction>();
        private readonly FakeDbSet<Player> _players = new FakeDbSet<Player>();
        private readonly FakeDbSet<GameProviderConfiguration> _configurations = new FakeDbSet<GameProviderConfiguration>();
        private readonly FakeDbSet<BrandGameProviderConfiguration> _brandGameProviderConfigurations = new FakeDbSet<BrandGameProviderConfiguration>();
        private readonly FakeDbSet<GameProvider> _gameProviders = new FakeDbSet<GameProvider>();
        private readonly FakeDbSet<Game> _games = new FakeDbSet<Game>();
        private readonly FakeDbSet<Brand> _brands = new FakeDbSet<Brand>();
        private readonly FakeDbSet<GameProviderBetLimit> _betLimits = new FakeDbSet<GameProviderBetLimit>();
        private readonly FakeDbSet<VipLevel> _vipLevels = new FakeDbSet<VipLevel>();
        private readonly FakeDbSet<VipLevelGameProviderBetLimit> _vipLevelBetLimit = new FakeDbSet<VipLevelGameProviderBetLimit>();
        private readonly FakeDbSet<Licensee> _licensees = new FakeDbSet<Licensee>();
        private readonly FakeDbSet<Wallet> _wallets = new FakeDbSet<Wallet>();
        private readonly FakeDbSet<GameCulture> _cultures = new FakeDbSet<GameCulture>();
        private readonly FakeDbSet<GameCurrency> _currencies = new FakeDbSet<GameCurrency>();
        private readonly FakeDbSet<GameProviderLanguage> _gpLanguages = new FakeDbSet<GameProviderLanguage>();
        private readonly FakeDbSet<GameProviderCurrency> _gpCurrencies = new FakeDbSet<GameProviderCurrency>();
        private readonly FakeDbSet<Lobby> _gpLobbies = new FakeDbSet<Lobby>();
        private readonly FakeDbSet<GameGroup> _gpGameGroups = new FakeDbSet<GameGroup>();
        private readonly FakeDbSet<GameGroupGame> _gpGameGroupGames = new FakeDbSet<GameGroupGame>();
        private readonly FakeDbSet<BrandLobby> _gpBrandLobbies = new FakeDbSet<BrandLobby>();
        private readonly FakeDbSet<BetLimitGroup> _gpBetLimitGroups = new FakeDbSet<BetLimitGroup>();
        private readonly FakeDbSet<VipLevelBetLimitGroup> _gpVipLevelBetLimitGroups = new FakeDbSet<VipLevelBetLimitGroup>();

        public EventHandler SavedChanges;

        public IDbSet<Round> Rounds { get { return _bets; } }
        public IDbSet<GameAction> GameActions { get { return _betTransactions; } }
        public IDbSet<Player> Players { get { return _players; } }

        
        public IDbSet<GameProviderConfiguration>     GameProviderConfigurations { get { return _configurations; } }
        public IDbSet<BrandGameProviderConfiguration> BrandGameProviderConfigurations { get { return _brandGameProviderConfigurations; } }
        public IDbSet<Game> Games { get { return _games; } }
        public IDbSet<GameProvider> GameProviders { get { return _gameProviders; } }
        public IDbSet<GameProviderBetLimit> BetLimits { get { return _betLimits; } }
        public IDbSet<GameProviderLanguage> GameProviderLanguages { get { return _gpLanguages; } }
        public IDbSet<GameProviderCurrency> GameProviderCurrencies { get { return _gpCurrencies; } }

        public IDbSet<VipLevel> VipLevels { get { return _vipLevels; } }
        public IDbSet<VipLevelGameProviderBetLimit> VipLevelBetLimits { get { return _vipLevelBetLimit; } }
        public IDbSet<Brand> Brands { get { return _brands; } }
        public IDbSet<Licensee> Licensees { get { return _licensees; } }
        public IDbSet<GameCulture> Cultures { get { return _cultures; } }
        public IDbSet<GameCurrency> Currencies { get { return _currencies; } }
        public IDbSet<Wallet> Wallets
        {
            get { return _wallets; }
        }

        
        public IDbSet<Lobby> Lobbies { get { return _gpLobbies; } }
        public IDbSet<GameGroup> GameGroups { get { return _gpGameGroups; } }
        public IDbSet<GameGroupGame> GameGroupGames { get { return _gpGameGroupGames; } }
        public IDbSet<BrandLobby> BrandLobbies { get { return _gpBrandLobbies; } }

        public IDbSet<BetLimitGroup> BetLimitGroups { get { return _gpBetLimitGroups; } }
        public IDbSet<VipLevelBetLimitGroup> VipLevelBetLimitGroups { get { return _gpVipLevelBetLimitGroups; } }

        public int SaveChanges()
        {
            if (_wallets.Any() && !_wallets.Any(x => x.Transactions.AllElementsAreUnique()))
            {
                throw new RegoException("Transactions with duplicate Ids were found");
            }

            var handler = SavedChanges;
            if (handler != null)
                handler(this, EventArgs.Empty);

            return 0;
        }

        public Task<int> SaveChangesAsync()
        {
            SaveChanges();

            return Task.FromResult(0);
        }



        public Core.Game.Entities.Round GetRound(string roundId)
        {
            return GetRound(b => b.ExternalRoundId == roundId);
        }
        public Core.Game.Entities.Round GetRound(System.Linq.Expressions.Expression<Func<Round, bool>> condition)
        {
            var roundDto = Rounds.Include(x => x.GameActions).Include(x => x.Brand).SingleOrDefault(condition);

            return (roundDto != null) ? new Core.Game.Entities.Round(roundDto) : null;
        }

        public Core.Game.Entities.Round GetOrCreateRound(string roundId,
            Guid gameId, Guid playerId, Guid brandId)
        {
            if (Rounds.Any(x => x.ExternalRoundId == roundId))
            {
                return GetRound(roundId);
            }

            var brand = Brands.Single(x => x.Id == brandId);
            return new Core.Game.Entities.Round(roundId, gameId, playerId, brand);
        }

        public List<Core.Game.Entities.Round> GetPlayerRounds(Guid playerId)
        {
            return Rounds
                .Include(x => x.GameActions)
                .Include(x => x.Brand)
                .Where(x => x.PlayerId == playerId)
                .Select(x => new Core.Game.Entities.Round(x))
                .ToList();
        }

        public List<GameProvider> GetGameProviderList()
        {
            return GameProviders
                .Include(x => x.GameProviderConfigurations)
                .Include(x => x.GameProviderCurrencies)
                .ToList();
        }

        public Task<Guid> GetGameProviderIdByGameIdAsync(Guid gameId)
        {
            return GameProviderConfigurations.Where(x => x.Id == gameId).Select(ge => ge.GameProviderId).FirstOrDefaultAsync();
        }

        public GameAction GetGameActionByExternalTransactionId(string externalTransactionId, Guid gameProviderId)
        {
            var round = GetRound(x => x.GameActions.Any(t => t.ExternalTransactionId == externalTransactionId));

            return (round == null) ? null :
                round.Data.GameActions.Single(t => t.ExternalTransactionId == externalTransactionId);
        }


        public bool DoesBatchIdExist(string batchId, Guid gameProviderId)
        {
            return (batchId != null) && GetRound(b => b.GameActions.Any(tx => tx.ExternalBatchId == batchId)) != null;
        }

        bool IGameRepository.DoesGameActionExist(string externTransactionId, Guid gameProviderId)
        {
            return GetRound(b => b.GameActions.Any(x => x.ExternalTransactionId == externTransactionId)) != null;
        }




        public Core.Game.Entities.Wallet GetWalletWithUPDLock(Guid playerId)
        {
            var wallet = Wallets
                .SingleOrDefault(x => x.PlayerId == playerId );

            if (wallet == null)
                throw new RegoException("Wallet does not exist.");

            return new Core.Game.Entities.Wallet(wallet);
        }

        public async Task<Core.Game.Entities.Wallet> GetWalletWithUPDLockAsync(Guid playerId)
        {
            var wallet = await Wallets
                .SingleOrDefaultAsync(x => x.PlayerId == playerId);

            if (wallet == null)
                throw new RegoException("Wallet does not exist.");

            return new Core.Game.Entities.Wallet(wallet);
        }
    }
}
