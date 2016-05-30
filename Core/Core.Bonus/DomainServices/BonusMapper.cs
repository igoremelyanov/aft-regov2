using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Core.Common.Utils;
using AutoMapper;
using GameContribution = AFT.RegoV2.Bonus.Core.Data.GameContribution;
using RewardTier = AFT.RegoV2.Bonus.Core.Data.RewardTier;
using Template = AFT.RegoV2.Bonus.Core.Data.Template;
using TemplateAvailability = AFT.RegoV2.Bonus.Core.Data.TemplateAvailability;
using TemplateInfo = AFT.RegoV2.Bonus.Core.Data.TemplateInfo;
using TemplateNotification = AFT.RegoV2.Bonus.Core.Data.TemplateNotification;
using TemplateRules = AFT.RegoV2.Bonus.Core.Data.TemplateRules;
using TemplateWagering = AFT.RegoV2.Bonus.Core.Data.TemplateWagering;

namespace AFT.RegoV2.Bonus.Core.DomainServices
{
    public class BonusMapper
    {
        private readonly BonusQueries _bonusQueries;
        private readonly IBonusRepository _bonusRepository;

        static BonusMapper()
        {
            Mapper.CreateMap<CreateUpdateBonus, Data.Bonus>()
                .ForMember(dest => dest.ActiveFrom, opt => opt.Ignore())
                .ForMember(dest => dest.ActiveTo, opt => opt.Ignore())
                .ForMember(dest => dest.DurationStart, opt => opt.Ignore())
                .ForMember(dest => dest.DurationEnd, opt => opt.Ignore());

            Mapper.CreateMap<CreateUpdateTemplateInfo, TemplateInfo>();
            CreateTemplateAvailabilityMap();
            CreateTemplateRulesMap();
            CreateTemplateWageringMap();
            CreateTemplateNotificationMap();
            CreateTemplateMap();
        }

        public BonusMapper(BonusQueries bonusQueries, IBonusRepository bonusRepository)
        {
            _bonusQueries = bonusQueries;
            _bonusRepository = bonusRepository;
        }

        public Data.Bonus MapModelToBonus(CreateUpdateBonus model)
        {
            var bonus = Mapper.Map<Data.Bonus>(model);

            var template = _bonusQueries.GetCurrentVersionTemplates().Single(a => a.Id == model.TemplateId);
            bonus.Template = template;

            var timezoneId = template.Info.Brand.TimezoneId;

            bonus.ActiveFrom = bonus.DurationStart = model.ActiveFrom.ToBrandDateTimeOffset(timezoneId);
            bonus.ActiveTo = bonus.DurationEnd = model.ActiveTo.ToBrandDateTimeOffset(timezoneId);

            if (model.DurationType == DurationType.Custom)
            {
                bonus.DurationStart = model.DurationStart.ToBrandDateTimeOffset(timezoneId);
                bonus.DurationEnd = model.DurationEnd.ToBrandDateTimeOffset(timezoneId);
            }
            else if (model.DurationType == DurationType.StartDateBased)
            {
                var durationLength = new TimeSpan(model.DurationDays, model.DurationHours, model.DurationMinutes, 0);
                bonus.DurationEnd = bonus.ActiveFrom.Add(durationLength);
            }

            return bonus;
        }

        public Template MapModelToTemplate(CreateUpdateTemplate model)
        {
            Template template;
            if (model.Id == Guid.Empty)
            {
                template = Mapper.Map<Template>(model);
                template.Info.Brand = _bonusRepository.Brands.Single(p => p.Id == model.Info.BrandId.Value);
            }
            else
            {
                template = _bonusQueries.GetCurrentVersionTemplates().Single(t => t.Id == model.Id && t.Version == model.Version);
                template = Mapper.Map(model, template);
            }

            return template;
        }

        private static void CreateTemplateAvailabilityMap()
        {
            Mapper.CreateMap<CreateUpdateTemplateAvailability, TemplateAvailability>()
                .ForMember(dest => dest.VipLevels,
                    opt => opt.ResolveUsing(data => data.VipLevels.Select(code => new BonusVip { Code = code })))
                .ForMember(dest => dest.ExcludeBonuses,
                    opt =>
                        opt.MapFrom(
                            data =>
                                data.ExcludeBonuses.Select(
                                    excludedBonusId => new BonusExclude { ExcludedBonusId = excludedBonusId })))
                .ForMember(dest => dest.ExcludeRiskLevels,
                    opt =>
                        opt.MapFrom(
                            data =>
                                data.ExcludeRiskLevels.Select(
                                    excludedRiskLevelId => new RiskLevelExclude { ExcludedRiskLevelId = excludedRiskLevelId })))
                .ForMember(dest => dest.PlayerRegistrationDateFrom, opt => opt.Ignore())
                .ForMember(dest => dest.PlayerRegistrationDateTo, opt => opt.Ignore());
        }

        private static void CreateTemplateRulesMap()
        {
            Mapper.CreateMap<CreateUpdateTemplateRules, TemplateRules>()
                .ForMember(dest => dest.FundInWallets,
                    opt =>
                        opt.MapFrom(data => data.FundInWallets.Select(walletId => new BonusFundInWallet { WalletId = walletId })));

            Mapper.CreateMap<CreateUpdateRewardTier, RewardTier>()
                .ForMember(dest => dest.BonusTiers, opt => opt.MapFrom(data => new List<TierBase>()));
        }

        private static void CreateTemplateWageringMap()
        {
            Mapper.CreateMap<CreateUpdateTemplateWagering, TemplateWagering>()
                .ForMember(dest => dest.GameContributions,
                    opt =>
                        opt.MapFrom(
                            data =>
                                data.GameContributions.Select(
                                    c => new GameContribution { GameId = c.GameId, Contribution = (c.Contribution / 100) })));
        }

        private static void CreateTemplateNotificationMap()
        {
            Mapper.CreateMap<CreateUpdateTemplateNotification, TemplateNotification>()
                .ForMember(dest => dest.Triggers,
                    opt =>
                        opt.MapFrom(
                            data =>
                                data.EmailTriggers.Select(x => new NotificationMessageType {MessageType = x, TriggerType = TriggerType.Email})
                                    .Union(
                                data.SmsTriggers.Select(x => new NotificationMessageType {MessageType = x, TriggerType = TriggerType.Sms}))));
        }

        private static void CreateTemplateMap()
        {
            Mapper.CreateMap<CreateUpdateTemplate, Template>()
                .AfterMap((templateVm, template) =>
                {
                    //Empty id means saving Info. This will discard any data except Info. 
                    if (templateVm.Id == Guid.Empty)
                    {
                        template.Availability = null;
                        template.Rules = null;
                        template.Wagering = null;
                        template.Notification = null;
                    }
                    else
                    {
                        if (templateVm.Rules != null)
                        {
                            foreach (var vmRewardTier in templateVm.Rules.RewardTiers)
                            {
                                var templateRewardTier =
                                    template.Rules.RewardTiers.Single(t => t.CurrencyCode == vmRewardTier.CurrencyCode);
                                if (template.Info.TemplateType == BonusType.HighDeposit)
                                {
                                    vmRewardTier.BonusTiers.ForEach(
                                        tier => templateRewardTier.BonusTiers.Add(new HighDepositTier
                                        {
                                            From = tier.From,
                                            Reward = tier.Reward,
                                            NotificationPercentThreshold = tier.NotificationPercentThreshold / 100
                                        }));
                                }
                                else
                                {
                                    vmRewardTier.BonusTiers.ForEach(tier => templateRewardTier.BonusTiers.Add(new BonusTier
                                    {
                                        From = tier.From,
                                        Reward = tier.Reward,
                                        MaxAmount = tier.MaxAmount
                                    }));
                                    if (templateVm.Rules.RewardType == BonusRewardType.Percentage ||
                                        templateVm.Rules.RewardType == BonusRewardType.TieredPercentage)
                                    {
                                        templateRewardTier.BonusTiers.ForEach(t => t.Reward = t.Reward / 100);
                                    }
                                }
                            }
                        }

                        if (templateVm.Availability != null)
                        {
                            var timezoneId = template.Info.Brand.TimezoneId;

                            template.Availability.PlayerRegistrationDateFrom =
                                templateVm.Availability.PlayerRegistrationDateFrom.HasValue == false
                                    ? new DateTimeOffset?()
                                    : templateVm.Availability.PlayerRegistrationDateFrom.Value.ToBrandDateTimeOffset(
                                        timezoneId);
                            template.Availability.PlayerRegistrationDateTo =
                                templateVm.Availability.PlayerRegistrationDateTo.HasValue == false
                                    ? new DateTimeOffset?()
                                    : templateVm.Availability.PlayerRegistrationDateTo.Value.ToBrandDateTimeOffset(
                                        timezoneId);
                        }
                        if (template.Info != null)
                        {
                            template.Info.Id = Guid.NewGuid();
                        }
                        if (template.Availability != null)
                        {
                            template.Availability.Id = Guid.NewGuid();
                            template.Availability.VipLevels.ForEach(v => v.Id = Guid.NewGuid());
                            template.Availability.ExcludeBonuses.ForEach(ex => ex.Id = Guid.NewGuid());
                            template.Availability.ExcludeRiskLevels.ForEach(ex => ex.Id = Guid.NewGuid());
                        }
                        if (template.Rules != null)
                        {
                            template.Rules.Id = Guid.NewGuid();
                            template.Rules.RewardTiers.ForEach(rt =>
                            {
                                rt.Id = Guid.NewGuid();
                                rt.BonusTiers.ForEach(bt => bt.Id = Guid.NewGuid());
                            });
                            template.Rules.FundInWallets.ForEach(fw => fw.Id = Guid.NewGuid());
                        }
                        if (template.Wagering != null)
                        {
                            template.Wagering.Id = Guid.NewGuid();
                            template.Wagering.GameContributions.ForEach(gc => gc.Id = Guid.NewGuid());
                        }
                        if (template.Notification != null)
                        {
                            template.Notification.Id = Guid.NewGuid();
                            template.Notification.Triggers.ForEach(t => t.Id = Guid.NewGuid());
                        }
                    }
                })
                .ForAllMembers(opt => opt.Condition(srs => !srs.IsSourceValueNull));
        }
    }
}