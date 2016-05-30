using System.Data.Entity;

namespace AFT.RegoV2.Core.Common
{
    public interface IDocumentsRepository
    {
        IDbSet<FileMetadata> Metadatas { get; }

        int SaveChanges();
    }
}
