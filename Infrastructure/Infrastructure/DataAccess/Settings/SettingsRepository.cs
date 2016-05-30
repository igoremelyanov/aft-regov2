using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using AFT.RegoV2.Core.Settings.Interface.Data;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Settings.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Settings
{
    public class SettingsRepository : DbContext, ISettingsRepository
    {
        private const string Schema = "settings";

        static SettingsRepository()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<SettingsRepository, Configuration>());
        }

        public SettingsRepository() : base("name=Default") { }

        public virtual IDbSet<SettingsItem> Settings { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SettingsItem>()
                .ToTable("Settings", Schema)
                .HasKey(x => x.Id)
                .Property(x => x.Key)
                    .HasColumnAnnotation(
                        IndexAnnotation.AnnotationName,
                        new IndexAnnotation(new IndexAttribute("IX_Key", 1) { IsUnique = true }));
        }
    }
}
