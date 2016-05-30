using System;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class AutoVerificationConfigurationTestHelper
    {
        private readonly IAVCConfigurationCommands _avcConfigurationCommands;

        public AutoVerificationConfigurationTestHelper(
            IAVCConfigurationCommands avcConfigurationCommands)
        {
            _avcConfigurationCommands = avcConfigurationCommands;
        }

        public AutoVerificationCheckConfiguration CreateConfiguration(AVCConfigurationDTO configuration)
        {
            return _avcConfigurationCommands.Create(configuration);
        }

        public void UpdateConfiguration(AVCConfigurationDTO configuration)
        {
            _avcConfigurationCommands.Update(configuration);
        }

        public void Activate(Guid configurationId, string remarks = null)
        {
            _avcConfigurationCommands.Activate(new AvcChangeStatusCommand
            {
                Id = configurationId,
                Remarks = remarks
            });
        }

        public void Deactivate(Guid configurationId, string remarks = null)
        {
            _avcConfigurationCommands.Deactivate(new AvcChangeStatusCommand
            {
                Id = configurationId,
                Remarks = remarks
            });
        }
    }
}