using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<SecurityRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            MigrationsDirectory = @"DataAccess\Security\Migrations";
        }
    }
}
