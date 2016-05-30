using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using FakeUGS.Core.Data;
using FakeUGS.Core.DataAccess.Mappings;
using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.DataAccess
{
    public class Repository : DbContext, IRepository
    {
        private const string Schema = "fakeUgs";

        public virtual IDbSet<Brand> Brands { get; set; }
        public virtual IDbSet<GameCurrency> Currencies { get; set; }
        public virtual IDbSet<GameCulture> Cultures { get; set; }
        public virtual IDbSet<Licensee> Licensees { get; set; }
        public virtual IDbSet<Lobby> Lobbies { get; set; }
        public virtual IDbSet<Player> Players { get; set; }

        public virtual IDbSet<Wallet> Wallets { get; set; }

        public virtual IDbSet<Round> Rounds { get; set; }
        public virtual IDbSet<GameAction> GameActions { get; set; }
        public virtual IDbSet<GameProvider> GameProviders { get; set; }
        public virtual IDbSet<Game> Games { get; set; }

        public virtual IDbSet<GameProviderConfiguration> GameProviderConfigurations { get; set; }
        public virtual IDbSet<VipLevelGameProviderBetLimit> VipLevelBetLimits { get; set; }
        public virtual IDbSet<GameProviderCurrency> GameProviderCurrencies { get; set; }

        public virtual IDbSet<GameProviderBetLimit> BetLimits { get; set; }
        public virtual IDbSet<GameProviderLanguage> GameProviderLanguages { get; set; }
        public virtual IDbSet<GameGroup> GameGroups { get; set; }
        public virtual IDbSet<GameGroupGame> GameGroupGames { get; set; }

        public Entities.Wallet GetWalletWithUPDLock(Guid playerId)
        {
            LockWallet(playerId);
            var wallet = Wallets
                .Include(w => w.Brand)
                .SingleOrDefault(x => x.PlayerId == playerId);

            if (wallet == null)
                throw new Exception("Wallet does not exist.");

            return new Entities.Wallet(wallet);
        }
        
        /// <returns></returns>
        public async Task<Entities.Wallet> GetWalletWithUPDLockAsync(Guid playerId)
        {

            LockWallet(playerId);
            var wallet = await Wallets
                .Include(w => w.Brand)
                .SingleOrDefaultAsync(x => x.PlayerId == playerId);

            if (wallet == null)
                throw new Exception("Wallet does not exist.");

            return new Entities.Wallet(wallet);
        }

        protected virtual void LockWallet(Guid playerId)
        {
            Database.ExecuteSqlCommand($"SELECT * FROM {Schema}.Wallets WITH (ROWLOCK, UPDLOCK) WHERE PlayerId = @p0", playerId);
        }


        static Repository()
        {
            Database.SetInitializer(new RepositoryInitializer());
        }

        public Repository(): base("name=Default")
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

            modelBuilder.Entity<Game>()
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
        }


        public Entities.Round GetRound(string roundId)
        {
            return GetRound(b => b.ExternalRoundId == roundId);
        }

        public Entities.Round GetRound(Expression<Func<Round, bool>> condition)
        {
            var round = Rounds
                .Include(x => x.GameActions)
                .Include(x => x.Brand)
                .SingleOrDefault(condition);

            return round == null ? null : new Entities.Round(round);
        }

        public GameAction GetGameActionByExternalTransactionId(string externalTransactionId, Guid gameProviderId)
        {
            var round = GetRound(x => x.GameActions.Any(t => t.ExternalTransactionId == externalTransactionId));

            return round?.Data.GameActions.Single(t => t.ExternalTransactionId == externalTransactionId);
        }

        public Entities.Round GetOrCreateRound(string roundId,
            Guid gameId, Guid playerId, Guid brandId)
        {
            if (Rounds.Any(x => x.ExternalRoundId == roundId))
            {
                return GetRound(roundId);
            }

            var brand = Brands.Single(x => x.Id == brandId);
            return new Entities.Round(roundId, gameId, playerId, brand);
        }

        public bool DoesGameActionExist(string externTransactionId, Guid gameProviderId)
        {
            return GameActions.Any(x => x.ExternalTransactionId == externTransactionId);
        }


        public List<Entities.Round> GetPlayerRounds(Guid playerId)
        {
            var rounds = Rounds
                .Include(x => x.GameActions)
                .Include(x => x.Brand)
                .Where(x => x.PlayerId == playerId)
                .OrderBy(x => x.CreatedOn)
                .ToList();

            return rounds.Select(x => new Entities.Round(x)).ToList();
        }

        public bool DoesBatchIdExist(string batchId, Guid gameProviderId)
        {
            return (batchId != null) && GetRound(b => b.GameActions.Any(tx => tx.ExternalBatchId == batchId)) != null;
        }

    }
    internal class RepositoryInitializer : MigrateDatabaseToLatestVersion<Repository, Configuration>
    {
    }
    public sealed class Configuration : DbMigrationsConfiguration<Repository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            MigrationsDirectory = @"DataAccess\Game\Migrations";
        }
    }
}