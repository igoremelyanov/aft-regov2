using System;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class WithdrawalVerificationLogsCommands : IWithdrawalVerificationLogsCommands
    {
        #region Fields

        private readonly IFraudRepository _repository;

        #endregion

        #region Constructors

        public WithdrawalVerificationLogsCommands(IFraudRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region Public methods

        public void LogWithdrawalVerificationStep(Guid withdrawalId, bool isSuccess, VerificationType type,
            VerificationStep step, string completeRuleDesc, string ruleRequiredValues, string criteriaActualValue)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var withdrawalVerificationLog = new WithdrawalVerificationLog
                {
                    Id = Guid.NewGuid(),
                    IsSuccess = isSuccess,
                    VerificationStep = step,    
                    VerificationType = type,
                    WithdrawalId = withdrawalId,
                    VerificationRule = completeRuleDesc,
                    RuleRequiredValue = ruleRequiredValues,
                    CurrentValue = criteriaActualValue
                };

                _repository.WithdrawalVerificationLogs.Add(withdrawalVerificationLog);
                _repository.SaveChanges();
                scope.Complete();
            }
        }

        #endregion
    }
}