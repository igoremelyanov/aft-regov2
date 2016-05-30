using System;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.ApplicationServices
{
    public interface IAVCConfigurationCommands
    {
        AutoVerificationCheckConfiguration Create(AVCConfigurationDTO data);
        void Update(AVCConfigurationDTO data);
        void Delete(Guid id);
        void Activate(AvcChangeStatusCommand command);
        void Deactivate(AvcChangeStatusCommand command);
    }
}
