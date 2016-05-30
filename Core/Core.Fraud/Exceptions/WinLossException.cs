using System;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class WinLossException : Exception
    {
        public WinLossException() : base("Player's winn/loss amount doesn't match configured criteria")
        {
        }
    }
}
