using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Resources;
using FluentValidation;

namespace AFT.RegoV2.Bonus.Core.Validators
{
    internal class ToggleBonusStatusValidator : AbstractValidator<ToggleBonusStatus>
    {
        public ToggleBonusStatusValidator(BonusQueries queries)
        {
            Func<Guid, Data.Bonus> bonusGetter =
                bonusId => queries.GetCurrentVersionBonuses().SingleOrDefault(b => b.Id == bonusId);

            RuleFor(model => model.Id)
                .Must(id => bonusGetter(id) != null)
                .WithMessage(ValidatorMessages.BonusDoesNotExist);

            RuleFor(model => model.IsActive)
                .Must((model, destStatus) =>
                {
                    var bonusToValidate = bonusGetter(model.Id);
                    var templateType = bonusToValidate.Template.Info.TemplateType;
                    if (templateType == BonusType.ReferFriend || templateType == BonusType.MobilePlusEmailVerification)
                    {
                        var isAnyActiveBonusesOfTrigger = queries
                            .GetCurrentVersionBonuses()
                            .Where(bonus =>
                                bonus.Id != bonusToValidate.Id &&
                                bonus.Template.Info.Brand.Id == bonusToValidate.Template.Info.Brand.Id &&
                                bonus.IsActive)
                            .Any(bonus => bonus.Template.Info.TemplateType == templateType);
                        return isAnyActiveBonusesOfTrigger == false;
                    }

                    return true;
                })
                .WithMessage(ValidatorMessages.OneBonusOfATypeCanBeActive)
                .When(model =>
                {
                    var bonus = bonusGetter(model.Id);
                    if (bonus == null)
                        return false;

                    return bonus.IsActive == false && model.IsActive;
                });
        }
    }
}