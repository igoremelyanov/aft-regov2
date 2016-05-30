using System.Data.Entity;

using AFT.RegoV2.Infrastructure.DataAccess.Documents.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Documents
{
    public class DocumentsRepositoryInitializer : MigrateDatabaseToLatestVersion<DocumentsRepository, Configuration>
    {
    }
}