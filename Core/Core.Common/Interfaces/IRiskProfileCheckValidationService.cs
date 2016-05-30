using System;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IRiskProfileCheckValidationService
    {
        void Validate(Guid withdrawalId);
    }
}