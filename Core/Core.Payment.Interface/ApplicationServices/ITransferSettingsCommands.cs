using System;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface ITransferSettingsCommands
    {        
        Guid AddSettings(SaveTransferSettingsCommand model);

        void UpdateSettings(SaveTransferSettingsCommand model);

        void Enable(TransferSettingsId id, string timezoneId, string remarks);

        void Disable(TransferSettingsId id, string timezoneId, string remarks);
    }
}
