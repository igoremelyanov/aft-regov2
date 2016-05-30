using System.Data.Entity;
using AFT.RegoV2.Infrastructure.DataAccess.Player.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Player
{
    public class PlayerRepositoryInitializer : MigrateDatabaseToLatestVersion<PlayerRepository, Configuration>
    {
    }
}