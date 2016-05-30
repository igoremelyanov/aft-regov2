using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Features
{
    internal class ManualByCsIssuanceTests : UnitTestBase
    {
        [Test]
        public void Can_issue_first_deposit_bonus()
        {
            MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = CreateFirstDepositBonus();

            BonusCommands.IssueBonusByCs(new IssueBonusByCs
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void Can_issue_reload_deposit_bonus()
        {
            MakeDeposit(PlayerId);
            MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).Last();
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Info.TemplateType = BonusType.ReloadDeposit;

            BonusCommands.IssueBonusByCs(new IssueBonusByCs
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void Can_issue_fundin_bonus()
        {
            var brandWalletId = BonusRepository.Brands.First().WalletTemplates.First().Id;
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByCs);
            bonus.Template.Info.TemplateType = BonusType.FundIn;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            MakeDeposit(PlayerId);
            MakeFundIn(PlayerId, brandWalletId, 100);

            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).Single(t => t.Type == TransactionType.FundIn);

            BonusCommands.IssueBonusByCs(new IssueBonusByCs
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void System_does_not_claim_ManualByPlayer_bonus_issued_by_CS()
        {
            MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);

            BonusCommands.IssueBonusByCs(new IssueBonusByCs
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Claimable);
        }

        [Test]
        public void System_claims_ManualByCs_bonus()
        {
            MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByCs);

            BonusCommands.IssueBonusByCs(new IssueBonusByCs
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void Before_wager_bonus_isnot_issued_before_wagering_requirement_is_fulfilled()
        {
            MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByCs);
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Multiplier = 2;
            bonus.Template.Wagering.IsAfterWager = true;

            BonusCommands.IssueBonusByCs(new IssueBonusByCs
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Pending);
            bonusRedemption.RolloverState.Should().Be(RolloverStatus.Active);
        }

        [Test]
        public void Bonus_issued_by_Cs_ignores_bonus_claim_duration_qualification()
        {
            MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);
            bonus.DaysToClaim = 1;

            BonusCommands.IssueBonusByCs(new IssueBonusByCs
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();

            //expiring the bonus
            bonus.ActiveTo = bonus.ActiveTo.AddDays(-2);
            BonusCommands.ClaimBonusRedemption(new ClaimBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            });

            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }
    }
}