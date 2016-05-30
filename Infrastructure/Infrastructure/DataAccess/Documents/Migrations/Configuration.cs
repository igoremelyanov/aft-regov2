using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Documents.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<DocumentsRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            MigrationsDirectory = @"DataAccess\Documents\Migrations";
        }
    }
}
