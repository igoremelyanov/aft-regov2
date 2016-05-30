using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Bonus.Core;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Infrastructure.DataAccess.Migrations;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Bonus.Infrastructure.DataAccess
{
    public class BonusRepository : DbContext, IBonusRepository
    {
        static BonusRepository()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<BonusRepository, Configuration>());
        }

        public BonusRepository() : base("name=Default") { }

        public virtual IDbSet<Template> Templates { get; set; }
        public virtual IDbSet<Core.Data.Bonus> Bonuses { get; set; }
        public virtual IDbSet<Player> Players { get; set; }
        public virtual IDbSet<Brand> Brands { get; set; }
        public virtual IDbSet<Game> Games { get; set; }

        public Core.Entities.Player GetLockedPlayer(Guid playerId)
        {
            LockPlayer(playerId);
            var data = Players
                .Include(p => p.Wallets.Select(o => o.Template))
                .Include(p => p.RiskLevels)
                .Include(p => p.ReferredWith)
                .SingleOrDefault(p => p.Id == playerId);
            if (data == null)
                throw new RegoException($"Player with Id '{playerId}' was not found");

            return new Core.Entities.Player(data);
        }

        public Core.Entities.Bonus GetLockedBonus(Guid bonusId)
        {
            var bonus = GetLockedBonusOrNull(bonusId);
            if (bonus == null)
                throw new RegoException("Bonus id is not valid.");

            return bonus;
        }

        public Core.Entities.Bonus GetLockedBonus(string bonusCode)
        {
            var bonus = GetLockedBonusOrNull(bonusCode);
            if (bonus == null)
                throw new RegoException("Bonus code is not valid.");

            return bonus;
        }

        public Core.Entities.Bonus GetLockedBonusOrNull(Guid bonusId)
        {
            LockBonus(bonusId);
            var bonusData = GetCurrentVersionBonuses()
                .SingleOrDefault(b => b.Id == bonusId);

            if (bonusData == null)
                return null;

            return new Core.Entities.Bonus(bonusData);
        }

        public Core.Entities.Bonus GetLockedBonusOrNull(string bonusCode)
        {
            var bonusData = GetCurrentVersionBonuses()
                .AsNoTracking()
                .Where(b => b.Template.Info.Mode == IssuanceMode.AutomaticWithCode || b.Template.Info.Mode == IssuanceMode.ManualByPlayer)
                .SingleOrDefault(b => b.Code == bonusCode);

            if (bonusData == null)
                return null;

            return GetLockedBonus(bonusData.Id);
        }

        public Core.Entities.BonusRedemption GetBonusRedemption(Guid playerId, Guid redemptionId)
        {
            var player = GetLockedPlayer(playerId);
            var bonusRedemption = player.BonusesRedeemed.SingleOrDefault(redemption => redemption.Id == redemptionId);
            if (bonusRedemption == null)
                throw new RegoException($"Redemption not found with id: {redemptionId}");

            return new Core.Entities.BonusRedemption(bonusRedemption);
        }

        public IQueryable<Core.Data.Bonus> GetCurrentVersionBonuses()
        {
            var currentIdVersion = Bonuses
                .GroupBy(bonus => bonus.Id)
                .Select(group => new { Id = group.Key, Version = group.Max(obj => obj.Version) });

            return Bonuses
                    .Include(bonus => bonus.Statistic)
                    .Where(bonus => currentIdVersion.Contains(new { bonus.Id, bonus.Version }));
        }

        public void RemoveGameContributionsForGame(Guid gameId)
        {
            var gameContributions = Templates
                .SelectMany(template => template.Wagering.GameContributions)
                .Where(contribution => contribution.GameId == gameId);
            foreach (var gameContribution in gameContributions)
            {
                Entry(gameContribution).State = EntityState.Deleted;
            }
        }

        /// <summary>
        /// This method returns player wallet and locks the record with UPD (update) lock for the scope of the transaction
        /// </summary>
        public Core.Entities.Wallet GetLockedWallet(Guid playerId, Guid? walletTemplateId = null)
        {
            LockPlayer(playerId);
            var wallets = Players.Single(p => p.Id == playerId).Wallets;
            var wallet = walletTemplateId.HasValue
                ? wallets.SingleOrDefault(x => x.Template.Id == walletTemplateId)
                : wallets.SingleOrDefault(w => w.Template.IsMain);

            if (wallet == null)
                throw new RegoException("Wallet does not exist.");

            return new Core.Entities.Wallet(wallet);
        }

        protected virtual void LockBonus(Guid bonusId)
        {
            Database.ExecuteSqlCommand("SELECT * FROM bonus.Bonuses WITH (ROWLOCK, UPDLOCK) WHERE Id = @p0", bonusId);
        }

        protected virtual void LockPlayer(Guid playerId)
        {
            Database.ExecuteSqlCommand("SELECT * FROM bonus.Players WITH (ROWLOCK, UPDLOCK) WHERE Id = @p0", playerId);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("bonus");
            modelBuilder.Entity<Core.Data.Bonus>()
                .ToTable("Bonuses")
                .HasKey(p => new { p.Id, p.Version });

            modelBuilder.Entity<Currency>()
                .HasKey(c => new { c.Id, c.BrandId });
            modelBuilder.Entity<Product>()
                .HasKey(c => new { c.Id, c.WalletTemplateId });

            modelBuilder.Entity<Template>()
                .HasKey(p => new { p.Id, p.Version });
            modelBuilder.Entity<Template>()
                .HasOptional(p => p.Info)
                .WithOptionalDependent();
            modelBuilder.Entity<Template>()
                .HasOptional(p => p.Availability)
                .WithOptionalDependent();
            modelBuilder.Entity<Template>()
                .HasOptional(p => p.Rules)
                .WithOptionalDependent();
            modelBuilder.Entity<Template>()
                .HasOptional(p => p.Wagering)
                .WithOptionalDependent();
            modelBuilder.Entity<Template>()
                .HasOptional(p => p.Notification)
                .WithOptionalDependent();

            modelBuilder.Entity<TierBase>()
                .ToTable("TemplateTiers")
                .Property(p => p.Reward).HasPrecision(16, 4);
            modelBuilder.Entity<RewardTier>()
                .Ignore(p => p.Tiers)
                .Ignore(p => p.HighDepositTiers);
            modelBuilder.Entity<HighDepositTier>()
                .Property(p => p.NotificationPercentThreshold).HasPrecision(3, 2);
            modelBuilder.ComplexType<RedemptionParams>();
            modelBuilder.Entity<Player>()
                .HasMany(p => p.RiskLevels)
                .WithMany();

            modelBuilder.Entity<Wallet>()
                .HasRequired(p => p.Template)
                .WithMany()
                .WillCascadeOnDelete(false);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}