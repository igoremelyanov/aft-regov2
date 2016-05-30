using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Bonus.Infrastructure.DataAccess.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<BonusRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"DataAccess\Migrations";
        }
    }

    // To rebuild the InitialCreate file use this command in the PM Console:
    //
    // Add-Migration InitialCreate -ConfigurationTypeName AFT.RegoV2.Bonus.Infrastructure.DataAccess.Migrations.Configuration -Force
    //
    // (set Bonus.Infrastructure as Default Project)
}