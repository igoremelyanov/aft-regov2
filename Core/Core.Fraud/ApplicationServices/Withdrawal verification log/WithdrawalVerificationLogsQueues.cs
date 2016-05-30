using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Fraud.Interface;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class WithdrawalVerificationLogsQueues : IWithdrawalVerificationLogsQueues
    {
        private readonly IFraudRepository _repository;

        public WithdrawalVerificationLogsQueues(IFraudRepository repository)
        {
            _repository = repository;
        }

        public bool HasFailedCriteras(Guid withdrawalId, VerificationType type)
        {
            return _repository
                .WithdrawalVerificationLogs
                .Where(x => x.WithdrawalId == withdrawalId)
                .Where(x => x.VerificationType == type)
                .Any(x => !x.IsSuccess);
        }

        public IEnumerable<WithdrawalVerificationLog> GetStatuses(Guid withdrawalId, VerificationType type)
        {
            return _repository
                .WithdrawalVerificationLogs
                .Where(x => x.WithdrawalId == withdrawalId && x.VerificationType == type)
                .ToList()
                .Select(x => new WithdrawalVerificationLog()
                {
                    Id = x.Id,
                    WithdrawalId = x.WithdrawalId,
                    VerificationStep = x.VerificationStep,
                    VerificationType = x.VerificationType,
                    VerificationRule = x.VerificationRule,
                    IsSuccess = x.IsSuccess,
                    CurrentValue = x.CurrentValue,
                    RuleRequiredValue = x.RuleRequiredValue
                });
        }

        public OfflineWithdrawVerificationStatusDTO RiskProfileCheckStatus(Guid id, 
            string brandName, 
            string licenseeName, 
            string playerName)
        {
            var logs = GetStatuses(id, VerificationType.RiskProfileCheck)
                .ToList()
                .Select(x => new WithdrawalVerificationLog
                {
                    VerificationRule = x.VerificationRule,
                    Status = RiskResultConverter.GetStatusForRpc(x.IsSuccess, x.VerificationStep).ToString(),
                    RuleRequiredValue = x.RuleRequiredValue,
                    CurrentValue = x.CurrentValue
                })
                .OrderBy(x => x.VerificationRule)
                .ToArray();

            var verificationHeader = new VerificationDialogHeaderValues
            {
                BrandName = brandName,
                LicenseeName = licenseeName,
                PlayerName = playerName,
                StatusSuccess = logs.All(x => x.Status == "Low") ? "Low" : "High"
            };

            return new OfflineWithdrawVerificationStatusDTO()
            {
                ListOfAppliedChecks = logs,
                VerificationDialogHeaderValues = verificationHeader
            };
        }

        public OfflineWithdrawVerificationStatusDTO AutoVerificationStatus(Guid id, 
            string brandName, 
            string licenseeName, 
            string playerName)
        {
            var logs = GetStatuses(id, VerificationType.AutoVerification)
                .ToList()
                .Select(x =>
                new WithdrawalVerificationLog
                {
                    VerificationRule = x.VerificationRule,
                    Status = x.IsSuccess ? "success" : "failed",
                    RuleRequiredValue = x.RuleRequiredValue,
                    CurrentValue = x.CurrentValue
                })
                .OrderBy(x => x.RuleRequiredValue)
                .ToArray();

            var verificationHeader = new VerificationDialogHeaderValues
            {
                BrandName = brandName,
                LicenseeName = licenseeName,
                PlayerName = playerName,
                StatusSuccess = logs.All(x => x.Status != "failed") ? "Succeeded" : "Failed",
                
            };

            return new OfflineWithdrawVerificationStatusDTO()
            {
                ListOfAppliedChecks = logs,
                VerificationDialogHeaderValues = verificationHeader
            };
        }
    }
}
