using System;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.ApplicationServices
{
    public interface IWithdrawalVerificationLogsCommands
    {
        void LogWithdrawalVerificationStep(Guid withdrawalId, bool isSuccess, VerificationType type,
            VerificationStep step, string completeRuleDesc, string ruleRequiredValues, string criteriaActualValue);
    }
}