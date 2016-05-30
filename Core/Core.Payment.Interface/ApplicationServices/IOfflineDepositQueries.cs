using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IOfflineDepositQueries
    {
        IEnumerable<OfflineDeposit> GetPendingDeposits(Guid playerId);
        OfflineDeposit GetOfflineDeposit(Guid id);
        ValidationResult GetValidationResult(OfflineDepositRequest request);
    }
}
