using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.DomainServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using FluentAssertions;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using BonusType = AFT.RegoV2.Bonus.Core.Models.Enums.BonusType;

namespace AFT.RegoV2.Bonus.Tests.Unit.Qualification
{
    internal class ActivationQualificationTests : UnitTestBase
    {
        [Test]
        public void Pending_deposit_bonus_redemption_is_negated_if_deposit_amount_became_unqualified()
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().From = 150;

            var depositId = SubmitDeposit(PlayerId, 200);
            ApproveDeposit(depositId, PlayerId, 100);

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Negated);
        }

        [Test]
        public void Per_player_bonus_issuance_limit_qualification_is_processed_during_activation()
        {
            MakeDeposit(PlayerId);

            var bonus = CreateFirstDepositBonus();
            bonus.Template.Availability.PlayerRedemptionsLimit = 1;
            bonus.Template.Info.TemplateType = BonusType.ReloadDeposit;

            var depositId = SubmitDeposit(PlayerId);
            MakeDeposit(PlayerId);
            ApproveDeposit(depositId, PlayerId, 200);

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Negated);
        }

        [Test]
        public void FraudRiskLevel_qualification_is_processed_during_activation()
        {
            var bonus = CreateFirstDepositBonus();
            var riskLevelId = bonus.Template.Info.Brand.RiskLevels.Single().Id;
            bonus.Template.Availability.ExcludeRiskLevels = new List<RiskLevelExclude> { new RiskLevelExclude { ExcludedRiskLevelId = riskLevelId } };

            var depositId = SubmitDeposit(PlayerId, 200);

            TagPlayerWithFraudRiskLevel(PlayerId, riskLevelId);

            ApproveDeposit(depositId, PlayerId, 100);

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Negated);
        }

        //Has claimed disqualification test in a separate test class

        [Test]
        public void Rest_of_qualification_is_not_processed_during_activation()
        {
            var bonus = CreateFirstDepositBonus();
            var depositId = SubmitDeposit(PlayerId, 100);

            bonus.DurationType = DurationType.Custom;
            bonus.DurationStart = SystemTime.Now.AddMinutes(5);
            bonus.DurationEnd = SystemTime.Now.AddMinutes(10);
            bonus.Template.Availability.ParentBonusId = Guid.NewGuid();
            bonus.Template.Availability.VipLevels = new List<BonusVip> { new BonusVip { Code = "Bronze" } };

            ApproveDeposit(depositId, PlayerId, 100);

            BonusRedemptions.First()
                .ActivationState.Should()
                .Be(ActivationStatus.Activated);
        }

        [Test]
        public void Qualification_runs_before_rollover_is_issued_for_AfterWager_bonus()
        {
            MakeDeposit(PlayerId);

            var excludedReloadedDeposit = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            excludedReloadedDeposit.Template.Info.TemplateType = BonusType.ReloadDeposit;

            var reloadDepositWithExclude = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            reloadDepositWithExclude.Template.Info.TemplateType = BonusType.ReloadDeposit;
            reloadDepositWithExclude.Template.Wagering.HasWagering = true;
            reloadDepositWithExclude.Template.Wagering.IsAfterWager = true;
            reloadDepositWithExclude.Template.Wagering.Multiplier = 1;
            reloadDepositWithExclude.Template.Availability.ExcludeBonuses.Add(new BonusExclude { ExcludedBonusId = excludedReloadedDeposit.Id });
            reloadDepositWithExclude.Template.Availability.ExcludeOperation = Operation.Any;

            var depositId = Guid.NewGuid();
            BonusCommands.ApplyForBonus(new DepositBonusApplication
            {
                PlayerId = PlayerId,
                Amount = 200,
                DepositId = depositId,
                BonusCode = reloadDepositWithExclude.Code
            });

            MakeDeposit(PlayerId, 200, excludedReloadedDeposit.Code);
            ApproveDeposit(depositId, PlayerId, 200);

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Negated);
        }

        [Test]
        public void Qualification_runs_after_rollover_fulfillment_for_AfterWager_bonus()
        {
            var reloadBonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            reloadBonus.Template.Info.TemplateType = BonusType.ReloadDeposit;

            var firstDeposit = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            firstDeposit.Template.Info.TemplateType = BonusType.FirstDeposit;
            firstDeposit.Template.Wagering.HasWagering = true;
            firstDeposit.Template.Wagering.IsAfterWager = true;
            firstDeposit.Template.Wagering.Multiplier = 1;
            firstDeposit.Template.Availability.ExcludeBonuses.Add(new BonusExclude { ExcludedBonusId = reloadBonus.Id });
            firstDeposit.Template.Availability.ExcludeOperation = Operation.Any;

            MakeDeposit(PlayerId, 200, firstDeposit.Code);
            MakeDeposit(PlayerId, 200, reloadBonus.Code);

            var walletQueriesMock = new Mock<IBrandOperations>();
            walletQueriesMock.Setup(queries => queries.GetPlayerBalance(It.IsAny<Guid>(), It.IsAny<string>())).Returns(10m);
            Container.RegisterInstance(walletQueriesMock.Object);
            PlaceAndWinBet(50, 100, PlayerId);

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Negated);
        }
    }
}