using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Player.Mappings;

namespace AFT.RegoV2.Infrastructure.DataAccess.Player
{
    public class PlayerRepository : DbContext, IPlayerRepository
    {
        public const string Schema = "player";

        static PlayerRepository()
        {
            Database.SetInitializer(new PlayerRepositoryInitializer());
        }

        public PlayerRepository(): base("name=Default")
        {
        }

        public IDbSet<Core.Common.Data.Player.Player> Players { get; set; }
        public IDbSet<PlayerBetStatistics> PlayerBetStatistics { get; set; }
        public IDbSet<VipLevel> VipLevels { get; set; }
        public IDbSet<SecurityQuestion> SecurityQuestions { get; set; }
        public IDbSet<PlayerActivityLog> PlayerActivityLog { get; set; }
        public IDbSet<PlayerInfoLog> PlayerInfoLog { get; set; }
        public IDbSet<Core.Common.Data.Player.Brand> Brands { get; set; }
        public IDbSet<IdentificationDocumentSettings> IdentificationDocumentSettings { get; set; }
        public IDbSet<BankAccount> BankAccounts { get; set; }
        public IDbSet<Bank> Banks { get; set; }
        public IDbSet<OnSiteMessage> OnSiteMessages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(Schema);
            modelBuilder.Configurations.Add(new PlayerMap());
            modelBuilder.Configurations.Add(new PlayerBetStatisticsMap());
            modelBuilder.Configurations.Add(new IdentityVerificationMap());
            modelBuilder.Configurations.Add(new OnSiteMessageMap());
            modelBuilder.Configurations.Add(new IdentificationDocumentSettingsMap());
            modelBuilder.Configurations.Add(new BankAccountMap());
            modelBuilder.Entity<PlayerActivityLog>().ToTable("PlayerActivityLog");
            modelBuilder.Entity<PlayerInfoLog>().ToTable("PlayerInfoLog");
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
    }
}
