using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Settings.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<SettingsRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            MigrationsDirectory = @"DataAccess\Settings\Migrations";
        }
    }
}
