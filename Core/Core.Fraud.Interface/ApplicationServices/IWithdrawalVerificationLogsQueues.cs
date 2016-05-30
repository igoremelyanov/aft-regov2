using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.ApplicationServices
{
    public interface IWithdrawalVerificationLogsQueues
    {
        bool HasFailedCriteras(Guid withdrawalId, VerificationType type);
        IEnumerable<WithdrawalVerificationLog> GetStatuses(Guid withdrawalId, VerificationType type);
        OfflineWithdrawVerificationStatusDTO RiskProfileCheckStatus(Guid id, string brandName, string licenseeName, string playerName);
        OfflineWithdrawVerificationStatusDTO AutoVerificationStatus(Guid id, string brandName, string licenseeName, string playerName);
    }
}