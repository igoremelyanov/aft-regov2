using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging.Migrations
{
    public class Configuration : DbMigrationsConfiguration<MessagingRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            MigrationsDirectory = @"DataAccess\Messaging\Migrations";
        }
    }
}