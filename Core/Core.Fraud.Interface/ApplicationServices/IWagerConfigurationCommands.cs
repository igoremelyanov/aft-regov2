using System;
using AFT.RegoV2.Core.Fraud.Interface.Data;

namespace AFT.RegoV2.Core.Fraud.Interface.ApplicationServices
{
    public interface IWagerConfigurationCommands
    {
        void ActivateWagerConfiguration(WagerConfigurationId wagerId, Guid userId);
        Guid CreateWagerConfiguration(WagerConfigurationDTO wagerConfigurationDTO, Guid userId);
        void DeactivateWagerConfiguration(WagerConfigurationId wagerId, Guid userId);
        Guid UpdateWagerConfiguration(WagerConfigurationDTO wagerConfigurationDTO, Guid userId);
    }
}