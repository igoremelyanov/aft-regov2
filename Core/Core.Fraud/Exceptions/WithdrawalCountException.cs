using System;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class WithdrawalCountException : Exception
    {
        public WithdrawalCountException() : base("Withdrawal count doesn't valid for current operation.")
        {
            
        }
    }
}
