using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.DomainServices;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Core.Common.Utils;
using FluentAssertions;
using NUnit.Framework;
using BonusType = AFT.RegoV2.Bonus.Core.Models.Enums.BonusType;

namespace AFT.RegoV2.Bonus.Tests.Unit.Qualification
{
    internal class RedemptionQualificationTests : UnitTestBase
    {
        [Test]
        public void Per_player_bonus_issuance_limit_qualification_per_lifetime()
        {
            MakeDeposit(PlayerId);

            var bonus = CreateFirstDepositBonus();
            bonus.Template.Availability.PlayerRedemptionsLimitType = BonusPlayerRedemptionsLimitType.Lifetime;
            bonus.Template.Availability.PlayerRedemptionsLimit = 1;
            bonus.Template.Info.TemplateType = BonusType.ReloadDeposit;

            MakeDeposit(PlayerId);
            MakeDeposit(PlayerId);

            BonusRedemptions.Count.Should().Be(1, because: "Only first bonus is processed.");
        }

        [TestCase(BonusPlayerRedemptionsLimitType.Day, 1)]
        [TestCase(BonusPlayerRedemptionsLimitType.Week, 7)]
        [TestCase(BonusPlayerRedemptionsLimitType.Month, 31)]
        public void Per_player_bonus_issuance_limit_qualification_per_period(BonusPlayerRedemptionsLimitType periodType, int daysOffset)
        {
            MakeDeposit(PlayerId);

            var bonus = CreateFirstDepositBonus();

            bonus.ActiveTo = bonus.ActiveTo.AddDays(60);
            bonus.Template.Availability.PlayerRedemptionsLimitType = periodType;
            bonus.Template.Availability.PlayerRedemptionsLimit = 2;
            bonus.Template.Info.TemplateType = BonusType.ReloadDeposit;

            MakeDeposit(PlayerId);
            MakeDeposit(PlayerId);
            MakeDeposit(PlayerId);

            BonusRedemptions.Count(br => br.ActivationState == ActivationStatus.Activated).Should().Be(2, because: "Only first 2 bonuses should be activated.");
            BonusRedemptions.Count(br => br.ActivationState != ActivationStatus.Activated).Should().Be(0, because: "Last deposit should not redeem bonus.");

            // Switch to the next datetime period
            SystemTime.Factory = () => DateTimeOffset.Now.AddDays(daysOffset);
            MakeDeposit(PlayerId);

            BonusRedemptions.Count(br => br.ActivationState == ActivationStatus.Activated).Should().Be(3, because: "Deposit falls into next period.");

            SystemTime.Factory = () => DateTimeOffset.Now;
        }

        [Test]
        public void Per_bonus_bonus_issuance_limit_qualification()
        {
            MakeDeposit(PlayerId);

            var bonus = CreateFirstDepositBonus();
            bonus.Template.Availability.RedemptionsLimit = 2;
            bonus.Template.Info.TemplateType = BonusType.ReloadDeposit;

            MakeDeposit(PlayerId);
            MakeDeposit(PlayerId);
            MakeDeposit(PlayerId);

            BonusRedemptions.Count.Should().Be(2, because: "Only first 2 bonuses are processed.");
        }

        [Test]
        public void Per_currency_reward_limit_qualification()
        {
            MakeDeposit(PlayerId);

            var bonus = CreateFirstDepositBonus();
            bonus.Template.Rules.RewardTiers.Single().RewardAmountLimit = bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward;
            bonus.Template.Info.TemplateType = BonusType.ReloadDeposit;

            MakeDeposit(PlayerId);
            MakeDeposit(PlayerId);

            BonusRedemptions.Count.Should().Be(1, because: "Only first bonus is processed.");
        }

        [Test]
        public void Bonus_is_not_issued_if_ParentBonus_is_not_issued()
        {
            var parentBonusRule = CreateFirstDepositBonus();
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Availability.ParentBonusId = parentBonusRule.Id;

            MakeDeposit(PlayerId);

            var bonusRedemption = BonusRedemptions.SingleOrDefault();
            bonusRedemption.Should().NotBeNull();
            bonusRedemption.Bonus.Id.Should().Be(parentBonusRule.Id);
        }

        [Test]
        public void Player_VIP_level_qualification()
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Availability.VipLevels = new List<BonusVip> { new BonusVip { Code = "Bronze" } };

            MakeDeposit(PlayerId);

            BonusRedemptions.Should().BeEmpty(because: "Player has not qualified VIP level");
        }

        [TestCase(true, ExpectedResult = 0)]
        [TestCase(false, ExpectedResult = 1)]
        public int Player_Risk_level_qualification(bool riskLevelActive)
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            var riskLevelId = bonus.Template.Info.Brand.RiskLevels.Single().Id;
            bonus.Template.Availability.ExcludeRiskLevels = new List<RiskLevelExclude> { new RiskLevelExclude { ExcludedRiskLevelId = riskLevelId } };

            TagPlayerWithFraudRiskLevel(PlayerId, riskLevelId);
            if (riskLevelActive == false)
            {
                DeactivateRiskLevel(riskLevelId);
            }

            return BonusQueries.GetDepositQualifiedBonuses(PlayerId).Count();
        }

        [TestCase(0, ExpectedResult = 0)]
        [TestCase(1, ExpectedResult = 1)]
        [TestCase(2, ExpectedResult = 1)]
        [TestCase(3, ExpectedResult = 0)]
        public int List_does_not_contain_bonuses_for_player_with_invalid_registration_date_range(int registeredDaysAgo)
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            var brandNow = DateTimeOffset.Now.ToBrandOffset(bonus.Template.Info.Brand.TimezoneId);
            bonus.Template.Availability.PlayerRegistrationDateFrom = brandNow.AddDays(-2);
            bonus.Template.Availability.PlayerRegistrationDateTo = brandNow.AddMilliseconds(-1);

            BonusRepository.Players.Single().DateRegistered = brandNow.AddDays(-1 * registeredDaysAgo);

            return BonusQueries.GetDepositQualifiedBonuses(PlayerId).Count();
        }

        [TestCase(0, ExpectedResult = 1)]
        [TestCase(1, ExpectedResult = 0)]
        [TestCase(6, ExpectedResult = 0)]
        [TestCase(7, ExpectedResult = 1)]
        [TestCase(8, ExpectedResult = 1)]
        public int List_does_not_contain_bonuses_for_player_with_invalid_within_registration_days(int withinRegistrationDays)
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Availability.WithinRegistrationDays = withinRegistrationDays;

            var player = BonusRepository.Players.First();
            player.DateRegistered = player.DateRegistered.AddDays(-7);

            MakeDeposit(PlayerId);

            return BonusRedemptions.Count(br => br.ActivationState == ActivationStatus.Activated);
        }

        [Test]
        public void Bonus_is_not_issed_if_no_matching_currency_is_found()
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Rules.RewardTiers.Single().CurrencyCode = "ABC";

            MakeDeposit(PlayerId);

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void Application_to_bonus_is_active_inside_Duration_only()
        {
            MakeDeposit(PlayerId);
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.TemplateType = BonusType.ReloadDeposit;
            bonus.DurationType = DurationType.Custom;
            bonus.DurationStart = SystemTime.Now.AddMinutes(5);
            bonus.DurationEnd = SystemTime.Now.AddMinutes(10);

            MakeDeposit(PlayerId);
            BonusRedemptions.Should().BeEmpty();

            SystemTime.Factory = () => DateTimeOffset.Now.AddMinutes(11);

            MakeDeposit(PlayerId);
            BonusRedemptions.Should().BeEmpty();

            SystemTime.Factory = () => DateTimeOffset.Now.AddMinutes(9);

            MakeDeposit(PlayerId);
            BonusRedemptions.Should().NotBeEmpty();

            SystemTime.Factory = () => DateTimeOffset.Now;
        }
    }
}