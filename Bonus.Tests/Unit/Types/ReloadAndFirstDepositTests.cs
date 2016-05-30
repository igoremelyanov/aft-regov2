using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;
using RewardTier = AFT.RegoV2.Bonus.Core.Data.RewardTier;

namespace AFT.RegoV2.Bonus.Tests.Unit.Types
{
    internal class ReloadAndFirstDepositTests : UnitTestBase
    {
        [Test]
        public void Percentage_deposit_bonus_reward_calculated_correctly()
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;

            MakeDeposit(PlayerId);

            BonusRedemptions.Last().Amount.Should().Be(5400);
        }

        [Test]
        public void Flat_deposit_bonus_reward_calculated_correctly()
        {
            var bonus = CreateFirstDepositBonus();
            //Adding a reward for RMB currency. This should not be used, 'cos Player's currency is CAD
            bonus.Template.Rules.RewardTiers.Add(new RewardTier
            {
                CurrencyCode = "RMB",
                BonusTiers = new List<TierBase>
                {
                    new BonusTier
                    {
                        Reward = 100
                    }
                }
            });

            MakeDeposit(PlayerId);

            BonusRedemptions.Last().Amount.Should().Be(27);
        }

        [Test]
        public void First_deposit_bonus_works()
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.TemplateType = BonusType.FirstDeposit;

            MakeDeposit(PlayerId);

            BonusRedemptions.Last().Amount.Should().Be(27);
        }

        [Test]
        public void First_deposit_bonus_is_applied_to_first_deposit_only()
        {
            MakeDeposit(PlayerId);

            CreateFirstDepositBonus();

            MakeDeposit(PlayerId);

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void Reload_deposit_bonus_is_applied_to_second_and_sunsequent_deposits()
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.TemplateType = BonusType.ReloadDeposit;

            MakeDeposit(PlayerId);
            BonusRedemptions.Should().BeEmpty();

            MakeDeposit(PlayerId);
            BonusRedemptions.Should().NotBeEmpty();
        }

        [Test]
        public void Bonus_redemtion_amount_is_recalculated_if_deposit_amount_changes()
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 0.5m;

            var depositId = SubmitDeposit(PlayerId);
            ApproveDeposit(depositId, PlayerId, 100);

            BonusRedemptions.First().Amount.Should().Be(50);
            bonus.Statistic.TotalRedeemedAmount.Should().Be(50);
        }

        [TestCase(100, 10)]
        [TestCase(200, 40)]
        [TestCase(300, 50)]
        public void Can_redeem_deposit_bonus_with_matched_percentage_specific_rules(int depositAmount,
            int expectedRedemptionAmount)
        {
            var bonus = CreateBonusWithBonusTiers(BonusRewardType.TieredPercentage);

            MakeDeposit(PlayerId, depositAmount);

            var bonusRedemption = BonusRedemptions.First();

            Assert.AreEqual(PlayerId, bonusRedemption.Player.Id);
            Assert.AreEqual(bonus.Id, bonusRedemption.Bonus.Id);
            Assert.AreEqual(ActivationStatus.Activated, bonusRedemption.ActivationState);
            Assert.AreEqual(expectedRedemptionAmount, bonusRedemption.Amount);
        }

        [TestCase(0.2, BonusRewardType.TieredPercentage, TestName = "Cannot redeem percentage tiered deposit bonus with invalid deposit amount")]
        [TestCase(0.9, BonusRewardType.TieredAmount, TestName = "Cannot redeem flat amount tiered deposit bonus with invalid deposit amount")]
        public void Cannot_redeem_deposit_bonus(decimal depositAmount, BonusRewardType rewardType)
        {
            CreateBonusWithBonusTiers(rewardType);

            MakeDeposit(PlayerId, depositAmount);

            BonusRedemptions.Should().BeEmpty();
        }

        [TestCase(100, 10)]
        [TestCase(200, 20)]
        [TestCase(500, 30)]
        public void Can_redeem_deposit_bonus_with_matched_fixed_amount_specific_rules(int depositAmount,
            int expectedRedemptionAmount)
        {
            var bonus = CreateBonusWithBonusTiers(BonusRewardType.TieredAmount);

            MakeDeposit(PlayerId, depositAmount);

            var bonusRedemption = BonusRedemptions.First();

            Assert.AreEqual(PlayerId, bonusRedemption.Player.Id);
            Assert.AreEqual(bonus.Id, bonusRedemption.Bonus.Id);
            Assert.AreEqual(ActivationStatus.Activated, bonusRedemption.ActivationState);
            Assert.AreEqual(expectedRedemptionAmount, bonusRedemption.Amount);
        }

        [Test]
        public void Cancel_of_deposit_negates_related_bonus_redemption()
        {
            CreateFirstDepositBonus();

            var depositId = SubmitDeposit(PlayerId, 100);
            UnverifyDeposit(depositId, PlayerId);

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Negated);
        }

        [Test]
        public void Cancel_of_deposit_discards_applied_bonus_statistics_changes()
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 0.5m;

            var depositId = SubmitDeposit(PlayerId, 100);

            Assert.AreEqual(50, bonus.Statistic.TotalRedeemedAmount);
            Assert.AreEqual(1, bonus.Statistic.TotalRedemptionCount);

            UnverifyDeposit(depositId, PlayerId);

            Assert.AreEqual(0, bonus.Statistic.TotalRedeemedAmount);
            Assert.AreEqual(0, bonus.Statistic.TotalRedemptionCount);
        }
    }
}