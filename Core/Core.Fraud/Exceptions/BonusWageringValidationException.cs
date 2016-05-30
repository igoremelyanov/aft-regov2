using System;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class BonusWageringValidationException : Exception
    {
        public BonusWageringValidationException()
            : base("Player has incompleted bonus wagering requirements.")
        {
            
        }
    }
}
