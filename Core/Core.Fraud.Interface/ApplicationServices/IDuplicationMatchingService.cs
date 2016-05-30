using System;

namespace AFT.RegoV2.Core.Fraud.Interface.ApplicationServices
{
    public interface IDuplicationMatchingService
    {
        void Match(Guid id);
    }
}
