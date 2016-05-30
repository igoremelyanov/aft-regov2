using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<BrandRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            MigrationsDirectory = @"DataAccess\Brand\Migrations";
        }
    }
}