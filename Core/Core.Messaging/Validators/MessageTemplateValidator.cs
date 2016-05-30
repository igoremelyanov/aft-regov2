using System;
using System.Linq;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Common.Extensions;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateCommands;
using DotLiquid;
using FluentValidation;

namespace AFT.RegoV2.Core.Messaging.Validators
{
    public abstract class MessageTemplateValidator<T> : AbstractValidator<T> where T : BaseMessageTemplate
    {
        protected readonly IMessagingRepository Repository;
        protected readonly IMessageTemplateQueries MessageTemplateQueries;
        protected Messaging.Data.Brand Brand;

        protected MessageTemplateValidator(
            IMessagingRepository repository,
            IMessageTemplateQueries messageTemplateQueries)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;
            Repository = repository;
            MessageTemplateQueries = messageTemplateQueries;
        }

        protected abstract void ValidateTemplateName(AbstractValidator<T> validator);

        protected abstract void ValidateSubject(AbstractValidator<T> validator);

        protected void ValidateMessageContent(AbstractValidator<T> validator)
        {
            validator.RuleFor(x => x.MessageContent)
                .NotEmpty()
                    .WithMessage(MessagingValidationError.Required)
                    .Must(CanParseTemplate)
                    .WithMessage(MessagingValidationError.InvalidMessageContent);
        }

        protected bool CanParseTemplate(string content)
        {
            try
            {
                var template = Template.Parse(content);

                return !template.Errors.Any();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}