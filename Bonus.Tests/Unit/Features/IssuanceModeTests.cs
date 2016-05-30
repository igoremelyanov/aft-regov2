using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Resources;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Tests.Common;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Features
{
    internal class IssuanceModeTests : UnitTestBase
    {
        [Test]
        public void System_activates_Automatic_mode_bonus_redemption()
        {
            CreateFirstDepositBonus();

            MakeDeposit(PlayerId);

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void System_activates_AutomaticWithBonusCode_mode_bonus_redemption()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            MakeDeposit(PlayerId, bonusCode: bonus.Code);

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void Player_should_activate_ManualByPlayer_mode_bonus_redemption()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);

            MakeDeposit(PlayerId, bonusId: bonus.Id);

            var bonusRedemption = BonusRedemptions.First();
            BonusCommands.ClaimBonusRedemption(new ClaimBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            });

            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void System_redeems_Automatic_mode_deposit_bonus()
        {
            var bonus1 = CreateFirstDepositBonus();

            //bonus #2
            CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);
            //bonus #3
            CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            MakeDeposit(PlayerId);

            BonusRedemptions.Count.Should().Be(1);
            BonusRedemptions.First().Bonus.Id.Should().Be(bonus1.Id);
        }

        [Test]
        public void Player_redeems_AutomaticWithBonusCode_deposit_bonus()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            MakeDeposit(PlayerId, bonusCode: bonus.Code);

            BonusRedemptions.Count.Should().Be(1);
            BonusRedemptions.First().Bonus.Id.Should().Be(bonus.Id);
        }

        [Test]
        public void Player_redeems_ManualByPlayer_deposit_bonus()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);
            MakeDeposit(PlayerId, bonusId: bonus.Id);

            BonusRedemptions.Count.Should().Be(1);
            BonusRedemptions.First().Bonus.Id.Should().Be(bonus.Id);
        }

        [Test]
        public void Player_can_not_redeem_Automatic_deposit_bonus()
        {
            var bonus = CreateFirstDepositBonus();

            Action action1 = () => MakeDeposit(PlayerId, bonusCode: bonus.Code);
            action1.ShouldThrow<Exception>().WithMessage(ValidatorMessages.BonusDoesNotExist);

            Action action2 = () => MakeDeposit(PlayerId, bonusId: bonus.Id);
            action2.ShouldThrow<Exception>().WithMessage(ValidatorMessages.BonusDoesNotExist);
        }

        [Test]
        public void System_redeems_Automatic_mode_funin_bonus()
        {
            var bonus1 = CreateFirstDepositBonus();
            bonus1.Template.Info.TemplateType = BonusType.FundIn;
            var brandWalletId = bonus1.Template.Info.Brand.WalletTemplates.First().Id;
            bonus1.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            var bonus2 = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);
            bonus2.Template.Info.TemplateType = BonusType.FundIn;
            bonus2.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            var bonus3 = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus3.Template.Info.TemplateType = BonusType.FundIn;
            bonus3.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            //depositing funds to use them for fund in
            MakeDeposit(PlayerId);
            MakeFundIn(PlayerId, brandWalletId, 200);

            BonusRedemptions.Count.Should().Be(1);
            BonusRedemptions.First().Bonus.Id.Should().Be(bonus1.Id);
        }

        [Test]
        public void Player_redeems_AutomaticWithBonusCode_fundin_bonus()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            var brandWalletId = bonus.Template.Info.Brand.WalletTemplates.First().Id;
            bonus.Template.Info.TemplateType = BonusType.FundIn;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            //depositing funds to use them for fund in
            MakeDeposit(PlayerId);
            MakeFundIn(PlayerId, brandWalletId, 200, bonus.Code);

            BonusRedemptions.Count.Should().Be(1);
            BonusRedemptions.First().Bonus.Id.Should().Be(bonus.Id);
        }

        [Test]
        public void Player_redeems_ManualByPlayer_fundin_bonus()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);
            var brandWalletId = bonus.Template.Info.Brand.WalletTemplates.First().Id;
            bonus.Template.Info.TemplateType = BonusType.FundIn;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            //depositing funds to use them for fund in
            MakeDeposit(PlayerId);
            MakeFundIn(PlayerId, brandWalletId, 200, bonusId: bonus.Id);

            BonusRedemptions.Count.Should().Be(1);
            BonusRedemptions.First().Bonus.Id.Should().Be(bonus.Id);
        }

        [Test]
        public void Player_can_not_redeem_Automatic_fundin_bonus()
        {
            var bonus = CreateFirstDepositBonus();
            var brandWalletId = bonus.Template.Info.Brand.WalletTemplates.First().Id;
            bonus.Template.Info.TemplateType = BonusType.FundIn;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            //depositing funds to use them for fund in
            MakeDeposit(PlayerId);

            Action action1 = () => MakeFundIn(PlayerId, brandWalletId, 200, bonus.Code);
            action1.ShouldThrow<Exception>().WithMessage(ValidatorMessages.BonusDoesNotExist);

            Action action2 = () => MakeFundIn(PlayerId, brandWalletId, 200, bonusId: bonus.Id);
            action2.ShouldThrow<Exception>().WithMessage(ValidatorMessages.BonusDoesNotExist);
        }

        [Test]
        public void Cannot_redeem_bonus_providing_non_existent_bonusCode()
        {
            Action action = () => MakeDeposit(PlayerId, bonusCode: TestDataGenerator.GetRandomString());
            action.ShouldThrow<Exception>().WithMessage(ValidatorMessages.BonusDoesNotExist);
        }

        [Test]
        public void Can_not_redeem_bonus_of_invalid_type_by_code()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Info.TemplateType = BonusType.FundIn;

            Action action = () => MakeDeposit(PlayerId, 200, bonus.Code);
            action.ShouldThrow<Exception>().WithMessage(ValidatorMessages.BonusDoesNotExist);
        }

        [Test]
        public void Can_redeem_bonus_providing_correct_bonusCode()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            MakeDeposit(PlayerId, bonusCode: bonus.Code);

            BonusRedemptions
                .Count(br => br.ActivationState == ActivationStatus.Activated)
                .Should().Be(1);
        }

        [Test]
        public void Can_claim_percentage_reward_type_bonus()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;
            bonus.Template.Rules.RewardTiers.Single().Tiers.Single().Reward = 0.5m;

            MakeDeposit(PlayerId, 1000, bonusId: bonus.Id);

            var bonusRedemption = BonusRedemptions.Single();
            Assert.DoesNotThrow(() => BonusCommands.ClaimBonusRedemption(new ClaimBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            }));
            bonusRedemption.Amount.Should().Be(500);
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void System_does_not_redeem_ManualByCS_bonus_automatically()
        {
            CreateFirstDepositBonus(mode: IssuanceMode.ManualByCs);

            MakeDeposit(PlayerId);

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void System_does_not_redeem_ManualByCS_bonus_by_bonus_code()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByCs);

            Action action = () => MakeDeposit(PlayerId, bonusCode: bonus.Code);
            action.ShouldThrow<Exception>().WithMessage(ValidatorMessages.BonusDoesNotExist);
        }
    }
}