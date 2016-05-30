using System;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IAVCValidationService
    {
        void Validate(Guid withdrawalId);
        bool Failed { get; }
    }
}