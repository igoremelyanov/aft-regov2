using System;
using System.Linq;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using FluentValidation;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Payment.Validators
{
    class SavePaymentGatewaySettingsValidator : AbstractValidator<SavePaymentGatewaysSettingsData>
    {
        public SavePaymentGatewaySettingsValidator(IPaymentRepository repository,bool isAdd)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            Custom(data =>
            {
                if(isAdd&& data.Id!=Guid.Empty)
                {
                    return new ValidationFailure("Id", PaymentGatewaySettingsErrors.AlreadyExistsError.ToString());                 
                }
                if(!isAdd && false==repository.PaymentGatewaySettings.Any(x=>x.Id==data.Id))
                {                                        
                    return new ValidationFailure("Id", PaymentGatewaySettingsErrors.NotFound.ToString());
                }
                return null;
            });

            RuleFor(x => x.OnlinePaymentMethodName)
                .NotNull()
                .WithMessage(PaymentGatewaySettingsErrors.RequiredField.ToString())
                .Length(2, 100)
                .WithMessage(PaymentGatewaySettingsErrors.ExceedMaxLength.ToString())
                .Matches(@"^[A-Za-z0-9 ]*$")
                .WithMessage(PaymentGatewaySettingsErrors.AlphanumericSpaces.ToString());

            RuleFor(x => x.EntryPoint)
               .NotNull()
               .WithMessage(PaymentGatewaySettingsErrors.RequiredField.ToString())
               .Length(1, 100)
               .WithMessage(PaymentGatewaySettingsErrors.ExceedMaxLength.ToString())
               .Matches(@"^[A-Za-z0-9-_\.\/\:]*$")
               .WithMessage(PaymentGatewaySettingsErrors.UrlFormatError.ToString());

            When(x => !string.IsNullOrEmpty(x.Remarks), () => RuleFor(x => x.Remarks)
                .Length(1, 200)
                .WithMessage(PaymentGatewaySettingsErrors.ExceedMaxLength.ToString()));

            Custom(data =>
            {
                //check the same payment method and channel             
                var sameSetting =repository.PaymentGatewaySettings.FirstOrDefault(
                    x => x.BrandId == data.Brand
                         && x.PaymentGatewayName == data.PaymentGatewayName
                         && x.Channel == data.Channel);

                if (sameSetting != null)
                {
                    if (isAdd || sameSetting.Id != data.Id)

                    {
                        return new ValidationFailure("PaymentGatewayName", PaymentGatewaySettingsErrors.PaymentGatewaySettingAlreadyExists.ToString());
                    }                    
                }
             
                //check the same setting name
                var theSameNameSetting = repository.PaymentGatewaySettings.FirstOrDefault(
                    x =>x.BrandId == data.Brand
                        &&x.OnlinePaymentMethodName == data.OnlinePaymentMethodName);
                if (theSameNameSetting != null)
                {
                    if (isAdd || theSameNameSetting.Id != data.Id)
                    {
                        return new ValidationFailure("OnlinePaymentMethodName", PaymentGatewaySettingsErrors.OnlinePaymentMethodNameAlreadyExists.ToString());
                    }
                }
                return null;
            });
        }
    }
}
