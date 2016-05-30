using System;

using AFT.RegoV2.Core.Game.Exceptions;

namespace AFT.RegoV2.Core.Game.Providers
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
                    throw new InvalidTokenException(String.Format("Incorrect token string: {0}", tokenString));
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
                    String.Format("Token parsing general exception. {0}. Token string: {1} ", ex.Message, tokenString));
            }
        }
        string ITokenProvider.Encrypt(Guid playerId)
        {
            return playerId.ToString();
        }
    }
}