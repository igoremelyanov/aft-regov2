using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Rollover
{
    internal class RolloverAmountCalculationTests : UnitTestBase
    {
        [TestCase(WageringMethod.Bonus, ExpectedResult = 54, TestName = "Bonus only rollover is calculated correctly")]
        [TestCase(WageringMethod.TransferAmount, ExpectedResult = 400, TestName = "Deposit only rollover is calculated correctly")]
        [TestCase(WageringMethod.BonusAndTransferAmount, ExpectedResult = 454, TestName = "Deposit+bonus rollover is calculated correctly")]
        public decimal Bonus_rollover_is_calculated_correctly(WageringMethod method)
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.IsWithdrawable = true;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = method;
            bonus.Template.Wagering.Multiplier = 2;

            MakeDeposit(PlayerId);

            return BonusRedemptions.First().Rollover;
        }

        [Test]
        public void Bonus_rollover_is_calculated_based_on_actual_transfer_and_bonus_amounts()
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;
            bonus.Template.Rules.RewardTiers.First().BonusTiers.First().Reward = 0.5m;
            bonus.Template.Info.IsWithdrawable = true;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = WageringMethod.BonusAndTransferAmount;
            bonus.Template.Wagering.Multiplier = 2;

            var depositId = SubmitDeposit(PlayerId, 200);
            ApproveDeposit(depositId, PlayerId, 150);

            BonusRedemptions
                .First()
                .Rollover
                .Should()
                .Be(450, "(150 deposit amount + 75 bonus amount) * 2 Multiplier");
        }

        [Test]
        public void Deposit_ManualByPlayer_bonus_rollover_is_calculated()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = WageringMethod.TransferAmount;
            bonus.Template.Wagering.Multiplier = 2;

            MakeDeposit(PlayerId, bonusId: bonus.Id);

            var bonusRedemption = BonusRedemptions.First();
            BonusCommands.ClaimBonusRedemption(new ClaimBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            });

            bonusRedemption.Rollover.Should().Be(400);
        }

        [Test]
        public void Fundin_ManualByPlayer_bonus_rollover_is_calculated()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);
            var brandWalletId = bonus.Template.Info.Brand.WalletTemplates.First().Id;
            bonus.Template.Info.TemplateType = BonusType.FundIn;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = WageringMethod.TransferAmount;
            bonus.Template.Wagering.Multiplier = 2;

            //depositing funds to use them for fund in
            MakeDeposit(PlayerId);
            MakeFundIn(PlayerId, brandWalletId, 100, bonusId: bonus.Id);

            var bonusRedemption = BonusRedemptions.First();
            BonusCommands.ClaimBonusRedemption(new ClaimBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            });

            bonusRedemption.Rollover.Should().Be(200);
        }
    }
}