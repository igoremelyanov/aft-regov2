using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IPaymentGatewaySettingsCommands
    {
        SavePaymentGatewaysSettingsResult Add(SavePaymentGatewaysSettingsData model);

        SavePaymentGatewaysSettingsResult Edit(SavePaymentGatewaysSettingsData model);

        void Activate(ActivatePaymentGatewaySettingsData model);

        void Deactivate(DeactivatePaymentGatewaySettingsData model);

        ValidationResult ValidateThatPaymentGatewaySettingsCanBeAdded(SavePaymentGatewaysSettingsData model);

        ValidationResult ValidateThatPaymentGatewaySettingsCanBeEdited(SavePaymentGatewaysSettingsData model);

        ValidationResult ValidateThatPaymentGatewaySettingsCanBeActivated(ActivatePaymentGatewaySettingsData model);

        ValidationResult ValidateThatPaymentGatewaySettingsCanBeDeactivated(DeactivatePaymentGatewaySettingsData model);
    }
}