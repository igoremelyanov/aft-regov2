using System;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class HasWinningsAmountExeption : Exception
    {
        public HasWinningsAmountExeption()
            : base("Player's Has Winnings amount doesn't match configured criteria")
        {

        }
    }
}
