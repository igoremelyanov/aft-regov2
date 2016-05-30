using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Types
{
    internal class ReferFriendTests : UnitTestBase
    {
        [Test]
        public void Can_redeem_refer_friend_bonus_of_matched_tier()
        {
            CreateBonusWithReferFriendTiers();

            var bonusPlayer = BonusRepository.GetLockedPlayer(PlayerId);
            for (var i = 1; i <= 9; i++)
            {
                CompleteReferAFriendRequirments(PlayerId);

                var referRedemption = bonusPlayer.BonusesRedeemed.ElementAt(i - 1);
                Assert.AreEqual(ActivationStatus.Activated, referRedemption.ActivationState);
                Assert.AreEqual(GetExpectedRedemptionAmount(i), referRedemption.Amount);
            }
        }

        [Test]
        public void Can_redeem_refer_friend_bonus_with_exact_matched_tier()
        {
            var referTier = new BonusTier
            {
                From = 1,
                Reward = 10
            };
            var bonus = CreateBonusWithReferFriendTiers();
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Clear();
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Add(referTier);

            CompleteReferAFriendRequirments(PlayerId);

            var bonusPlayer = BonusRepository.GetLockedPlayer(PlayerId);
            var referRedemption = bonusPlayer.BonusesRedeemed.SingleOrDefault(x => x.ActivationState == ActivationStatus.Activated);
            Assert.NotNull(referRedemption);
            Assert.AreEqual(referTier.Reward, referRedemption.Amount);
        }

        [Test]
        public void Cannot_redeem_refer_friend_bonus_with_wager_requirement_not_met()
        {
            CreateBonusWithReferFriendTiers();
            var newPlayer = CreatePlayer(PlayerId);
            MakeDeposit(newPlayer.Id);

            PlaceAndLoseBet(90, newPlayer.Id);

            var redemption = BonusRepository.GetLockedPlayer(PlayerId).BonusesRedeemed.Single();
            redemption.ActivationState.Should().Be(ActivationStatus.Pending);
        }

        [Test]
        public void Cannot_redeem_refer_friend_bonus_with_min_first_deposit_requirement_not_met()
        {
            var bonus = CreateBonusWithReferFriendTiers();
            bonus.Template.Rules.ReferFriendMinDepositAmount = 300;
            var newPlayer = CreatePlayer(PlayerId);

            MakeDeposit(newPlayer.Id);
            PlaceAndLoseBet(90, newPlayer.Id);

            var redemption = BonusRepository.GetLockedPlayer(PlayerId).BonusesRedeemed.Single();
            redemption.ActivationState.Should().Be(ActivationStatus.Pending);
        }

        [Test]
        public void Playing_beyound_minimal_rollover_does_not_create_additional_bonus_redemptions()
        {
            CreateBonusWithReferFriendTiers();

            var referredPlayer = CreatePlayer(PlayerId);
            MakeDeposit(referredPlayer.Id);
            PlaceAndWinBet(200, 200, referredPlayer.Id);
            PlaceAndWinBet(200, 200, referredPlayer.Id);

            var bonusPlayer = BonusRepository.GetLockedPlayer(PlayerId);
            bonusPlayer.BonusesRedeemed.Count.Should().Be(1);
        }

        [Test]
        public void Can_not_get_2_refer_a_friend_redemptions_using_1_referred()
        {
            //Getting bonus #1
            var bonus = CreateBonusWithReferFriendTiers();
            var referredPlayer = CreatePlayer(PlayerId);
            MakeDeposit(referredPlayer.Id);
            PlaceAndWinBet(200, 200, referredPlayer.Id);
            bonus.IsActive = false;

            //Trying to get bonus #2
            bonus = CreateBonusWithReferFriendTiers();
            bonus.Template.Rules.ReferFriendWageringCondition = 2;
            MakeDeposit(referredPlayer.Id, 201);
            PlaceAndWinBet(201, 201, referredPlayer.Id);

            var bonusPlayer = BonusRepository.GetLockedPlayer(PlayerId);
            bonusPlayer.BonusesRedeemed.Count.Should().Be(1);
        }

        [Test]
        public void Referer_can_claim_2_different_bonus_redemptions()
        {
            var bonus = CreateBonusWithReferFriendTiers();
            bonus.Template.Info.Mode = IssuanceMode.ManualByPlayer;
            CompleteReferAFriendRequirments(PlayerId);
            bonus.IsActive = false;

            var bonus2 = CreateBonusWithReferFriendTiers();
            bonus2.Template.Info.Mode = IssuanceMode.ManualByPlayer;
            CompleteReferAFriendRequirments(PlayerId);
            bonus.IsActive = true;

            var bonusPlayer = BonusRepository.GetLockedPlayer(PlayerId);
            bonusPlayer.BonusesRedeemed.Count.Should().Be(2);
            bonusPlayer.BonusesRedeemed.All(br => br.Amount == 10).Should().BeTrue();

            BonusCommands.ClaimBonusRedemption(new ClaimBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusPlayer.BonusesRedeemed.ElementAt(0).Id
            });
            BonusCommands.ClaimBonusRedemption(new ClaimBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusPlayer.BonusesRedeemed.ElementAt(1).Id
            });
        }

        [Test]
        public void Referer_tiers_are_bonus_dependent()
        {
            var bonus1 = CreateBonusWithReferFriendTiers();
            CompleteReferAFriendRequirments(PlayerId);
            bonus1.IsActive = false;

            var bonus2 = CreateBonusWithReferFriendTiers();
            bonus2.Template.Rules.RewardTiers.Single().BonusTiers = new List<TierBase>
            {
                new BonusTier {From = 1, Reward = 10}
            };
            CompleteReferAFriendRequirments(PlayerId);
            bonus1.IsActive = true;

            var bonusPlayer = BonusRepository.GetLockedPlayer(PlayerId);
            bonusPlayer.BonusesRedeemed.Count.Should().Be(2);
            bonusPlayer.BonusesRedeemed.All(br => br.Amount == 10).Should().BeTrue();
        }

        private decimal GetExpectedRedemptionAmount(int referralCount)
        {
            if (referralCount >= 1 && referralCount <= 3)
            {
                return 10;
            }
            if (referralCount >= 4 && referralCount <= 6)
            {
                return 20;
            }
            if (referralCount >= 7 && referralCount <= 9)
            {
                return 30;
            }
            return 0;
        }
    }
}