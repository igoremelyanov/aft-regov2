using System;

using FakeUGS.Core.Exceptions;

namespace FakeUGS.Core.Services
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