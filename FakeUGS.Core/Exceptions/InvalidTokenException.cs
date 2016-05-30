using System;

namespace FakeUGS.Core.Exceptions
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException() {}
        public InvalidTokenException(string message) : base(message){}
        public InvalidTokenException(string message, Exception ex) : base(message, ex){}
    }
}