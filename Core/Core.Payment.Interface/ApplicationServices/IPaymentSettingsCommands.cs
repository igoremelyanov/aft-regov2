using System;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IPaymentSettingsCommands
    {
        Guid AddSettings(SavePaymentSettingsCommand model);

        void UpdateSettings(SavePaymentSettingsCommand model);
        
        void Enable(PaymentSettingsId id, string remarks);

        void Disable(PaymentSettingsId id, string remarks);
    }
}
