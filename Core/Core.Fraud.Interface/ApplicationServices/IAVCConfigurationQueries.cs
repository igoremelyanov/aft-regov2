using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Fraud.Interface.ApplicationServices
{
    public interface IAVCConfigurationQueries
    {
        #region Public methods

        AVCConfigurationDTO GetAutoVerificationCheckConfiguration(Guid id);
        IEnumerable<AutoVerificationCheckConfiguration> GetAutoVerificationCheckConfigurations();
        IEnumerable<AutoVerificationCheckConfiguration> GetAutoVerificationCheckConfigurations(Guid brandId);

        #endregion

        ValidationResult GetValidationResult(AvcChangeStatusCommand model, AutoVerificationCheckStatus inactive);
    }
}