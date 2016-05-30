using System;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class FraudlentRiskException : Exception
    {
        public FraudlentRiskException() : base("User has been tagged with fraudlent risk level.")
        {
        }
    }
}
