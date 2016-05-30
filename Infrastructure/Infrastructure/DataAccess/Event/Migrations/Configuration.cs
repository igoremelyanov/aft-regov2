using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Event.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<EventRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"DataAccess\Event\Migrations";
        }

        // To rebuild the InitialCreate file use this command in the PM Console:
        //
        // Add-Migration InitialCreate -ConfigurationTypeName AFT.RegoV2.Infrastructure.DataAccess.Event.Migrations.Configuration -Force
        //
        // (set Infrastructure as Default Project)
    }
}
