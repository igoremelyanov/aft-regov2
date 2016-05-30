using System;
using System.Collections.Generic;
using AFT.RegoV2.Bonus.Api.Interface.Responses;

namespace AFT.RegoV2.AdminApi.Interface.Bonus
{
    public class TemplateDataResponse
    {
        public Template Template { get; set; }
        public IEnumerable<string> NotificationTriggers { get; set; }
        public IEnumerable<BonusData> Bonuses { get; set; }
        public IEnumerable<Licensee> Licensees { get; set; }
        public IEnumerable<Game> Games { get; set; }
    }

    public class Licensee
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<TemplateBrand> Brands { get; set; }
    }

    public class Game
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
    }

    public class TemplateBrand
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<TemplateVipLevel> VipLevels { get; set; }
        public IEnumerable<TemplateCurrency> Currencies { get; set; }
        public IEnumerable<TemplateWalletTemplate> WalletTemplates { get; set; }
        public IEnumerable<Guid> Products { get; set; }
        public IEnumerable<TemplateRiskLevel> RiskLevels { get; set; }
    }

    public class TemplateVipLevel
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class TemplateCurrency
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class TemplateWalletTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsMain { get; set; }
    }

    public class TemplateRiskLevel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class Template
    {
        public Guid Id { get; set; }
        public int Version { get; set; }

        public TemplateInfo Info { get; set; }
        public TemplateAvailability Availability { get; set; }
        public TemplateRules Rules { get; set; }
        public TemplateWagering Wagering { get; set; }
        public TemplateNotification Notification { get; set; }
    }

    public class TemplateInfo
    {
        public Guid? LicenseeId { get; set; }
        public string LicenseeName { get; set; }
        public Guid? BrandId { get; set; }
        public string BrandName { get; set; }
        public string Name { get; set; }
        public string TemplateType { get; set; }
        public string Description { get; set; }
        public Guid WalletTemplateId { get; set; }
        public bool IsWithdrawable { get; set; }
        public string Mode { get; set; }
    }

    public class TemplateAvailability
    {
        public Guid? ParentBonusId { get; set; }
        public string PlayerRegistrationDateFrom { get; set; }
        public string PlayerRegistrationDateTo { get; set; }
        public int WithinRegistrationDays { get; set; }
        public List<string> VipLevels { get; set; }
        public int ExcludeOperation { get; set; }
        public List<Guid> ExcludeBonuses { get; set; }
        public List<Guid> ExcludeRiskLevels { get; set; }
        public int PlayerRedemptionsLimit { get; set; }
        public int PlayerRedemptionsLimitType { get; set; }
        public int RedemptionsLimit { get; set; }
    }

    public class TemplateRules
    {
        public TemplateRules()
        {
            RewardTiers = new List<RewardTier>();
            FundInWallets = new List<Guid>();
        }

        public int RewardType { get; set; }
        public List<RewardTier> RewardTiers { get; set; }
        public bool IsAutoGenerateHighDeposit { get; set; }

        public decimal ReferFriendMinDepositAmount { get; set; }
        public decimal ReferFriendWageringCondition { get; set; }

        public List<Guid> FundInWallets { get; set; }
    }

    public class RewardTier
    {
        public RewardTier()
        {
            BonusTiers = new List<TemplateTier>();
        }

        public string CurrencyCode { get; set; }
        public List<TemplateTier> BonusTiers { get; set; }
        public decimal RewardAmountLimit { get; set; }
    }

    public class TemplateTier
    {
        public decimal Reward { get; set; }
        public decimal From { get; set; }
        public decimal? To { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal NotificationPercentThreshold { get; set; }
    }

    public class TemplateWagering
    {
        public TemplateWagering()
        {
            GameContributions = new List<GameContribution>();
        }

        public bool HasWagering { get; set; }
        public int Method { get; set; }
        public decimal Multiplier { get; set; }
        public decimal Threshold { get; set; }
        public List<GameContribution> GameContributions { get; set; }
        public bool IsAfterWager { get; set; }
    }

    public class GameContribution
    {
        public Guid GameId { get; set; }
        public string Name { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Contribution { get; set; }
    }

    public class TemplateNotification
    {
        public TemplateNotification()
        {
            EmailTriggers = new List<string>();
            SmsTriggers = new List<string>();
        }

        public List<string> EmailTriggers { get; set; }
        public List<string> SmsTriggers { get; set; }
    }
}