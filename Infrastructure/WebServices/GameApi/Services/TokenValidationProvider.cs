using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.Core.Game.Extensions;
using AFT.RegoV2.Core.Game.Providers;
using AFT.RegoV2.GameApi.ServiceContracts;

namespace AFT.RegoV2.GameApi.Services
{
    public interface ITokenValidationProvider
    {
        void ValidateToken(string token);
    }

    public sealed class TokenValidationProvider : ITokenValidationProvider
    {
        void ITokenValidationProvider.ValidateToken(string token)
        {
            Guid playerId;
            if (!Guid.TryParse(token, out playerId))
            {
                throw new InvalidTokenException(String.Format("Incorrect token string: {0}", token));
            }
        }
    }
}