using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Features
{
    internal class DisableBonusesForPlayer: UnitTestBase
    {
        [Test]
        public void Can_set_fraudulent_status_for_player()
        {
            var player = BonusRepository.Players.Single(p => p.Id == PlayerId);

            DisableBonusesForPlayer(PlayerId);

            player.IsFraudulent.Should().BeTrue();
        }

        [Test]
        public void Qualification_of_fraudulent_player_for_bonus_fails()
        {
            CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            DisableBonusesForPlayer(PlayerId);

            var bonuses = BonusQueries.GetDepositQualifiedBonuses(PlayerId);
            bonuses.Should().BeEmpty();
        }

        [Test]
        public void Qualification_ignores_fraudulent_status_for_Manual_by_CS_issuance()
        {
            MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = CreateFirstDepositBonus();

            DisableBonusesForPlayer(PlayerId);

            BonusCommands.IssueBonusByCs(new IssueBonusByCs
            {
                BonusId = bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            BonusRedemptions.Single().ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void Fraudulent_player_has_no_claimable_bonuses()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);

            MakeDeposit(PlayerId, bonusId: bonus.Id);

            BonusRedemptions.Count.Should().Be(1);
            DisableBonusesForPlayer(PlayerId);

            BonusQueries.GetClaimableRedemptions(PlayerId).Should().BeEmpty();
        }

        [Test]
        public void Bonus_claim_by_fraudulent_player_negates_it()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.ManualByPlayer);

            MakeDeposit(PlayerId, bonusId: bonus.Id);
            DisableBonusesForPlayer(PlayerId);

            var bonusRedemption = BonusRedemptions.First();
            BonusCommands.ClaimBonusRedemption(new ClaimBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            });

            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Negated);
        }
    }
}