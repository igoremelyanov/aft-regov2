using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Extensions;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateCommands;
using FluentValidation;

namespace AFT.RegoV2.Core.Messaging.Validators
{
    public sealed class EditMessageTemplateValidator : MessageTemplateValidator<EditMessageTemplate>
    {
        private Data.MessageTemplate _messageTemplate;

        public EditMessageTemplateValidator(
            IMessagingRepository repository,
            IMessageTemplateQueries messageTemplateQueries) 
            : base(repository, messageTemplateQueries)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            ValidatedId().DependentRules(x =>
            {
                x.CascadeMode = CascadeMode.StopOnFirstFailure;
                ValidateTemplateName(x);
                ValidateSubject(x);
                ValidateMessageContent(x);
            });
        }

        private IRuleBuilderOptions<EditMessageTemplate, Guid> ValidatedId()
        {
            return RuleFor(x => x.Id)
                .Must(x =>
                {
                    _messageTemplate = Repository.MessageTemplates
                        .SingleOrDefault(y => y.Id == x);

                    if (_messageTemplate != null)
                        Brand = Repository.Brands
                            .Include(y => y.Languages)
                            .Single(y => y.Id == _messageTemplate.BrandId);

                    return _messageTemplate != null;
                })
                .WithMessage(MessagingValidationError.InvalidId);
        }

        protected override void ValidateTemplateName(AbstractValidator<EditMessageTemplate> validator)
        {
            validator.RuleFor(x => x.TemplateName)
                .NotEmpty()
                .WithMessage(MessagingValidationError.Required)
                .Must((data, name) => !Repository.MessageTemplates
                    .Any(y =>
                        y.Id != data.Id &&
                        y.BrandId == Brand.Id &&
                        y.MessageType == _messageTemplate.MessageType &&
                        y.MessageDeliveryMethod == _messageTemplate.MessageDeliveryMethod &&
                        y.LanguageCode == _messageTemplate.LanguageCode &&
                        y.TemplateName == name))
                .WithMessage(MessagingValidationError.TemplateNameInUse);
        }

        protected override void ValidateSubject(AbstractValidator<EditMessageTemplate> validator)
        {
            validator.When(x => _messageTemplate.MessageDeliveryMethod == MessageDeliveryMethod.Email,
                () => validator.RuleFor(x => x.Subject)
                    .NotEmpty()
                    .WithMessage(MessagingValidationError.Required));

            validator.When(x => _messageTemplate.MessageDeliveryMethod == MessageDeliveryMethod.Sms,
                () => validator.RuleFor(x => x.Subject)
                    .Must(string.IsNullOrWhiteSpace)
                    .WithMessage(MessagingValidationError.SubjectNotApplicable));
        }
    }
}