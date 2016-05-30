using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Features
{
    internal class RewardLimitingTests : UnitTestBase
    {
        [Test]
        public void Percentage_bonus_reward_is_limited_by_transaction_limit()
        {
            const decimal limit = 50m;
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;
            bonus.Template.Rules.RewardTiers.Single().Tiers.Single().Reward = 0.5m;
            bonus.Template.Rules.RewardTiers.Single().Tiers.Single().MaxAmount = limit;

            MakeDeposit(PlayerId, 1000);

            Assert.AreEqual(limit, BonusRedemptions.First().Amount);
        }

        [Test]
        public void Bonus_reward_is_limited_by_RewardTier_reward_limit()
        {
            const decimal limit = 45m;
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.TemplateType = BonusType.ReloadDeposit;
            bonus.Template.Rules.RewardTiers.Single().RewardAmountLimit = limit;

            //Make 1st deposit so 2nd and 3rd deposits will be qualified
            MakeDeposit(PlayerId);
            MakeDeposit(PlayerId);

            Assert.AreEqual(27, BonusRedemptions.First().Amount);

            MakeDeposit(PlayerId);

            Assert.AreEqual(18, BonusRedemptions.Last().Amount);
        }
    }
}