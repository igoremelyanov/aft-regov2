using System;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class AutoWagerCheckException : Exception
    {
        public AutoWagerCheckException()
            : base("Deposit wagering requirement has not been completed.")
        {
            
        }
    }
}
