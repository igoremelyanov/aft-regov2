using System;

using AFT.RegoV2.Core.Game.Interface.Data;

namespace AFT.RegoV2.Core.Game.Interfaces
{
    public interface IBrandCredentialsQueries
    {
        BrandCredentials Get(Guid brandId);
    }
}
