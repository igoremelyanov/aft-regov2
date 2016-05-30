using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Resources;
using FluentValidation;

namespace AFT.RegoV2.Bonus.Core.Validators
{
    internal class TemplateDeletionValidator : AbstractValidator<DeleteTemplate>
    {
        public TemplateDeletionValidator(BonusQueries queries)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(model => model)
                .Must(model => queries.GetCurrentVersionTemplates().Any(t => t.Id == model.TemplateId))
                .WithMessage(ValidatorMessages.TemplateDoesNotExist)
                .Must(model =>
                        queries.GetCurrentVersionBonuses().Any(bonus => bonus.Template.Id == model.TemplateId) == false)
                .WithMessage(ValidatorMessages.TemplateIsInUse)
                .WithName("Template");
        }
    }
}