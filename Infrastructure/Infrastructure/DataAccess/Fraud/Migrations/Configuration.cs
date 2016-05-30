using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations
{
    public class Configuration : DbMigrationsConfiguration<FraudRepository>
    {
        public const string Schema = "fraud";

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            MigrationsDirectory = @"DataAccess\Fraud\Migrations";
        }
    }
}