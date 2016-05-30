using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game
{
    public class GameRepository : DbContext, IGameRepository
    {
        private const string Schema = "game";

        public virtual IDbSet<Round> Rounds { get; set; }
        public virtual IDbSet<GameAction> GameActions { get; set; }
        public virtual IDbSet<Core.Game.Interface.Data.Player> Players { get; set; }
        public virtual IDbSet<GameProviderConfiguration> GameProviderConfigurations { get; set; }
        public virtual IDbSet<BrandGameProviderConfiguration> BrandGameProviderConfigurations { get; set; }
        public virtual IDbSet<GameProvider> GameProviders { get; set; }
        public virtual IDbSet<Core.Game.Interface.Data.Game> Games { get; set; }

        public virtual IDbSet<Core.Game.Interface.Data.Brand> Brands { get; set; }
        public virtual IDbSet<Licensee> Licensees { get; set; }

        public virtual IDbSet<GameProviderBetLimit> BetLimits { get; set; }
        public virtual IDbSet<GameProviderLanguage> GameProviderLanguages { get; set; }
        public virtual IDbSet<GameProviderCurrency> GameProviderCurrencies { get; set; }
        public virtual IDbSet<VipLevel> VipLevels { get; set; }
        public virtual IDbSet<VipLevelGameProviderBetLimit> VipLevelBetLimits { get; set; }

        public virtual IDbSet<Wallet> Wallets { get; set; }

        public virtual IDbSet<GameCulture> Cultures { get; set; }
        public virtual IDbSet<GameCurrency> Currencies { get; set; }
        public virtual IDbSet<Lobby> Lobbies { get; set; }
        public virtual IDbSet<BrandLobby> BrandLobbies { get; set; }
        public virtual IDbSet<GameGroup> GameGroups { get; set; }
        public virtual IDbSet<GameGroupGame> GameGroupGames { get; set; }
        public virtual IDbSet<BetLimitGroup> BetLimitGroups { get; set; }
        public virtual IDbSet<VipLevelBetLimitGroup> VipLevelBetLimitGroups { get; set; }

        static GameRepository()
        {
            Database.SetInitializer(new GameRepositoryInitializer());
        }

        public GameRepository()
            : base("name=Default")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Configurations.Add(new RoundMap(Schema));
            modelBuilder.Configurations.Add(new GameActionMap(Schema));
            modelBuilder.Configurations.Add(new PlayerMap(Schema));
            modelBuilder.Configurations.Add(new GameProviderConfigurationsMap(Schema));
            modelBuilder.Configurations.Add(new GameProviderMap(Schema));

            modelBuilder.Configurations.Add(new BrandGameProviderConfigurationMap(Schema));

            modelBuilder.Entity<Core.Game.Interface.Data.Game>()
                .HasKey(x => x.Id)
                .ToTable("Games", Schema);

            modelBuilder.Configurations.Add(new BrandMap(Schema));
            modelBuilder.Configurations.Add(new BetLimitMap(Schema));

            modelBuilder.Configurations.Add(new TransactionMap(Schema));
            modelBuilder.Configurations.Add(new WalletMap(Schema));
            
            modelBuilder.Entity<VipLevel>()
                .ToTable("VipLevels", Schema);

            modelBuilder.Entity<Licensee>()
                .ToTable("Licensees", Schema);

            modelBuilder.Entity<VipLevelGameProviderBetLimit>()
                .HasKey(x => new { x.VipLevelId, x.BetLimitId })
                .ToTable("xref_VipLevelBetLimits", Schema);

            modelBuilder.Entity<GameCulture>()
                .HasKey(x => x.Code)
                .ToTable("Cultures", Schema);

            modelBuilder.Entity<GameCurrency>()
                .HasKey(x => x.Code)
                .ToTable("Currencies", Schema);

            modelBuilder.Entity<GameProviderLanguage>()
                .HasKey(x => x.Id)
                .ToTable("GameProviderLanguages", Schema);
            modelBuilder.Entity<GameProviderCurrency>()
                .HasKey(x => x.Id)
                .ToTable("GameProviderCurrencies", Schema);

            
            modelBuilder.Entity<Lobby>()
                .HasKey(x => x.Id)
                .ToTable("Lobbies", Schema);

            modelBuilder.Entity<GameGroup>()
                .HasKey(x => x.Id)
                .ToTable("GameGroups", Schema);

            modelBuilder.Configurations.Add(new BrandLobbyMap(Schema));
            modelBuilder.Configurations.Add(new GameGroupGameMap(Schema));

            modelBuilder.Entity<BetLimitGroup>()
                .HasKey(x => x.Id)
                .ToTable("BetLimitGroups", Schema);

            modelBuilder.Configurations.Add(new VipLevelBetLimitGroupMap(Schema));
        }


        public Core.Game.Entities.Round GetRound(string roundId)
        {
            return GetRound(b => b.ExternalRoundId == roundId);
        }

        public Core.Game.Entities.Round GetRound(Expression<Func<Round, bool>> condition)
        {
            var round = Rounds
                .Include(x => x.GameActions)
                .Include(x => x.Brand)
                .SingleOrDefault(condition);

            return round == null ? null : new Core.Game.Entities.Round(round);
        }

        public Task<Guid> GetGameProviderIdByGameIdAsync(Guid gameId)
        {
            return GameProviderConfigurations.Where(ge => ge.Id == gameId).Select(ge => ge.GameProviderId).FirstOrDefaultAsync();
        }

        public GameAction GetGameActionByExternalTransactionId(string externalTransactionId, Guid gameProviderId)
        {
            var round = GetRound(x => x.GameActions.Any(t => t.ExternalTransactionId == externalTransactionId));

            return (round == null) ? null :
                round.Data.GameActions.Single(t => t.ExternalTransactionId == externalTransactionId);
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
            var rounds = Rounds
                .Include(x => x.GameActions)
                .Include(x => x.Brand)
                .Where(x => x.PlayerId == playerId)
                .OrderBy(x => x.CreatedOn)
                .ToList();

            return rounds.Select(x => new Core.Game.Entities.Round(x)).ToList();
        }

        public List<GameProvider> GetGameProviderList()
        {
            return GameProviders
                .Include(g => g.GameProviderConfigurations)
                .Include(g => g.GameProviderCurrencies)
                .ToList();
        }


        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }

        public override Task<int> SaveChangesAsync()
        {
            try
            {
                return base.SaveChangesAsync();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }

        public bool DoesBatchIdExist(string batchId, Guid gameProviderId)
        {
            return (batchId != null) && GetRound(b => b.GameActions.Any(tx => tx.ExternalBatchId == batchId)) != null;
        }

        public bool DoesGameActionExist(string externTransactionId, Guid gameProviderId)
        {
            return GameActions.Any(x => x.ExternalTransactionId == externTransactionId);
        }

        /// <summary>
        /// This method returns player wallet and locks the record with UPD (update) lock for the scope of the transaction
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public Core.Game.Entities.Wallet GetWalletWithUPDLock(Guid playerId)
        {

            LockWallet(playerId);
            var wallet = Wallets
                .Include(w => w.Brand)
                .SingleOrDefault(x => x.PlayerId == playerId);

            if (wallet == null)
                throw new RegoException("Wallet does not exist.");

            return new Core.Game.Entities.Wallet(wallet);
        }

        /// <summary>
        /// This method returns player wallet and locks the record with UPD (update) lock for the scope of the transaction
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public async Task<Core.Game.Entities.Wallet> GetWalletWithUPDLockAsync(Guid playerId)
        {

            LockWallet(playerId);
            var wallet = await Wallets
                .Include(w => w.Brand)
                .SingleOrDefaultAsync(x => x.PlayerId == playerId );

            if (wallet == null)
                throw new RegoException("Wallet does not exist.");

            return new Core.Game.Entities.Wallet(wallet);
        }

        protected virtual void LockWallet(Guid playerId)
        {
            Database.ExecuteSqlCommand("SELECT * FROM game.Wallets WITH (ROWLOCK, UPDLOCK) WHERE PlayerId = @p0", playerId);
        }
    }
}
