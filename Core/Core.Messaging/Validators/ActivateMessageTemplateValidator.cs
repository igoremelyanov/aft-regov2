using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Extensions;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateCommands;
using FluentValidation;
using MessageTemplate = AFT.RegoV2.Core.Messaging.Data.MessageTemplate;

namespace AFT.RegoV2.Core.Messaging.Validators
{
    public class ActivateMessageTemplateValidator : AbstractValidator<ActivateMessageTemplate>
    {
        public ActivateMessageTemplateValidator(
            IMessagingRepository repository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            MessageTemplate messageTemplate = null;

            RuleFor(x => x.Id)
                .Must(x =>
                {
                    messageTemplate = repository.MessageTemplates.SingleOrDefault(y => y.Id == x);
                    return messageTemplate != null;
                })
                .WithMessage(MessagingValidationError.InvalidId)
                .Must(x => messageTemplate.Status != Status.Active)
                .WithMessage(MessagingValidationError.AlreadyActive);

            RuleFor(x => x.Remarks)
                .NotEmpty()
                .WithMessage(MessagingValidationError.Required);
        }
    }
}