using System.Linq;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Fraud.Validations
{
    public class AVCStatusValidator:AbstractValidator<AvcChangeStatusCommand>
    {
        public AVCStatusValidator(IFraudRepository fraudRepository, AutoVerificationCheckStatus status)
        {            
            switch (status)
            {
                //Here we verify that we have such avc in the DB and its status is Inactive
                case AutoVerificationCheckStatus.Inactive:
                    RuleFor(command => command.Id)
                        .Must(commmand => fraudRepository
                            .AutoVerificationCheckConfigurations
                            .Where(avc => avc.Status == AutoVerificationCheckStatus.Inactive)
                            .Select(avc => avc.Id).Contains(commmand))
                        .WithMessage(AVCConfigurationValidationMessagesEnum.AvcMissingInTheDbOrItsStatusIsAlreadyActive.ToString());
                    break;

                //Here we verify that we have such avc in the DB and its status is Active
                case AutoVerificationCheckStatus.Active:
                    RuleFor(command => command.Id)
                        .Must(commmand => fraudRepository
                            .AutoVerificationCheckConfigurations
                            .Where(avc => avc.Status == AutoVerificationCheckStatus.Active)
                            .Select(avc => avc.Id).Contains(commmand))
                        .WithMessage(AVCConfigurationValidationMessagesEnum.AvcMissingInTheDbOrItsStatusIsAlreadyInactive.ToString());
                    break;
            }
            
                
        }
    }
}
