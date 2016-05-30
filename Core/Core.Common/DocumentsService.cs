using System;
using System.Linq;

namespace AFT.RegoV2.Core.Common
{
    public class DocumentsService : IDocumentService
    {
        private readonly IDocumentsRepository _repository;
        private readonly IFileStorage _fileStorage;

        public DocumentsService(IDocumentsRepository repository, IFileStorage fileStorage)
        {
            _repository = repository;
            _fileStorage = fileStorage;
        }

        public Guid SaveFile(string fileName, byte[] content, Guid playerId, Guid brandId, Guid licenseeId)
        {
            var fileId = _fileStorage.Save(fileName, content);

            _repository.Metadatas.Add(new FileMetadata
            {
                FileId = fileId,
                LicenseeId = licenseeId,
                BrandId = brandId,
                PlayerId = playerId
            });

            _repository.SaveChanges();

            return fileId;
        }

        public byte[] GetFile(Guid fileId, Guid playerId)
        {
            if (_repository.Metadatas.Any(o => o.PlayerId == playerId && o.FileId == fileId))
                return _fileStorage.Get(fileId);

            return new byte[] { };
        }
    }

    public interface IDocumentService
    {
        Guid SaveFile(string fileName, byte[] content, Guid playerId, Guid brandId, Guid licenseeId);
        byte[] GetFile(Guid fileId, Guid playerId);
    }
}
