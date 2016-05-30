using System.Data.Entity;
using AFT.RegoV2.Core.Common;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeDocumentsRepository : IDocumentsRepository
    {
        private readonly FakeDbSet<FileMetadata> _metadatas = new FakeDbSet<FileMetadata>();

        public IDbSet<FileMetadata> Metadatas
        {
            get { return _metadatas; }
        }

        public int SaveChanges()
        {
            return 0;
        }
    }
}
