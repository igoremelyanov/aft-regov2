using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Report.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<ReportRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            MigrationsDirectory = @"DataAccess\Report\Migrations";
        }
    }
}
