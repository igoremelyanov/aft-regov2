using System;

using FakeUGS.Core.Exceptions;

namespace FakeUGS.Core.Interfaces
{
    public interface ITokenProvider
    {
        Guid Decrypt(string tokenString);
        string Encrypt(Guid playerId);
    }

    public sealed class TokenProvider : ITokenProvider
    {
        Guid ITokenProvider.Decrypt(string tokenString)
        {
            try
            {
                if (String.IsNullOrEmpty(tokenString))
                {
                    throw new ArgumentException("Missing token string");
                }

                Guid playerId;
                if (!Guid.TryParse(tokenString, out playerId))
                {
                    throw new InvalidTokenException($"Incorrect token string: {tokenString}");
                }

                return playerId;
            }
            catch (InvalidTokenException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new InvalidTokenException(
                    $"Token parsing general exception. {ex.Message}. Token string: {tokenString} ");
            }
        }
        string ITokenProvider.Encrypt(Guid playerId)
        {
            return playerId.ToString();
        }
    }
}
