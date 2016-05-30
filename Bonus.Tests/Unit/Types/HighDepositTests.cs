using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Core.Messaging.Interface.Commands;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Tests.Common.TestDoubles;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Types
{
    internal class HighDepositTests : UnitTestBase
    {
        [TestCase(500, 50)]
        [TestCase(600, 50)]
        [TestCase(1000, 150)]
        public void Can_redeem_bonus_with_matched_tier(int depositAmount, int expectedRedemptionsAmount)
        {
            CreateBonusWithHighDepositTiers(false);
            MakeDeposit(PlayerId, depositAmount);

            BonusRedemptions.All(br => br.ActivationState == ActivationStatus.Activated).Should().BeTrue();
            BonusRedemptions.Sum(br => br.Amount).Should().Be(expectedRedemptionsAmount);
        }

        [Test]
        public void Bonus_rewards_are_calculated_correctly_across_several_deposits()
        {
            CreateBonusWithHighDepositTiers(false);
            MakeDeposit(PlayerId, 600);

            var bonusRedemption = BonusRedemptions.SingleOrDefault(br => br.Amount == 50);
            Assert.NotNull(bonusRedemption);
            Assert.AreEqual(ActivationStatus.Activated, bonusRedemption.ActivationState);

            MakeDeposit(PlayerId, 400);

            bonusRedemption = BonusRedemptions.SingleOrDefault(br => br.Amount == 100);
            Assert.NotNull(bonusRedemption);
            Assert.AreEqual(ActivationStatus.Activated, bonusRedemption.ActivationState);
        }

        [TestCase(500, 50)]
        [TestCase(600, 50)]
        [TestCase(1000, 100)]
        public void Can_redeem_bonus_with_matched_auto_generate_tier(int depositAmount, int expectedRedemptionsAmount)
        {
            CreateBonusWithHighDepositTiers();
            MakeDeposit(PlayerId, depositAmount);

            BonusRedemptions.All(br => br.ActivationState == ActivationStatus.Activated).Should().BeTrue();
            BonusRedemptions.Sum(br => br.Amount).Should().Be(expectedRedemptionsAmount);
        }

        [Test]
        public void Auto_generated_bonus_rewards_are_calculated_correctly_across_several_deposits()
        {
            CreateBonusWithHighDepositTiers();
            MakeDeposit(PlayerId, 600);
            MakeDeposit(PlayerId, 400);

            BonusRedemptions.Count.Should().Be(2);
            BonusRedemptions.All(br => br.ActivationState == ActivationStatus.Activated).Should().BeTrue();
            BonusRedemptions.All(br => br.Amount == 50).Should().BeTrue();
        }

        [Test]
        public void Sms_notification_is_sent_upon_hitting_tier_threshold()
        {
            CreateBonusWithHighDepositTiers(false);
            MakeDeposit(PlayerId, 450);

            var bus = (FakeServiceBus)ServiceBus;
            var command = (SendPlayerAMessage)bus.PublishedCommands.Last();

            command.MessageType.Should().Be(MessageType.HighDepositReminder);
            command.MessageDeliveryMethod.Should().Be(MessageDeliveryMethod.Sms);
        }

        [Test]
        public void Sms_notification_is_sent_upon_hitting_auto_generated_tier_threshold()
        {
            CreateBonusWithHighDepositTiers();
            MakeDeposit(PlayerId, 900);

            var bus = (FakeServiceBus)ServiceBus;
            var command = (SendPlayerAMessage)bus.PublishedCommands.Last();

            command.MessageType.Should().Be(MessageType.HighDepositReminder);
            command.MessageDeliveryMethod.Should().Be(MessageDeliveryMethod.Sms);
        }

        [Test]
        public void Cannot_redeem_bonus_with_unmatched_tiers()
        {
            CreateBonusWithHighDepositTiers(false);
            MakeDeposit(PlayerId, 450);

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void Cannot_redeem_bonus_with_unmatched_auto_generated_tiers()
        {
            CreateBonusWithHighDepositTiers();
            MakeDeposit(PlayerId, 450);

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void Reward_threshold_is_calculated_correctly_for_auto_generated_tiers()
        {
            var bonus = CreateBonusWithHighDepositTiers();
            var player = BonusRepository.GetLockedPlayer(PlayerId);

            MakeDeposit(PlayerId, 950);

            var bonusRewardThreshold = new Core.Entities.Bonus(bonus).CalculateRewardThreshold(player);

            Assert.AreEqual(1000, bonusRewardThreshold.DepositAmountRequired);
            Assert.AreEqual(50, bonusRewardThreshold.BonusAmount);
            Assert.AreEqual(50, bonusRewardThreshold.RemainingAmount);
        }

        [Test]
        public void Reward_threshold_is_calculated_correctly()
        {
            var bonus = CreateBonusWithHighDepositTiers(false);
            var player = BonusRepository.GetLockedPlayer(PlayerId);

            MakeDeposit(PlayerId, 950);

            var bonusRewardThreshold = new Core.Entities.Bonus(bonus).CalculateRewardThreshold(player);

            Assert.AreEqual(1000, bonusRewardThreshold.DepositAmountRequired);
            Assert.AreEqual(100, bonusRewardThreshold.BonusAmount);
            Assert.AreEqual(50, bonusRewardThreshold.RemainingAmount);
        }

        [Test]
        public void Reward_threshold_is_null_if_player_deposited_more_then_requires_last_tier()
        {
            var bonus = CreateBonusWithHighDepositTiers(false);
            var player = BonusRepository.GetLockedPlayer(PlayerId);

            MakeDeposit(PlayerId, 1100);

            var bonusRewardThreshold = new Core.Entities.Bonus(bonus).CalculateRewardThreshold(player);

            Assert.Null(bonusRewardThreshold);
        }

        [Test]
        public void Redemptions_are_not_created_if_all_reward_tiers_were_redeemed_this_month()
        {
            CreateBonusWithHighDepositTiers(false);
            MakeDeposit(PlayerId, 1000);
            MakeDeposit(PlayerId, 1000);

            BonusRepository.GetLockedPlayer(PlayerId).BonusesRedeemed.Count.Should().Be(2);
        }
    }
}