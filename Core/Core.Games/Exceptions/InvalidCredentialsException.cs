using System;

namespace AFT.RegoV2.Core.Game.Exceptions
{
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException() : base("Invalid Credentials") { }
    }
}