using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using FluentValidation;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class ActivatePaymentGatewaySettingsValidator : AbstractValidator<ActivatePaymentGatewaySettingsData>
    {
        public ActivatePaymentGatewaySettingsValidator(IPaymentRepository paymentRepository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            Custom(data =>
            {
                var settings = paymentRepository.PaymentGatewaySettings.FirstOrDefault(
                    x => x.Id==data.Id);

                if (settings == null)
                {
                    return new ValidationFailure("Id", PaymentGatewaySettingsErrors.NotFound.ToString());
                  
                }
              
                if (settings.Status==Status.Active)
                {
                    return new ValidationFailure("Status", PaymentGatewaySettingsErrors.AlreadyActive.ToString());
                }
                return null;
            });
        }
    }
}
