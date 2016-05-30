using System;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class DepositCountException : Exception
    {
        public DepositCountException() : base("Deposit count doesn't valid for current operation.")
        {
        }
    }
}
