using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Resources;
using AFT.RegoV2.Core.Common.Utils;
using FluentValidation;

namespace AFT.RegoV2.Bonus.Core.Validators
{
    internal class BonusValidator : AbstractValidator<CreateUpdateBonus>
    {
        public BonusValidator(IBonusRepository repository, BonusQueries bonusQueries)
        {
            Func<Guid, Template> templateGetter = templateId => bonusQueries.GetCurrentVersionTemplates().SingleOrDefault(a => a.Id == templateId);

            When(bonus => bonus.Id != Guid.Empty, () =>
            {
                Func<CreateUpdateBonus, IQueryable<Data.Bonus>> bonuses = bonusVm => repository.Bonuses.Where(b => b.Id == bonusVm.Id);
                RuleFor(bonus => bonus)
                    .Must(bonus => bonuses(bonus).Any())
                    .WithMessage(ValidatorMessages.BonusDoesNotExist)
                    .WithName("Bonus");

                RuleFor(bonus => bonus)
                    .Must(bonus => bonus.Version == bonuses(bonus).Max(b => b.Version))
                    .WithMessage(ValidatorMessages.BonusVersionIsNotCurrent)
                    .WithName("Bonus")
                    .When(bonus => bonuses(bonus).Any());
            });

            ValidateName(repository);

            RuleFor(bonus => bonus.Description)
                .NotNull()
                .WithMessage(ValidatorMessages.DescriptionIsRequired)
                .Length(1, 200)
                .WithMessage(ValidatorMessages.DescriptionLengthIsInvalid, 1, 200);

            RuleFor(bonus => bonus.DaysToClaim)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidatorMessages.BonusDaysToClaimIsNegative);

            RuleFor(bonus => bonus.TemplateId)
                .Must(templateId => templateGetter(templateId) != null)
                .WithMessage(ValidatorMessages.BonusTemplateIsNotAssigned);

            When(b => templateGetter(b.TemplateId) != null, () =>
            {
                RuleFor(bonus => bonus.Code)
                    .NotEmpty()
                    .When(bonus => templateGetter(bonus.TemplateId).Info.Mode == IssuanceMode.AutomaticWithCode)
                    .WithMessage(ValidatorMessages.BonusCodeIsNotSpecified);
                When(bonus => string.IsNullOrWhiteSpace(bonus.Code) == false, () => RuleFor(bonus => bonus.Code)
                    .Matches(@"^[a-zA-Z0-9]*$")
                    .WithMessage(ValidatorMessages.BonusCodeIsInvalid)
                    .Length(1, 20)
                    .WithMessage(ValidatorMessages.BonusCodeLengthIsInvalid, 1, 20)
                    .Must((bonus, code) =>
                    {
                        if (bonus.Id == Guid.Empty)
                        {
                            return bonusQueries.GetCurrentVersionBonuses().Any(b => b.Code == code) == false;
                        }

                        return bonusQueries.GetCurrentVersionBonuses().Any(b => b.Code == code && b.Id != bonus.Id) == false;
                    })
                    .WithMessage(ValidatorMessages.BonusCodeIsNotUnique));

                RuleFor(bonus => bonus.ActiveTo)
                    .Must((bonus, activeTo) => activeTo > bonus.ActiveFrom)
                    .WithMessage(ValidatorMessages.BonusActivityRangeIsInvalid)
                    .Must((bonus, activeTo) =>
                    {
                        var template = templateGetter(bonus.TemplateId);
                        var timezoneId = template.Info.Brand.TimezoneId;
                        return activeTo.ToBrandDateTimeOffset(timezoneId) >
                               DateTimeOffset.Now.ToBrandOffset(timezoneId);
                    })
                    .WithMessage(ValidatorMessages.BonusActiveToIsInvalid);

                ValidateDuration();
            });
        }

        private void ValidateName(IBonusRepository repository)
        {
            RuleFor(bonus => bonus.Name)
                .NotEmpty()
                .WithMessage(ValidatorMessages.NameIsNotSpecified);
            When(bonus => string.IsNullOrWhiteSpace(bonus.Name) == false, () => RuleFor(bonus => bonus.Name)
                .Matches(@"^[a-zA-Z0-9_\-\s]*$")
                .WithMessage(ValidatorMessages.BonusNameIsInvalid)
                .Length(1, 50)
                .WithMessage(ValidatorMessages.NameLengthIsInvalid, 1, 50)
                .Must((bonus, name) =>
                {
                    if (bonus.Id == Guid.Empty)
                    {
                        return repository.Bonuses.Any(b => b.Name == name) == false;
                    }

                    return repository.Bonuses.Any(b => b.Name == name && b.Id != bonus.Id) == false;
                })
                .WithMessage(ValidatorMessages.NameIsNotUnique));
        }

        private void ValidateDuration()
        {
            When(bonus => bonus.DurationType != DurationType.None, () =>
            {
                Func<CreateUpdateBonus, DateTime> durationEndGetter = vm => 
                    vm.DurationType == DurationType.StartDateBased ? 
                        vm.ActiveFrom.Add(new TimeSpan(vm.DurationDays, vm.DurationHours, vm.DurationMinutes, 0)) : 
                        vm.DurationEnd;
                Func<CreateUpdateBonus, DateTime> durationStartGetter = vm => vm.DurationType == DurationType.StartDateBased ? vm.ActiveFrom : vm.DurationStart;
                Func<CreateUpdateBonus, bool> isNonZeroLengthDuration = bonus => durationEndGetter(bonus) - durationStartGetter(bonus) > TimeSpan.Zero;

                RuleFor(bonus => bonus)
                    .Must(isNonZeroLengthDuration)
                    .WithMessage(ValidatorMessages.BonusDurationIsZeroLength)
                    .OverridePropertyName("DurationType");

                When(isNonZeroLengthDuration, () =>
                {
                    RuleFor(bonus => bonus)
                        .Must(bonus =>
                        {
                            var durationEnd = durationEndGetter(bonus);
                            return durationEnd > durationStartGetter(bonus) && durationEnd <= bonus.ActiveTo;
                        })
                        .WithMessage(ValidatorMessages.BonusDurationDaterangeIsIncorrect)
                        .When(bonus => bonus.DurationType == DurationType.StartDateBased)
                        .OverridePropertyName("DurationType");

                    RuleFor(bonus => bonus.DurationEnd)
                        .Must((bonus, durationEnd) =>
                            durationEnd > bonus.DurationStart &&
                            durationEnd <= bonus.ActiveTo &&
                            bonus.DurationStart >= bonus.ActiveFrom)
                        .WithMessage(ValidatorMessages.BonusDurationDaterangeIsIncorrect)
                        .When(bonus => bonus.DurationType == DurationType.Custom)
                        .OverridePropertyName("DurationType");
                });
            });
        }
    }
}