using System;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class TotalDepositAmountException : Exception
    {
        public TotalDepositAmountException()
            : base("Player's total deposit amount doesn't match configured criteria")
        {
            
        }
    }
}
