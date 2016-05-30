using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Auth.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<AuthRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            MigrationsDirectory = @"DataAccess\Auth\Migrations";
        }
    }
}
