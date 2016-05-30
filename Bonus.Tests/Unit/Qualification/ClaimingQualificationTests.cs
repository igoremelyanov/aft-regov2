using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.DomainServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Qualification
{
    internal class ClaimingQualificationTests : UnitTestBase
    {
        [Test]
        public void Pending_expired_bonus_can_be_activated_during_claim_period()
        {
            var bonus = CreateFirstDepositBonus();
            bonus.DaysToClaim = 1;

            var depositId = SubmitDeposit(PlayerId);
            Assert.AreEqual(ActivationStatus.Pending, BonusRedemptions.Single().ActivationState);
            //expiring the bonus
            bonus.ActiveTo = bonus.ActiveTo.AddDays(-2);
            ApproveDeposit(depositId, PlayerId, 200);

            Assert.AreEqual(ActivationStatus.Activated, BonusRedemptions.Single().ActivationState);
        }

        [Test]
        public void Claiming_bonus_outside_claim_duration_negates_it()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);
            bonus.DaysToClaim = 1;

            MakeDeposit(PlayerId, bonusId: bonus.Id);
            //expiring the bonus
            bonus.ActiveTo = bonus.ActiveTo.AddDays(-3);

            var bonusRedemption = BonusRedemptions.First();
            BonusCommands.ClaimBonusRedemption(new ClaimBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            });

            bonusRedemption
                .ActivationState.Should()
                .Be(ActivationStatus.Negated);
        }

        [Test]
        public void Rest_of_qualification_is_not_processed_during_claim()
        {
            MakeDeposit(PlayerId);

            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);
            bonus.Template.Info.TemplateType = BonusType.ReloadDeposit;

            MakeDeposit(PlayerId, bonusId: bonus.Id);
            BonusCommands.ClaimBonusRedemption(new ClaimBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = BonusRedemptions.First().Id
            });

            MakeDeposit(PlayerId, bonusId: bonus.Id);
            var bonusRedemption = BonusRedemptions.Last();

            bonus.DurationType = DurationType.Custom;
            bonus.DurationStart = SystemTime.Now.AddMinutes(5);
            bonus.DurationEnd = SystemTime.Now.AddMinutes(10);
            bonus.Template.Availability.ParentBonusId = Guid.NewGuid();
            bonus.Template.Availability.VipLevels = new List<BonusVip> { new BonusVip { Code = "Bronze" } };
            bonus.Template.Availability.PlayerRedemptionsLimit = 1;

            BonusCommands.ClaimBonusRedemption(new ClaimBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            });

            bonusRedemption
                .ActivationState.Should()
                .Be(ActivationStatus.Activated);
        }
    }
}