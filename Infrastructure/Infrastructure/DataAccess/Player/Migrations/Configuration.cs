using System.Data.Entity.Migrations;
namespace AFT.RegoV2.Infrastructure.DataAccess.Player.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<PlayerRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            MigrationsDirectory = @"DataAccess\Player\Migrations";
        }
    }
}