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
    internal class RolloverLockUnlockTests : UnitTestBase
    {
        private Wallet _mainWallet;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _mainWallet = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.Single(a => a.Template.IsMain);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Bonus_without_rollover_is_not_locked_on_activation(bool isWithdrawable)
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.IsWithdrawable = isWithdrawable;
            bonus.Template.Wagering.HasWagering = false;

            MakeDeposit(PlayerId);

            _mainWallet.BonusLock.Should().Be(0);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Bonus_with_rollover_is_locked_on_activation(bool isWithdrawable)
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.IsWithdrawable = isWithdrawable;
            bonus.Template.Wagering.HasWagering = true;

            MakeDeposit(PlayerId);

            _mainWallet.BonusLock.Should().Be(227);
        }

        [Test]
        public void Lock_is_issued_to_correct_wallets()
        {
            var recievingWallet = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.Single(a => a.Template.IsMain == false);
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Info.WalletTemplateId = recievingWallet.Template.Id;

            MakeDeposit(PlayerId);

            _mainWallet.BonusLock.Should().Be(200);
            recievingWallet.BonusLock.Should().Be(27);
        }

        [Test]
        public void Lock_is_released_from_correct_wallets_when_rollover_is_finished()
        {
            var productWallet = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.Single(a => a.Template.IsMain == false);

            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.TemplateType = BonusType.FundIn;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = productWallet.Template.Id}
            };
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Multiplier = 0.1m;
            bonus.Template.Info.WalletTemplateId = _mainWallet.Template.Id;

            MakeDeposit(PlayerId, 100);
            MakeFundIn(PlayerId, productWallet.Template.Id, 100);
            PlaceAndLoseBet(27, PlayerId);

            _mainWallet.BonusLock.Should().Be(0);
            _mainWallet.Locks
                .Single(tr => tr.Amount == 27)
                .UnlockedOn
                .Should()
                .HaveValue();
            productWallet.BonusLock.Should().Be(0);
            productWallet.Locks
                .Single(tr => tr.Amount == 100)
                .UnlockedOn
                .Should()
                .HaveValue();
        }

        [Test]
        public void Lock_is_released_from_correct_wallets_when_bonus_is_canceled()
        {
            var player = BonusRepository.Players.Single(p => p.Id == PlayerId);
            var productWallet = player.Wallets.Single(a => a.Template.IsMain == false);
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.TemplateType = BonusType.FundIn;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = productWallet.Template.Id}
            };
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Multiplier = 0.1m;
            bonus.Template.Info.WalletTemplateId = _mainWallet.Template.Id;

            MakeDeposit(PlayerId, 100);
            MakeFundIn(PlayerId, productWallet.Template.Id, 100);
            var model = new CancelBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = BonusRedemptions.Single().Id
            };
            BonusCommands.CancelBonusRedemption(model);

            _mainWallet.BonusLock.Should().Be(0);
            _mainWallet.Locks
                .Single(tr => tr.Amount == 27)
                .UnlockedOn
                .Should()
                .HaveValue();
            productWallet.BonusLock.Should().Be(0);
            productWallet.Locks
                .Single(tr => tr.Amount == 100)
                .UnlockedOn
                .Should()
                .HaveValue();
        }
    }
}