using System.Data.Entity;
using AFT.RegoV2.Infrastructure.DataAccess.Messaging.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging
{
    public class MessagingRepositoryInitializer : MigrateDatabaseToLatestVersion<MessagingRepository, Configuration>
    {
    }
}