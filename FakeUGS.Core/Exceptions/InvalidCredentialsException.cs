using System;

namespace FakeUGS.Core.Exceptions
{
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException() : base("Invalid Credentials") { }
    }
}