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
    public sealed class AddMessageTemplateValidator : MessageTemplateValidator<AddMessageTemplate>
    {
        public AddMessageTemplateValidator(
            IMessagingRepository repository,
            IMessageTemplateQueries messageTemplateQueries) 
            : base(repository, messageTemplateQueries)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            ValidateBrand().DependentRules(x =>
            {
                x.CascadeMode = CascadeMode.StopOnFirstFailure;
                ValidateLanguage(x);
                ValidateMessageType(x);
                ValidateMessageDeliveryMethod(x);
                ValidateTemplateName(x);
                ValidateSubject(x);
                ValidateMessageContent(x);
            });
        }

        private IRuleBuilderOptions<AddMessageTemplate, Guid> ValidateBrand()
        {
            return RuleFor(x => x.BrandId)
                .Must(x =>
                {
                    Brand = Repository.Brands
                        .Include(y => y.Languages)
                        .SingleOrDefault(y => y.Id == x);

                    return Brand != null;
                })
                .WithMessage(MessagingValidationError.InvalidBrand);
        }

        private void ValidateLanguage(AbstractValidator<AddMessageTemplate> validator)
        {
            validator.RuleFor(x => x.LanguageCode)
                .NotEmpty()
                .WithMessage(MessagingValidationError.Required)
                .Must(x => Brand.Languages.Any(y => y.Code == x))
                .WithMessage(MessagingValidationError.InvalidLanguage);
        }

        private static void ValidateMessageType(AbstractValidator<AddMessageTemplate> validator)
        {
            validator.RuleFor(x => x.MessageType)
                .Must(x => Enum.IsDefined(typeof(MessageType), x))
                .WithMessage(MessagingValidationError.InvalidMessageType);
        }

        private static void ValidateMessageDeliveryMethod(AbstractValidator<AddMessageTemplate> validator)
        {
            validator.RuleFor(x => x.MessageDeliveryMethod)
                .Must(x => Enum.IsDefined(typeof(MessageDeliveryMethod), x))
                .WithMessage(MessagingValidationError.InvalidMessageDeliveryMethod);
        }

        protected override void ValidateTemplateName(AbstractValidator<AddMessageTemplate> validator)
        {
            validator.RuleFor(x => x.TemplateName)
                .NotEmpty()
                .WithMessage(MessagingValidationError.Required)
                .Must((data, name) => !Repository.MessageTemplates
                    .Any(y =>
                        y.BrandId == Brand.Id &&
                        y.MessageType == data.MessageType &&
                        y.MessageDeliveryMethod == data.MessageDeliveryMethod &&
                        y.LanguageCode == data.LanguageCode &&
                        y.TemplateName == name))
                .WithMessage(MessagingValidationError.TemplateNameInUse);
        }

        protected override void ValidateSubject(AbstractValidator<AddMessageTemplate> validator)
        {
            validator.When(x => x.MessageDeliveryMethod == MessageDeliveryMethod.Email,
                () => validator.RuleFor(x => x.Subject)
                    .NotEmpty()
                    .WithMessage(MessagingValidationError.Required)
                    .Must(CanParseTemplate)
                    .WithMessage(MessagingValidationError.InvalidSubject));

            validator.When(x => x.MessageDeliveryMethod == MessageDeliveryMethod.Sms,
                () => validator.RuleFor(x => x.Subject)
                    .Must(string.IsNullOrWhiteSpace)
                    .WithMessage(MessagingValidationError.SubjectNotApplicable));
        }
    }
}