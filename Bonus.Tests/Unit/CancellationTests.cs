using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit
{
    internal class CancellationTests : UnitTestBase
    {
        private Wallet _wallet;
        private Core.Data.Bonus _bonus;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _bonus = CreateFirstDepositBonus();
            _bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Multiplier = 3m;
            _wallet = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.Single(w => w.Template.Id == _bonus.Template.Info.WalletTemplateId);
        }

        [Test]
        public void Bonus_cancellation_after_net_loss()
        {
            MakeDeposit(PlayerId, 100);
            PlaceAndWinBet(20, 40, PlayerId);
            PlaceAndLoseBet(50, PlayerId);
            var model = new CancelBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = BonusRedemptions.First().Id
            };
            BonusCommands.CancelBonusRedemption(model);

            _wallet.Main.Should().Be(70);
            _wallet.Bonus.Should().Be(0);
            _wallet.NonTransferableBonus.Should().Be(0);
            var transaction = _wallet.Transactions.SingleOrDefault(tr => tr.Type == TransactionType.BonusCancelled);
            transaction.Should().NotBeNull();
            transaction.MainBalanceAmount.Should().Be(40);
            transaction.BonusBalanceAmount.Should().Be(-40);
            transaction.NonTransferableAmount.Should().Be(-100);
        }

        [Test]
        public void Bonus_cancellation_after_net_win()
        {
            MakeDeposit(PlayerId, 300);
            PlaceAndWinBet(200, 300, PlayerId);
            PlaceAndLoseBet(50, PlayerId);
            var model = new CancelBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = BonusRedemptions.First().Id
            };
            BonusCommands.CancelBonusRedemption(model);

            _wallet.Main.Should().Be(300);
            _wallet.Bonus.Should().Be(0);
            _wallet.NonTransferableBonus.Should().Be(0);
            var transaction = _wallet.Transactions.SingleOrDefault(tr => tr.Type == TransactionType.BonusCancelled);
            transaction.Should().NotBeNull();
            transaction.MainBalanceAmount.Should().Be(250);
            transaction.BonusBalanceAmount.Should().Be(-300);
            transaction.NonTransferableAmount.Should().Be(-100);
        }

        [Test]
        public void Can_cancel_a_bonus_that_has_no_wagering_contributions()
        {
            MakeDeposit(PlayerId, 300);
            var model = new CancelBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = BonusRedemptions.First().Id
            };
            BonusCommands.CancelBonusRedemption(model);

            _wallet.Main.Should().Be(300);
            _wallet.Bonus.Should().Be(0);
            _wallet.NonTransferableBonus.Should().Be(0);
            var transaction = _wallet.Transactions.SingleOrDefault(tr => tr.Type == TransactionType.BonusCancelled);
            transaction.Should().NotBeNull();
            transaction.MainBalanceAmount.Should().Be(0);
            transaction.BonusBalanceAmount.Should().Be(0);
            transaction.NonTransferableAmount.Should().Be(-100);
        }

        [Test]
        public void Bonus_cancellation_releases_bonus_lock()
        {
            MakeDeposit(PlayerId, 300);
            var model = new CancelBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = BonusRedemptions.First().Id
            };
            BonusCommands.CancelBonusRedemption(model);

            _wallet.Locks.Count(tr => tr.UnlockedOn.HasValue)
                .Should()
                .Be(2, "Unlock of deposit, bonus amount");
        }

        [Test]
        public void Bonus_cancellation_creates_Cancellation_wagering_contribution_record()
        {
            MakeDeposit(PlayerId, 300);

            var bonusRedemption = BonusRedemptions.First();
            var model = new CancelBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            };
            BonusCommands.CancelBonusRedemption(model);

            bonusRedemption.Contributions.SingleOrDefault(c => c.Type == ContributionType.Cancellation)
                .Should()
                .NotBeNull();
        }

        [Test]
        public void Bonus_cancellation_sets_activation_status_to_Cancelled()
        {
            MakeDeposit(PlayerId);
            var bonusRedemption = BonusRedemptions.First();
            var model = new CancelBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            };
            BonusCommands.CancelBonusRedemption(model);

            Assert.AreEqual(ActivationStatus.Canceled, bonusRedemption.ActivationState);
        }

        [Test]
        public void Bonus_cancellation_sets_rollover_status_to_None()
        {
            MakeDeposit(PlayerId);
            var bonusRedemption = BonusRedemptions.First();
            var model = new CancelBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            };
            BonusCommands.CancelBonusRedemption(model);

            Assert.AreEqual(RolloverStatus.None, bonusRedemption.RolloverState);
        }

        [Test]
        public void Bonus_cancellation_discards_applied_bonus_statistics_changes()
        {
            MakeDeposit(PlayerId);

            Assert.AreEqual(100, _bonus.Statistic.TotalRedeemedAmount);
            Assert.AreEqual(1, _bonus.Statistic.TotalRedemptionCount);

            var model = new CancelBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = BonusRedemptions.First().Id
            };
            BonusCommands.CancelBonusRedemption(model);

            Assert.AreEqual(0, _bonus.Statistic.TotalRedeemedAmount);
            Assert.AreEqual(0, _bonus.Statistic.TotalRedemptionCount);
        }
    }
}