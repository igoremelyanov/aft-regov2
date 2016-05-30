using System;
using System.Collections.Generic;
using AFT.RegoV2.Bonus.Core.Models.Enums;

namespace AFT.RegoV2.Bonus.Core.Models.Data
{
    public class Template
    {
        public Guid Id { get; set; }
        public int Version { get; set; }

        public TemplateInfo Info { get; set; }
        public TemplateAvailability Availability { get; set; }
        public TemplateRules Rules { get; set; }
        public TemplateWagering Wagering { get; set; }
        public TemplateNotification Notification { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
        public TemplateStatus Status { get; set; }
    }

    public class TemplateInfo
    {
        public Guid BrandId { get; set; }
        public string Name { get; set; }
        public string TemplateType { get; set; }
        public string Description { get; set; }
        public Guid WalletTemplateId { get; set; }
        public bool IsWithdrawable { get; set; }
        public IssuanceMode Mode { get; set; }
    }

    public class TemplateAvailability
    {
        public TemplateAvailability()
        {
            VipLevels = new List<string>();
            ExcludeBonuses = new List<Guid>();
            ExcludeRiskLevels = new List<Guid>();
        }

        public Guid? ParentBonusId { get; set; }
        public DateTimeOffset? PlayerRegistrationDateFrom { get; set; }
        public DateTimeOffset? PlayerRegistrationDateTo { get; set; }
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