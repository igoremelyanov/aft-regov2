using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Features
{
    internal class AfterWagerTests : UnitTestBase
    {
        private Core.Data.Bonus _bonus;
        private Wallet _wallet;

        public override void BeforeEach()
        {
            base.BeforeEach();         

            _bonus = CreateFirstDepositBonus();
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Multiplier = 1;
            _bonus.Template.Wagering.IsAfterWager = true;

            _wallet = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.Single(a => a.Template.Id == _bonus.Template.Info.WalletTemplateId);
        }

        [Test]
        public void Bonus_redemption_is_Pending_with_Active_rollover_after_fulfilling_qualification()
        {
            MakeDeposit(PlayerId);

            var bonusRedemption = BonusRepository.GetLockedPlayer(PlayerId).BonusesRedeemed.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Pending);
            bonusRedemption.RolloverState.Should().Be(RolloverStatus.Active);
        }

        [Test]
        public void Bonus_reward_is_credited_after_wagering_is_completed()
        {
            MakeDeposit(PlayerId);

            _wallet.BonusLock.Should().Be(200);

            PlaceAndLoseBet(54, PlayerId);

            var bonusWallet = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.Single(a => a.Template.Id == _bonus.Template.Info.WalletTemplateId);

            bonusWallet.NonTransferableBonus.Should().Be(27);
            _wallet.BonusLock.Should().Be(0);
        }

        [Test]
        public void ManualByPlayer_bonus_lock_is_released_after_wagering_is_completed()
        {
            _bonus.Template.Info.Mode = IssuanceMode.ManualByPlayer;
            MakeDeposit(PlayerId, bonusId: _bonus.Id);

            _wallet.BonusLock.Should().Be(200);

            PlaceAndLoseBet(54, PlayerId);

            _wallet.BonusLock.Should().Be(0);
        }

        [Test]
        public void Bonus_reward_is_credited_after_wagering_is_zeroed_out()
        {
            _bonus.Template.Wagering.Multiplier = 10;
            _bonus.Template.Wagering.Threshold = 175;
            MakeDeposit(PlayerId);

            _wallet.BonusLock.Should().Be(200);

            PlaceAndLoseBet(27, PlayerId);

            var bonusWallet = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.Single(a => a.Template.Id == _bonus.Template.Info.WalletTemplateId);

            bonusWallet.NonTransferableBonus.Should().Be(27);
            _wallet.BonusLock.Should().Be(0);
        }
    }
}