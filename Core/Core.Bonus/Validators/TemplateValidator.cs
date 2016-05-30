using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Resources;
using FluentValidation;
using Template = AFT.RegoV2.Bonus.Core.Data.Template;
using TemplateInfo = AFT.RegoV2.Bonus.Core.Data.TemplateInfo;

namespace AFT.RegoV2.Bonus.Core.Validators
{
    internal class TemplateValidator : AbstractValidator<CreateUpdateTemplate>
    {
        private readonly Func<CreateUpdateTemplate, Data.Brand> _brandGetter;
        private readonly Func<CreateUpdateTemplate, TemplateInfo> _infoGetter;

        public TemplateValidator(IBonusRepository repository, BonusQueries queries)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            _infoGetter = template => repository.Templates.Single(t => t.Id == template.Id && t.Version == template.Version).Info;
            _brandGetter = template => template.Info == null ? _infoGetter(template).Brand : repository.Brands.SingleOrDefault(b => b.Id == template.Info.BrandId);

            When(template => template.Id != Guid.Empty, () =>
            {
                Func<CreateUpdateTemplate, Template> templateGetter =
                    vm => queries.GetCurrentVersionTemplates().SingleOrDefault(t => t.Id == vm.Id);
                RuleFor(template => template)
                    .Must(template => templateGetter(template) != null)
                    .WithMessage(ValidatorMessages.TemplateDoesNotExist)
                    .Must(template => templateGetter(template).Version == template.Version)
                    .WithMessage(ValidatorMessages.TemplateVersionIsNotCurrent)
                    .Must(template => queries.GetBonusesUsingTemplate(templateGetter(template)).Any(bonus => bonus.IsActive) == false)
                    .WithMessage(ValidatorMessages.AllBonusesShouldBeInactive)
                    .When(template => templateGetter(template).Status == TemplateStatus.Complete, ApplyConditionTo.CurrentValidator)
                    .WithName("Template")
                    .DependentRules(rules =>
                    {
                        When(template => template.Availability != null, () => ValidateAvailability(repository));
                        When(template => template.Rules != null, ValidateRules);
                        When(template => template.Wagering != null, () => ValidateWagering(repository));
                    });
            });

            When(template => template.Info != null, () => ValidateInfo(repository));
        }

        private void ValidateInfo(IBonusRepository repository)
        {
            RuleFor(template => template.Info.Name)
                .NotEmpty()
                .WithMessage(ValidatorMessages.NameIsNotSpecified)
                .Matches(@"^[a-zA-Z0-9_\-\s]*$")
                .WithMessage(ValidatorMessages.TemplateNameIsInvalid)
                .Length(1, 50)
                .WithMessage(ValidatorMessages.NameLengthIsInvalid, 1, 50)
                .Must((template, name) =>
                {
                    var templates = repository.Templates.Where(t => t.Info.Name == name && t.Status != TemplateStatus.Deleted);
                    if (template.Info.BrandId.HasValue)
                    {
                        var brand = repository.Brands.SingleOrDefault(p => p.Id == template.Info.BrandId.Value);
                        if (brand != null)
                        {
                            templates = templates.Where(t => t.Info.Brand.Id == brand.Id);
                        }
                    }
                    if (template.Id != Guid.Empty)
                    {
                        templates = templates.Where(t => t.Id != template.Id);
                    }

                    return templates.Any() == false;
                })
                .WithMessage(ValidatorMessages.NameIsNotUnique);

            RuleFor(template => template.Info.Description)
                .Length(1, 200)
                .WithMessage(ValidatorMessages.DescriptionLengthIsInvalid, 1, 200)
                .When(template => string.IsNullOrWhiteSpace(template.Info.Description) == false);

            RuleFor(template => template.Info.BrandId)
                .Must((template, guid) => _brandGetter(template) != null)
                .WithMessage(ValidatorMessages.TemplateBrandDoesNotExist);

            RuleFor(template => template.Info.WalletTemplateId)
                .NotEmpty()
                .WithMessage(ValidatorMessages.TemplateReceivingWalletIsNotSpecified)
                .Must((template, walletId) => _brandGetter(template).WalletTemplates.Select(c => c.Id).Contains(walletId))
                .WithMessage(ValidatorMessages.TemplateReceivingWalletIsInvalid)
                .When(template => _brandGetter(template) != null, ApplyConditionTo.CurrentValidator);

            RuleFor(template => template.Info.Mode)
                .Must((template, mode) =>
                {
                    if (new[]
                    {
                        BonusType.FirstDeposit, BonusType.ReloadDeposit, BonusType.FundIn
                    }.Contains(template.Info.TemplateType))
                    {
                        return true;
                    }

                    return mode != IssuanceMode.AutomaticWithCode;
                })
                .WithMessage(ValidatorMessages.TemplateModeIsIncorrect);
        }

        private void ValidateAvailability(IBonusRepository repository)
        {
            RuleFor(template => template.Availability.RedemptionsLimit)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidatorMessages.TemplatePlayerRedemptionLimitIsNegative);

            RuleFor(template => template.Availability.ParentBonusId)
                .Must(id => repository.Bonuses.Any(a => a.Id == id))
                .WithMessage(ValidatorMessages.TemplateParentBonusDoesNotExist)
                .When(bonus => bonus.Availability.ParentBonusId.HasValue);

            RuleFor(template => template.Availability.PlayerRegistrationDateTo)
                .Must((template, registrationDateTo) => registrationDateTo >= template.Availability.PlayerRegistrationDateFrom)
                .WithMessage(ValidatorMessages.TemplatePlayerRegistrationDateRangeIsInvalid)
                .When(template => template.Availability.PlayerRegistrationDateFrom.HasValue &&
                    template.Availability.PlayerRegistrationDateTo.HasValue);

            RuleFor(template => template.Availability.WithinRegistrationDays)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidatorMessages.TemplateWithinRegistrationDaysIsNegative);

            RuleFor(template => template.Availability.ExcludeBonuses)
                .Must((template, excludes) => excludes.Contains(template.Availability.ParentBonusId.Value) == false)
                .WithMessage(ValidatorMessages.TemplateBonusExcludesContainsParentBonus)
                .When(template => template.Availability.ParentBonusId.HasValue);

            RuleFor(template => template.Availability.PlayerRedemptionsLimit)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidatorMessages.TemplatePlayerRedemptionsIsNegative);

            RuleFor(template => template.Availability.VipLevels)
                .Must((template, vips) =>
                {
                    var brandVips = _brandGetter(template).Vips.Select(v => v.Code);
                    return vips.All(brandVips.Contains);
                })
                .WithMessage(ValidatorMessages.TemplateVipsAreInvalid);

            RuleFor(template => template.Availability.ExcludeRiskLevels)
                .Must((template, riskLevels) =>
                {
                    var rls = _brandGetter(template).RiskLevels.Select(x => x.Id);
                    return riskLevels.All(rls.Contains);
                })
                .WithMessage(ValidatorMessages.TemplateRiskLevelsAreInvalid);
        }

        private void ValidateRules()
        {
            RuleFor(template => template.Rules.RewardTiers)
                .NotEmpty()
                .WithMessage(ValidatorMessages.TemplateCurrenciesAreEmpty);

            RuleFor(template => template.Rules.RewardType)
                .Must((template, type) =>
                {
                    var templateInfo = _infoGetter(template);
                    switch (templateInfo.TemplateType)
                    {
                        case BonusType.FirstDeposit:
                            return true;
                        case BonusType.ReloadDeposit:
                            return true;
                        case BonusType.HighDeposit:
                            return type == BonusRewardType.TieredAmount;
                        case BonusType.FundIn:
                            return true;
                        case BonusType.MobilePlusEmailVerification:
                            return type == BonusRewardType.Amount;
                        case BonusType.ReferFriend:
                            return type == BonusRewardType.TieredAmount;
                        default:
                            return false;
                    }
                })
                .WithMessage(ValidatorMessages.TemplateRewardTypeIsNotSupported);

            When(template => _infoGetter(template).TemplateType == BonusType.FundIn, () =>
            {
                RuleFor(template => template.Rules.FundInWallets)
                    .NotEmpty()
                    .WithMessage(ValidatorMessages.TemplateNoFundInWallets)
                    .Must((template, wallets) =>
                    {
                        var brandWallets = _brandGetter(template).WalletTemplates.Select(c => c.Id);
                        return wallets.All(brandWallets.Contains);
                    })
                    .WithMessage(ValidatorMessages.TemplateFundInWalletsAreInvalid);
            });

            When(template => template.Rules.RewardTiers.Any(), () =>
            {
                RuleFor(template => template.Rules.RewardTiers)
                    .Must(rewardTiers => rewardTiers.All(t => t.BonusTiers.Any()))
                    .WithMessage(ValidatorMessages.TemplateBonusTierAtLeastOneIsRequired)
                    .Must(rewardTiers => rewardTiers.All(t => t.RewardAmountLimit >= 0))
                    .WithMessage(ValidatorMessages.TemplateRewardLimitIsNegative)
                    .Must((template, rewardTiers) =>
                    {
                        var brandCurrencies = _brandGetter(template).Currencies.Select(c => c.Code);
                        return rewardTiers.Select(rw => rw.CurrencyCode).All(brandCurrencies.Contains);
                    })
                    .WithMessage(ValidatorMessages.TemplateRewardCurrenciesAreInvalid);

                When(template => template.Rules.RewardTiers.All(t => t.BonusTiers.Any()), () =>
                {
                    RuleFor(template => template.Rules.RewardTiers)
                        .Must(rewardTiers =>
                        {
                            var isValid = true;
                            foreach (var rewardTier in rewardTiers)
                            {
                                var tiers = rewardTier.BonusTiers;
                                var currencyTiersAreInvalid = tiers.Any(t1 => tiers.Any(t2 => t1 != t2 && t1.From == t2.From));
                                if (currencyTiersAreInvalid) isValid = false;
                            }
                            return isValid;
                        })
                        .WithMessage(ValidatorMessages.TemplateDuplicateHighDepositTiers)
                        .When(template => _infoGetter(template).TemplateType == BonusType.HighDeposit);

                    When(template => _infoGetter(template).TemplateType != BonusType.HighDeposit, () =>
                    {
                        RuleFor(template => template.Rules.RewardTiers)
                            .Must(rewardTiers => rewardTiers.All(t => t.BonusTiers.Single().Reward > 0))
                            .WithMessage(ValidatorMessages.TemplateRewardValueIsInvalid)
                            .Must(rewardTiers => rewardTiers.All(t => t.BonusTiers.Single().MaxAmount >= 0))
                            .WithMessage(ValidatorMessages.TemplateTransactionAmountLimitIsNegative)
                            .When(template => template.Rules.RewardType == BonusRewardType.Amount || template.Rules.RewardType == BonusRewardType.Percentage);

                        RuleFor(template => template.Rules.RewardTiers)
                            .Must(rewardTiers => rewardTiers.All(t => t.BonusTiers.All(r => r.From > 0 && r.Reward > 0 && r.MaxAmount >= 0)))
                            .WithMessage(ValidatorMessages.TemplateBonusTierIsInvalid)
                            .Must(rewardTiers =>
                            {
                                var isValid = true;
                                foreach (var rewardTier in rewardTiers)
                                {
                                    var tiers = rewardTier.BonusTiers;
                                    for (var i = 0; i < tiers.Count() - 1; i++)
                                    {
                                        var thisTier = tiers[i];
                                        var nextTier = tiers[i + 1];
                                        if (thisTier.From >= nextTier.From)
                                        {
                                            isValid = false;
                                            break;
                                        }
                                    }
                                }
                                return isValid;
                            })
                            .WithMessage(ValidatorMessages.TemplateBonusTiersOverlap)
                            .When(template => template.Rules.RewardType == BonusRewardType.TieredAmount || template.Rules.RewardType == BonusRewardType.TieredPercentage);
                    });
                });
            });
        }

        private void ValidateWagering(IBonusRepository repository)
        {
            RuleFor(template => template.Wagering.Multiplier)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidatorMessages.TemplateWageringConditionIsNegative)
                .GreaterThan(0)
                .WithMessage(ValidatorMessages.TemplateWageringConditionIsZeroOrLess)
                .When(template => template.Wagering.HasWagering, ApplyConditionTo.CurrentValidator);

            RuleFor(template => template.Wagering.Threshold)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidatorMessages.TemplateWageringThresholdIsNegative);

            RuleFor(template => template.Wagering.GameContributions)
                .Must(contributions => contributions.All(c => c.Contribution >= 0))
                .WithMessage(ValidatorMessages.TemplateOneOfGameContributionsIsNegative);

            RuleFor(template => template.Wagering.GameContributions)
                .Must(contributions =>
                {
                    var gameIds = contributions.Select(c => c.GameId);
                    var dbGameIds = repository.Games.Select(g => g.Id);
                    return gameIds.All(dbGameIds.Contains);
                })
                .WithMessage(ValidatorMessages.TemplateOneOfGameContributionsPointsToInvalidGame)
                .When(template => template.Wagering.GameContributions.Any());

            RuleFor(template => template.Wagering.Method)
                .Must(method => method == WageringMethod.Bonus)
                .WithMessage(ValidatorMessages.TemplateWageringMethodIsNotSupportedByBonusTrigger)
                .When(template =>
                {
                    var info = _infoGetter(template);
                    return info.TemplateType == BonusType.ReferFriend ||
                           info.TemplateType == BonusType.MobilePlusEmailVerification ||
                           info.TemplateType == BonusType.HighDeposit;
                });

            RuleFor(template => template.Wagering.IsAfterWager)
                .Must(isAfterWager => isAfterWager == false)
                .WithMessage(ValidatorMessages.TemplateWageringIsAfterWagerIsNotApplicable)
                .When(template => template.Wagering.HasWagering == false);
        }
    }
}