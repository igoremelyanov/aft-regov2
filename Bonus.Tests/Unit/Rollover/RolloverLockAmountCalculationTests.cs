using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Core.Common.Events.Player;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Rollover
{
    internal class RolloverLockAmountCalculationTests : UnitTestBase
    {
        [TestCase(true, ExpectedResult = 200, TestName = "Deposit after wager bonus locks deposit only")]
        [TestCase(false, ExpectedResult = 227, TestName = "Deposit before wager bonus locks deposit and bonus")]
        public decimal Deposit_bonus_lock_amount_is_correct(bool isAfterWager)
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.TemplateType = BonusType.FirstDeposit;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.IsAfterWager = isAfterWager;

            MakeDeposit(PlayerId);

            return BonusRedemptions.First().LockedAmount;
        }

        [TestCase(true, ExpectedResult = 100, TestName = "Fund-in after wager bonus locks deposit only")]
        [TestCase(false, ExpectedResult = 127, TestName = "Fund-in before wager bonus locks deposit and bonus")]
        public decimal Fundin_bonus_lock_amount_is_correct(bool isAfterWager)
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.TemplateType = BonusType.FundIn;
            var brandWalletId = bonus.Template.Info.Brand.WalletTemplates.First().Id;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.IsAfterWager = isAfterWager;

            MakeDeposit(PlayerId, 100);
            MakeFundIn(PlayerId, brandWalletId, 100);

            return BonusRedemptions.Single().LockedAmount;
        }

        [TestCase(true, ExpectedResult = 0, TestName = "Referral after wager bonus locks nothing")]
        [TestCase(false, ExpectedResult = 10, TestName = "Referral before wager bonus locks bonus")]
        public decimal Referral_bonus_lock_amount_is_correct(bool isAfterWager)
        {
            var referTier = new BonusTier
            {
                From = 1,
                Reward = 10
            };

            var bonus = CreateBonusWithReferFriendTiers();
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Clear();
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Add(referTier);
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.IsAfterWager = isAfterWager;

            CompleteReferAFriendRequirments(PlayerId);

            return BonusRedemptions.Single().LockedAmount;
        }

        [TestCase(true, ExpectedResult = 0, TestName = "High deposit after wager bonus locks nothing")]
        [TestCase(false, ExpectedResult = 50, TestName = "High deposit before wager bonus locks bonus")]
        public decimal High_deposit_bonus_lock_amount_is_correct(bool isAfterWager)
        {
            var bonus = CreateBonusWithHighDepositTiers(false);
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.IsAfterWager = isAfterWager;

            MakeDeposit(PlayerId, 600);

            return BonusRedemptions.Single().LockedAmount;
        }

        [TestCase(true, ExpectedResult = 0, TestName = "Verification after wager bonus locks nothing")]
        [TestCase(false, ExpectedResult = 27, TestName = "Verification before wager bonus locks bonus")]
        public decimal Verification_bonus_redemption_locks_bonus_only(bool isAfterWager)
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);
            bonus.Template.Info.TemplateType = BonusType.MobilePlusEmailVerification;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.IsAfterWager = isAfterWager;

            var player = CreatePlayer();
            var bonusRedemption = player.Wallets.First().BonusesRedeemed.First();
            VerifyPlayerContact(player.Id, ContactType.Mobile);
            VerifyPlayerContact(player.Id, ContactType.Email);
            if(isAfterWager == false)
                BonusCommands.ClaimBonusRedemption(new ClaimBonusRedemption
                {
                    PlayerId = player.Id,
                    RedemptionId = bonusRedemption.Id
                });

            return bonusRedemption.LockedAmount;
        }
    }
}