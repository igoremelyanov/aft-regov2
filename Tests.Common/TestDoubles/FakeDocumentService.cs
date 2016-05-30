using System;
using AFT.RegoV2.Core.Common;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeDocumentService : IDocumentService
    {
        public Guid SaveFile(string fileName, byte[] content, Guid playerId, Guid brandId, Guid licenseeId)
        {
            return Guid.Empty;
        }

        public byte[] GetFile(Guid fileId, Guid playerId)
        {
            return new byte[] { };
        }
    }
}
