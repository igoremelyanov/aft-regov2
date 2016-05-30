using System;

namespace FakeUGS.Core.Exceptions
{
    public class InvalidAmountException : Exception
    {
        public InvalidAmountException()
        {    
        }

        public InvalidAmountException(string message) : base(message) 
        {
        }
    }
}