using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Infrastructure.DataAccess.Documents
{
    public class DocumentsRepository : DbContext, IDocumentsRepository, ISeedable
    {
        public const string Schema = "doc";

        public IDbSet<FileMetadata> Metadatas { get; set; }

        static DocumentsRepository()
        {
            Database.SetInitializer(new DocumentsRepositoryInitializer());
        }

        public DocumentsRepository() : base("name=Default")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<FileMetadata>().ToTable("FileMetadatas", Schema);
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }

        public void Seed()
        {
			Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,
string.Format(@"
use [{0}]

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'doc' 
                 AND  TABLE_NAME = 'Documents'))
BEGIN
    USE Master
  
    DECLARE @uniqueDirName nvarchar(max) = CONVERT(nvarchar(max), NEWID())
    exec('ALTER DATABASE [{0}]
    SET FILESTREAM (NON_TRANSACTED_ACCESS = FULL, DIRECTORY_NAME = '''+@uniqueDirName+''')')

    ALTER DATABASE [{0}]
    ADD FILEGROUP [Documents]
    CONTAINS FILESTREAM

    DECLARE @dataPath nvarchar(max) = CONVERT( nvarchar(max), SERVERPROPERTY('InstanceDefaultDataPath')) + N'{0}-Documents'
    exec('ALTER DATABASE [{0}] 
    ADD FILE(NAME = N''Files'', FILENAME = ''' + @dataPath + ''')
    TO FILEGROUP [Documents]')

    exec('USE [{0}]
    CREATE TABLE [doc].[Documents] AS FileTable')
END", Database.Connection.Database));
		}
    }
}