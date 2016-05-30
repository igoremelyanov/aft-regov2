using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<GameRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            MigrationsDirectory = @"DataAccess\Game\Migrations";
        }
    }
}

