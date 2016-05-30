using System;
using System.Collections.Generic;
using AFT.RegoV2.Bonus.Core.Models.Enums;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    public class CreateUpdateTemplateRules
    {
        public CreateUpdateTemplateRules()
        {
            RewardTiers = new List<CreateUpdateRewardTier>();
            FundInWallets = new List<Guid>();
        }

        public BonusRewardType RewardType { get; set; }
        public List<CreateUpdateRewardTier> RewardTiers { get; set; }
        public bool IsAutoGenerateHighDeposit { get; set; }

        public decimal ReferFriendMinDepositAmount { get; set; }
        public decimal ReferFriendWageringCondition { get; set; }

        public List<Guid> FundInWallets { get; set; }
    }

    public class CreateUpdateRewardTier
    {
        public CreateUpdateRewardTier()
        {
            BonusTiers = new List<CreateUpdateTemplateTier>();
        }

        public string CurrencyCode { get; set; }
        public List<CreateUpdateTemplateTier> BonusTiers { get; set; }
        public decimal RewardAmountLimit { get; set; }
    }

    public class CreateUpdateTemplateTier
    {
        public decimal Reward { get; set; }
        public decimal From { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal NotificationPercentThreshold { get; set; }
    }
}