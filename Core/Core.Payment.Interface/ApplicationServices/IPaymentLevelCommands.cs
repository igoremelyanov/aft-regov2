using System;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using FluentValidation.Results;
namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IPaymentLevelCommands
    {        
        PaymentLevelSaveResult Save(EditPaymentLevel model);
     
        PaymentLevelSaveResult Edit(EditPaymentLevel model);

        ValidationResult ValidatePaymentLevelCanBeActivated(ActivatePaymentLevelCommand command);

        void Activate(ActivatePaymentLevelCommand command);

        ValidationResult ValidatePaymentLevelCanBeDeactivated(DeactivatePaymentLevelCommand command);

        void Deactivate(DeactivatePaymentLevelCommand command);
    }
}
