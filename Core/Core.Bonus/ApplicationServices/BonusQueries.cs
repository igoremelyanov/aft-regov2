using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Entities;
using AFT.RegoV2.Bonus.Core.Validators;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Shared.Utils;
using AutoMapper;
using FluentValidation.Results;
using BonusRedemption = AFT.RegoV2.Bonus.Core.Models.Data.BonusRedemption;
using GameContribution = AFT.RegoV2.Bonus.Core.Models.Data.GameContribution;
using Player = AFT.RegoV2.Bonus.Core.Entities.Player;
using RewardTier = AFT.RegoV2.Bonus.Core.Data.RewardTier;
using Template = AFT.RegoV2.Bonus.Core.Data.Template;
using TemplateAvailability = AFT.RegoV2.Bonus.Core.Data.TemplateAvailability;
using TemplateInfo = AFT.RegoV2.Bonus.Core.Data.TemplateInfo;
using TemplateNotification = AFT.RegoV2.Bonus.Core.Data.TemplateNotification;
using TemplateRules = AFT.RegoV2.Bonus.Core.Data.TemplateRules;
using TemplateWagering = AFT.RegoV2.Bonus.Core.Data.TemplateWagering;

namespace AFT.RegoV2.Bonus.Core.ApplicationServices
{
    public class BonusQueries
    {
        private readonly IBonusRepository _repository;
        private readonly IBrandOperations _brandOperations;

        static BonusQueries()
        {
            Mapper.CreateMap<Data.BonusRedemption, BonusRedemption>()
                .ForMember(br => br.ClaimableFrom, opt => opt.MapFrom(br => br.Bonus.DurationStart))
                .ForMember(br => br.ClaimableTo, opt => opt.MapFrom(br => br.Bonus.ActiveTo.AddDays(br.Bonus.DaysToClaim)))
                .ForMember(br => br.IsExpired, opt => opt.MapFrom(br => new Entities.BonusRedemption(br).IsExpired))
                .ForMember(br => br.RolloverLeft, opt => opt.MapFrom(br => new Entities.BonusRedemption(br).RolloverLeft));

            Mapper.CreateMap<Data.Bonus, BonusRedemptionBonus>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(data => data.Template.Info.TemplateType));

            Mapper.CreateMap<Data.Bonus, Models.Data.Bonus>()
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(data => data.Template.Info.Brand.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(data => data.Template.Info.TemplateType))
                .ForMember(dest => dest.Mode, opt => opt.MapFrom(data => data.Template.Info.Mode));

            Mapper.CreateMap<TemplateAvailability, Models.Data.TemplateAvailability>()
                .ForMember(dest => dest.VipLevels, opt => opt.MapFrom(data => data.VipLevels.Select(a => a.Code)))
                .ForMember(dest => dest.ExcludeBonuses,
                    opt => opt.MapFrom(data => data.ExcludeBonuses.Select(a => a.ExcludedBonusId)))
                .ForMember(dest => dest.ExcludeRiskLevels,
                    opt => opt.MapFrom(data => data.ExcludeRiskLevels.Select(a => a.ExcludedRiskLevelId)));
            Mapper.CreateMap<TemplateInfo, Models.Data.TemplateInfo>()
                .ForMember(dest => dest.TemplateType, opt => opt.MapFrom(data => data.TemplateType.ToString()));
            Mapper.CreateMap<TemplateRules, Models.Data.TemplateRules>()
                .ForMember(dest => dest.FundInWallets, opt => opt.MapFrom(data => data.FundInWallets.Select(a => a.WalletId)));
            Mapper.CreateMap<TemplateWagering, Models.Data.TemplateWagering>()
                .ForMember(dest => dest.GameContributions, opt =>
                    opt.MapFrom(data => data.GameContributions.Select(c =>
                                    new GameContribution { GameId = c.GameId, Contribution = c.Contribution * 100 })));
            Mapper.CreateMap<RewardTier, Models.Data.RewardTier>()
                .ForMember(dest => dest.BonusTiers, opt => opt.MapFrom(data => new List<TemplateTier>()));
            Mapper.CreateMap<TemplateNotification, Models.Data.TemplateNotification>()
                .ForMember(dest => dest.EmailTriggers, opt => opt.MapFrom(data => data.Triggers.Where(t => t.TriggerType == TriggerType.Email).Select(t => t.MessageType.ToString())))
                .ForMember(dest => dest.SmsTriggers, opt => opt.MapFrom(data => data.Triggers.Where(t => t.TriggerType == TriggerType.Sms).Select(t => t.MessageType.ToString())));
            Mapper.CreateMap<Template, Models.Data.Template>()
                .AfterMap((template, data) =>
                {
                    if (template.Rules != null)
                    {
                        var inRewardTiers = template.Rules.RewardTiers;
                        if (template.Info.TemplateType == BonusType.HighDeposit)
                        {
                            foreach (var rewardTier in inRewardTiers)
                            {
                                var outRewardTier = data.Rules.RewardTiers.Single(t => t.CurrencyCode == rewardTier.CurrencyCode);

                                rewardTier.HighDepositTiers.OrderBy(dt => dt.From).ForEach(tier => outRewardTier.BonusTiers.Add(new TemplateTier
                                {
                                    Reward = tier.Reward,
                                    From = tier.From,
                                    NotificationPercentThreshold = tier.NotificationPercentThreshold * 100
                                }));
                            }
                        }
                        else
                        {
                            foreach (var rewardTier in inRewardTiers)
                            {
                                var outRewardTier = data.Rules.RewardTiers.Single(t => t.CurrencyCode == rewardTier.CurrencyCode);

                                rewardTier.Tiers.OrderBy(t => t.From).ForEach(tier => outRewardTier.BonusTiers.Add(new TemplateTier
                                {
                                    From = tier.From,
                                    Reward = tier.Reward,
                                    MaxAmount = tier.MaxAmount
                                }));
                                if (template.Rules.RewardType == BonusRewardType.Percentage ||
                                    template.Rules.RewardType == BonusRewardType.TieredPercentage)
                                {
                                    outRewardTier.BonusTiers.ForEach(t => t.Reward = t.Reward * 100);
                                }
                            }
                        }
                    }
                });
        }

        public BonusQueries(IBonusRepository repository, IBrandOperations brandOperations)
        {
            _repository = repository;
            _brandOperations = brandOperations;
        }

        public BonusBalance GetBalance(Guid playerId, Guid? walletTemplateId = null)
        {
            var wallet = _repository.GetLockedWallet(playerId, walletTemplateId);

            return new BonusBalance
            {
                Main = wallet.Data.Main,
                Bonus = wallet.TotalBonus,
                BonusLock = wallet.Data.BonusLock
            };
        }
        public PlayerWagering GetWageringBalances(Guid playerId, Guid? walletTemplateId = null)
        {
            var wallet = _repository.GetLockedWallet(playerId, walletTemplateId);

            decimal requirement = 0;
            decimal completed = 0;

            foreach (var item in wallet.Data.BonusesRedeemed)
            {
                requirement += item.Rollover;
                completed += item.Contributions.Sum(x => x.Contribution);
            }

            var wagering = new PlayerWagering
            {
                Remaining = requirement - completed,
                Completed = completed,
                Requirement = requirement
            };

            return wagering;
        }

        internal Data.Bonus[] GetQualifiedBonuses(Guid playerId, BonusType type, RedemptionParams redemptionParams = null)
        {
            var player = _repository.GetLockedPlayer(playerId);
            return GetQualifiedBonuses(player, type, redemptionParams);
        }
        internal Data.Bonus[] GetQualifiedBonuses(Player player, BonusType type, RedemptionParams redemptionParams = null)
        {
            redemptionParams = redemptionParams ?? new RedemptionParams();
            return GetCurrentVersionBonuses(player.Data.Brand.Id)
                .Where(x => x.Template.Info.TemplateType == type)
                .ToList()
                .Select(x => new Entities.Bonus(x))
                .Where(bonus => bonus.QualifiesFor(player, QualificationPhase.Redemption, redemptionParams))
                .Select(x => x.Data)
                .ToArray();
        }
        internal List<Guid> GetQualifiedAutomaticBonusIds(Guid playerId, BonusType type, RedemptionParams redemptionParams)
        {
            var bonuses = GetQualifiedBonuses(playerId, type, redemptionParams)
                .Where(b => b.Template.Info.Mode == IssuanceMode.Automatic);

            return bonuses
                .Select(b => b.Id)
                .ToList();
        }
        internal IQueryable<Data.Bonus> GetCurrentVersionBonuses()
        {
            return _repository.GetCurrentVersionBonuses().AsNoTracking();
        }
        internal IQueryable<Data.Bonus> GetCurrentVersionBonuses(Guid brandId)
        {
            return GetCurrentVersionBonuses().Where(bonus => bonus.Template.Info.Brand.Id == brandId);
        }
        internal IQueryable<Data.Bonus> GetBonusesUsingTemplate(Template template)
        {
            return GetCurrentVersionBonuses(template.Info.Brand.Id)
                        .Where(bonus => bonus.Template.Id == template.Id && bonus.Template.Version == template.Version);
        }

        public IQueryable<Models.Data.Bonus> GetBonuses()
        {
            var bonuses = GetCurrentVersionBonuses()
                .Include(b => b.Template)
                .Include(b => b.Template.Info.Brand);

            return Mapper.Map<List<Models.Data.Bonus>>(bonuses).AsQueryable();
        }
        public Models.Data.Bonus GetBonusOrNull(Guid bonusId)
        {
            var bonus = GetCurrentVersionBonuses().SingleOrDefault(b => b.Id == bonusId);

            return bonus == null ? null : Mapper.Map<Models.Data.Bonus>(bonus);
        }

        public IQueryable<Template> GetCurrentVersionTemplates()
        {
            var currentIdVersion = _repository.Templates
                .GroupBy(template => template.Id)
                .Select(group => new { Id = group.Key, Version = group.Max(obj => obj.Version) });

            return _repository.Templates
                .Where(t => t.Status != TemplateStatus.Deleted)
                .Where(template => currentIdVersion.Contains(new { template.Id, template.Version }))
                .AsNoTracking();
        }
        public IQueryable<Models.Data.Template> GetTemplates()
        {
            var templates = GetCurrentVersionTemplates()
                .Include(t => t.Info)
                .Include(t => t.Info.Brand);

            return Mapper.Map<List<Models.Data.Template>>(templates).AsQueryable();
        }
        public Models.Data.Template GetTemplateOrNull(Guid templateId)
        {
            var template = GetCurrentVersionTemplates().SingleOrDefault(b => b.Id == templateId);

            return template == null ? null: Mapper.Map<Models.Data.Template>(template);
        }

        public Models.Data.Player GetPlayerOrNull(Guid playerId)
        {
            var player = _repository
                .Players
                .Include(x => x.Brand)
                .SingleOrDefault(x => x.Id == playerId);
            if (player == null)
                return null;

            return new Models.Data.Player
            {
                BrandId = player.Brand.Id,
                BrandTimezoneId = player.Brand.TimezoneId
            };
        }

        public IEnumerable<DepositQualifiedBonus> GetDepositQualifiedBonuses(Guid playerId, decimal transferAmount = 0)
        {
            var player = _repository.GetLockedPlayer(playerId);
            var currentVersionBonuses = GetCurrentVersionBonuses(player.Data.Brand.Id);
            var bonuses = currentVersionBonuses
                .Where(x => x.Template.Info.TemplateType == player.DepositQuailifiedBonusType &&
                            (x.Template.Info.Mode == IssuanceMode.AutomaticWithCode || x.Template.Info.Mode == IssuanceMode.ManualByPlayer))
                .ToList();

            var qualifiedBonuses = bonuses
                .Select(x => new Entities.Bonus(x))
                .Where(bonus => bonus.QualifiesFor(player, QualificationPhase.PreRedemption, new RedemptionParams { TransferAmount = transferAmount }));

            return qualifiedBonuses
                .Select(bonus => new DepositQualifiedBonus
                {
                    Id = bonus.Data.Id,
                    Name = bonus.Data.Name,
                    Description = bonus.Data.Description,
                    Code = bonus.Data.Code,
                    BonusAmount = bonus.CalculateReward(player, new RedemptionParams { TransferAmount = transferAmount })
                })
                .ToList();
        }

        public IEnumerable<DepositQualifiedBonus> GetVisibleDepositQualifiedBonuses(Guid playerId, decimal transferAmount = 0)
        {
            var player = _repository.GetLockedPlayer(playerId);
            var currentVersionBonuses = GetCurrentVersionBonuses(player.Data.Brand.Id);
            var bonuses = currentVersionBonuses
                .Where(x => x.Template.Info.TemplateType == player.DepositQuailifiedBonusType &&
                            x.Template.Info.Mode == IssuanceMode.ManualByPlayer)
                .ToList();

            var qualifiedBonuses = bonuses
                .Select(x => new Entities.Bonus(x))
                .Where(bonus => bonus.QualifiesFor(player, QualificationPhase.PreRedemption, new RedemptionParams { TransferAmount = transferAmount }));

            return qualifiedBonuses
                .Select(bonus => new DepositQualifiedBonus
                {
                    Id = bonus.Data.Id,
                    Name = bonus.Data.Name,
                    Code = bonus.Data.Code,
                    BonusAmount = bonus.CalculateReward(player, new RedemptionParams { TransferAmount = transferAmount })
                });
        }

        public IEnumerable<DepositQualifiedBonus> GetVisibleDepositQualifiedBonuses(Guid playerId)
        {
            var player = _repository.GetLockedPlayer(playerId);
            var currentVersionBonuses = GetCurrentVersionBonuses(player.Data.Brand.Id);
            var bonuses = currentVersionBonuses
                .Where(x => x.Template.Info.TemplateType == player.DepositQuailifiedBonusType &&
                            x.Template.Info.Mode == IssuanceMode.ManualByPlayer)
                .ToList();

            var qualifiedBonuses = bonuses
                .Select(x => new Entities.Bonus(x))
                .Where(bonus => bonus.QualifiesFor(player, QualificationPhase.PreRedemption, new RedemptionParams()));

            return qualifiedBonuses
                .Select(bonus =>
                {
                    var requiredAmount = bonus
                        .Data
                        .Template
                        .Rules
                        .RewardTiers
                        .Single(r => r.CurrencyCode == player.Data.CurrencyCode)
                        .BonusTiers
                        .Min(bt => bt.From);
                    var bonusAmount =  bonus
                        .Data
                        .Template
                        .Rules
                        .RewardTiers
                        .Single(r => r.CurrencyCode == player.Data.CurrencyCode)
                        .BonusTiers
                        .Min(bt => bt.Reward);
                    string percentage = string.Empty;
                    if (bonus
                        .Data
                        .Template.Rules.RewardType == BonusRewardType.Percentage)
                    {
                        percentage = (bonus
                            .Data
                            .Template
                            .Rules
                            .RewardTiers
                            .Single(r => r.CurrencyCode == player.Data.CurrencyCode)
                            .BonusTiers
                            .Min(bt => bt.Reward) * 100).ToString();
                    }
                    
                    return new DepositQualifiedBonus
                    {
                        Id = bonus.Data.Id,
                        Name = bonus.Data.Name,
                        Code = bonus.Data.Code,
                        RequiredAmount = requiredAmount,
                        Percenage = percentage,
                        BonusAmount = bonusAmount
                    };
                });
        }

        public DepositQualifiedBonus GetDepositQualifiedBonusByCode(Guid playerId, string code, decimal transferAmount)
        {
            var player = _repository.GetLockedPlayer(playerId);
            var currentVersionBonuses = GetCurrentVersionBonuses(player.Data.Brand.Id);
            var bonuses = currentVersionBonuses
                .Where(x => x.Template.Info.TemplateType == player.DepositQuailifiedBonusType && x.Template.Info.Mode == IssuanceMode.AutomaticWithCode)
                .Where(x => x.Code == code)
                .ToList();
            var qualifiedBonus = bonuses
                .Select(x => new Entities.Bonus(x))
                .SingleOrDefault(bonus => bonus.QualifiesFor(player, QualificationPhase.PreRedemption, new RedemptionParams { TransferAmount = transferAmount }));
            if (qualifiedBonus == null)
                return null;
            var requiredAmount = qualifiedBonus
                        .Data
                        .Template
                        .Rules
                        .RewardTiers
                        .Single(r => r.CurrencyCode == player.Data.CurrencyCode)
                        .BonusTiers
                        .Min(bt => bt.From);
            var percentage = string.Empty;
            if (qualifiedBonus
                .Data
                .Template.Rules.RewardType == BonusRewardType.Percentage)
            {
                percentage = (qualifiedBonus
                    .Data
                    .Template
                    .Rules
                    .RewardTiers
                    .Single(r => r.CurrencyCode == player.Data.CurrencyCode)
                    .BonusTiers
                    .Min(bt => bt.Reward) * 100).ToString();
            }
            return new DepositQualifiedBonus
            {
                Id = qualifiedBonus.Data.Id,
                Name = qualifiedBonus.Data.Name,
                Code = qualifiedBonus.Data.Code,
                RequiredAmount = requiredAmount,
                Percenage = percentage,
                BonusAmount = qualifiedBonus.CalculateReward(player, new RedemptionParams { TransferAmount = transferAmount })
            };
        }

        public IEnumerable<ManualByCsQualifiedBonus> GetManualByCsQualifiedBonuses(Guid playerId)
        {
            var player = _repository.GetLockedPlayer(playerId);
            var now = DateTimeOffset.Now.ToBrandOffset(player.Data.Brand.TimezoneId);
            return GetCurrentVersionBonuses(player.Data.Brand.Id)
                .ToList()
                .Select(x => new Entities.Bonus(x))
                .Where(bonus => bonus.QualifiesFor(player, QualificationPhase.PreRedemption, new RedemptionParams { IsIssuedByCs = true }))
                .Select(b => b.Data)
                .Select(b => new ManualByCsQualifiedBonus
                {
                    Id = b.Id,
                    Name = b.Name,
                    Code = b.Code,
                    Description = b.Description,
                    Type = b.Template.Info.TemplateType.ToString(),
                    Status = (now.Date >= b.ActiveTo.Date ? BonusQualificationStatus.Expired : BonusQualificationStatus.Active).ToString()
                });
        }
        public List<QualifiedTransaction> GetManualByCsQualifiedTransactions(Guid playerId, Guid bonusId)
        {
            var player = _repository.GetLockedPlayer(playerId);
            var bonus = _repository.GetLockedBonus(bonusId);

            return bonus.GetQualifiedTransactions(player).ToList();
        }

        public List<BonusRedemption> GetClaimableRedemptions(Guid playerId)
        {
            var player = _repository.GetLockedPlayer(playerId);
            var redemptions = player.GetClaimableRedemptions();

            return Mapper.Map<List<BonusRedemption>>(redemptions);
        }
        public List<BonusRedemption> GetCompletedBonuses(Guid playerId)
        {
            var player = _repository.GetLockedPlayer(playerId);
            var redemptions = player.GetCompletedRedemptions();

            return Mapper.Map<List<BonusRedemption>>(redemptions.Select(br => br.Data));
        }
        public List<BonusRedemption> GetBonusesWithIncompleteWagering(Guid playerId)
        {
            var player = _repository.GetLockedPlayer(playerId);
            var redemptions = player.GetRedemptionsWithActiveRollover();

            return Mapper.Map<List<BonusRedemption>>(redemptions.Select(br => br.Data));
        }
        public IQueryable<BonusRedemption> GetBonusRedemptions(Guid playerId)
        {
            var redemptions = _repository.Players
                .Include(p => p.Brand)
                .Include(p => p.Wallets.Select(w => w.BonusesRedeemed.Select(br => br.Bonus)))
                .Include(p => p.Wallets.Select(w => w.BonusesRedeemed.Select(br => br.Contributions)))
                .Single(p => p.Id == playerId)
                .Wallets
                .SelectMany(w => w.BonusesRedeemed);

            return Mapper.Map<List<BonusRedemption>>(redemptions).AsQueryable();
        }
        public BonusRedemption GetBonusRedemption(Guid playerId, Guid redemptionId)
        {
            var redemption = _repository.GetBonusRedemption(playerId, redemptionId).Data;

            return Mapper.Map<BonusRedemption>(redemption);
        }

        //Validation calls
        public ValidationResult GetValidationResult(IssueBonusByCs model)
        {
            return new IssueByCsValidator(this, _repository, _brandOperations).Validate(model);
        }
        public ValidationResult GetValidationResult(ToggleBonusStatus model)
        {
            return new ToggleBonusStatusValidator(this).Validate(model);
        }
        public ValidationResult GetValidationResult(DeleteTemplate model)
        {
            return new TemplateDeletionValidator(this).Validate(model);
        }
        public ValidationResult GetValidationResult(CreateUpdateBonus model)
        {
            return new BonusValidator(_repository, this).Validate(model);
        }
        public ValidationResult GetValidationResult(CreateUpdateTemplate model)
        {
            return new TemplateValidator(_repository, this).Validate(model);
        }
        public ValidationResult GetValidationResult(CancelBonusRedemption model)
        {
            return new CancelBonusValidator(_repository).Validate(model);
        }
        public ValidationResult GetValidationResult(ClaimBonusRedemption model)
        {
            return new ClaimBonusValidator(_repository).Validate(model);
        }
        public ValidationResult GetValidationResult(DepositBonusApplication model)
        {
            return new DepositBonusApplicationValidator(_repository).Validate(model);
        }
        public ValidationResult GetValidationResult(FundInBonusApplication model)
        {
            return new FundInBonusApplicationValidator(_repository).Validate(model);
        }
        public ValidationResult GetValidationResult(FirstDepositApplication model)
        {
            return new FirstDepositApplicationValidator(_repository).Validate(model);
        }
    }
}