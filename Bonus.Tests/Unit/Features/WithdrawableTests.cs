using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Features
{
    internal class WithdrawableTests : UnitTestBase
    {
        private Core.Data.Wallet _bonusWallet;
        private Core.Data.Bonus _bonus;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _bonus = CreateFirstDepositBonus();
            _bonusWallet = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.Single(a => a.Template.Id == _bonus.Template.Info.WalletTemplateId);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NotWithdrawable_bonus_is_credited_to_NonTransferableBonus(bool hasRollover)
        {
            _bonus.Template.Info.IsWithdrawable = false;
            _bonus.Template.Wagering.HasWagering = hasRollover;

            MakeDeposit(PlayerId);

            _bonusWallet.Main.Should().Be(200);
            _bonusWallet.Bonus.Should().Be(0);
            _bonusWallet.NonTransferableBonus.Should().Be(27);
        }

        [Test]
        public void Withdrawable_bonus_without_rollover_is_credited_to_main_balance()
        {
            _bonus.Template.Info.IsWithdrawable = true;
            _bonus.Template.Wagering.HasWagering = false;

            MakeDeposit(PlayerId);

            _bonusWallet.Main.Should().Be(227);
        }

        [Test]
        public void Withdrawable_bonus_with_rollover_is_credited_to_bonus_balance()
        {
            _bonus.Template.Info.IsWithdrawable = true;
            _bonus.Template.Wagering.HasWagering = true;

            MakeDeposit(PlayerId);

            _bonusWallet.Bonus.Should().Be(27);
        }

        [Test]
        public void NotWithrawable_funds_arent_transferred_to_main_once_wagering_is_completed()
        {
            _bonus.Template.Rules.RewardTiers.First().BonusTiers.First().Reward = 100;
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Method = WageringMethod.Bonus;
            _bonus.Template.Wagering.Multiplier = 2.75m;
            MakeDeposit(PlayerId);

            PlaceAndLoseBet(275, PlayerId);

            _bonusWallet.Main.Should().Be(0m);
            _bonusWallet.Bonus.Should().Be(0m);
            _bonusWallet.NonTransferableBonus.Should().Be(25m);
        }

        [Test]
        public void NotWithrawable_bonus_cancellation_decreases_NonTransferableBonus()
        {
            _bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            _bonus.Template.Info.IsWithdrawable = false;
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Multiplier = 3m;

            MakeDeposit(PlayerId, 100);

            PlaceAndWinBet(20, 40, PlayerId);
            PlaceAndLoseBet(50, PlayerId);
            var model = new CancelBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = BonusRedemptions.First().Id
            };
            BonusCommands.CancelBonusRedemption(model);

            _bonusWallet.NonTransferableBonus.Should().Be(0);
        }
    }
}