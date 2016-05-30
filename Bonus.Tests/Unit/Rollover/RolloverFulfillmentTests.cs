using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Core.Game.Interface.Events;
using FluentAssertions;
using NUnit.Framework;
using BonusRedemption = AFT.RegoV2.Bonus.Core.Data.BonusRedemption;
using GameContribution = AFT.RegoV2.Bonus.Core.Data.GameContribution;

namespace AFT.RegoV2.Bonus.Tests.Unit.Rollover
{
    internal class RolloverFulfillmentTests : UnitTestBase
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

            _wallet = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.Single(a => a.Template.Id == _bonus.Template.Info.WalletTemplateId);
        }

        [Test]
        public void Bet_won_increases_wagering_completed()
        {
            MakeDeposit(PlayerId);
            PlaceAndWinBet(27, 54, PlayerId);

            BonusRedemptions.First().Contributions.SingleOrDefault(c => c.Contribution == 27).Should().NotBeNull();
        }

        [Test]
        public void Bet_lost_increases_wagering_completed()
        {
            MakeDeposit(PlayerId);
            PlaceAndLoseBet(27, PlayerId);

            BonusRedemptions.First().Contributions.SingleOrDefault(c => c.Contribution == 27).Should().NotBeNull();
        }

        [Test]
        public void Bet_tied_increases_wagering_completed()
        {
            MakeDeposit(PlayerId);
            var gameId = Guid.NewGuid();
            var roundId = Guid.NewGuid();
            ServiceBus.PublishMessage(new BetPlaced
            {
                PlayerId = PlayerId,
                Amount = 27,
                GameId = gameId,
                RoundId = roundId
            });
            ServiceBus.PublishMessage(new BetTied
            {
                PlayerId = PlayerId,
                Amount = 27,
                GameId = gameId,
                RoundId = roundId,
                Turnover = 27
            });

            BonusRedemptions.First().Contributions.SingleOrDefault(c => c.Contribution == 27).Should().NotBeNull();
        }

        [Test]
        public void FreeBet_increases_wagering_completed()
        {
            MakeDeposit(PlayerId);
            var gameId = Guid.NewGuid();
            var roundId = Guid.NewGuid();
            ServiceBus.PublishMessage(new BetPlacedFree
            {
                PlayerId = PlayerId,
                Amount = 27,
                GameId = gameId,
                RoundId = roundId,
                Turnover = 27
            });

            BonusRedemptions.First().Contributions.SingleOrDefault(c => c.Contribution == 27).Should().NotBeNull();
        }

        [Test]
        public void Placing_a_bet_does_not_change_wagering_completed()
        {
            MakeDeposit(PlayerId);
            PlaceBet(27, PlayerId, Guid.NewGuid(), Guid.NewGuid());

            BonusRedemptions.First().Contributions.Should().BeEmpty();
        }

        [Test]
        public void Meeting_wagering_threshold_completes_wagering()
        {
            _bonus.Template.Wagering.Threshold = 200;
            MakeDeposit(PlayerId);
            PlaceAndLoseBet(100, PlayerId);

            var bonusRedemption = BonusRedemptions.First();

            bonusRedemption.RolloverState.Should().Be(RolloverStatus.ZeroedOut);
            var thresholdContribution = bonusRedemption.Contributions.ElementAt(1);
            thresholdContribution.Type.Should().Be(ContributionType.Threshold);
            thresholdContribution.Contribution.Should().Be(200);
        }

        [Test]
        public void Playable_balance_is_taken_into_account_to_trigger_wagering_threshold()
        {
            _bonus.Template.Wagering.Threshold = 125;
            MakeDeposit(PlayerId);

            PlaceAndLoseBet(175, PlayerId);
            BonusRedemptions.Single().RolloverState.Should().Be(RolloverStatus.ZeroedOut);
        }

        [Test]
        public void Bet_outcome_decreases_RolloverLeft_of_bonus_redemptions_one_by_one()
        {
            var bonus2 = CreateFirstDepositBonus();
            bonus2.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            bonus2.Template.Wagering.HasWagering = true;
            bonus2.Template.Wagering.Multiplier = 3m;

            MakeDeposit(PlayerId, 800);

            var redemption1 = BonusRedemptions.First();
            var redemption2 = BonusRedemptions.Last();
            PlaceAndLoseBet(500, PlayerId);

            redemption1.RolloverState.Should().Be(RolloverStatus.Completed);
            redemption1.Contributions.SingleOrDefault(c => c.Type == ContributionType.Bet).Should().NotBeNull();
            redemption2.RolloverState.Should().Be(RolloverStatus.Active);
            redemption2.Contributions.SingleOrDefault(c => c.Contribution == 200).Should().NotBeNull();
        }

        [Test]
        public void Bet_outcome_decreases_RolloverLeft_of_bonus_redemption_that_was_created_first()
        {
            var bonus2 = CreateFirstDepositBonus();
            bonus2.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            bonus2.Template.Wagering.HasWagering = true;
            bonus2.Template.Wagering.Multiplier = 3m;

            MakeDeposit(PlayerId, 400);

            var redemption1 = BonusRedemptions.First();
            var redemption2 = BonusRedemptions.Last();
            BonusRepository.GetLockedPlayer(PlayerId).Data.Wallets.First().BonusesRedeemed = new List<BonusRedemption> { redemption2, redemption1 };
            PlaceAndLoseBet(50, PlayerId);

            redemption1.Contributions.SingleOrDefault(c => c.Type == ContributionType.Bet).Should().NotBeNull();
            redemption2.Contributions.Should().BeEmpty();
        }

        [Test]
        public void Bet_outcome_decreases_RolloverLeft_of_BR_fulfillment_of_which_started()
        {
            var bonus2 = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus2.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            bonus2.Template.Wagering.HasWagering = true;
            bonus2.Template.Wagering.Multiplier = 3m;

            var depositId = SubmitDeposit(PlayerId);
            MakeDeposit(PlayerId, 400, bonus2.Code);
            //starting to fulfill rollover of bonus redemption tied to bonus2
            PlaceAndLoseBet(54, PlayerId);

            ApproveDeposit(depositId, PlayerId, 200);

            PlaceAndLoseBet(54, PlayerId);

            BonusRedemptions.ElementAt(0).Contributions.Should().BeEmpty();
            BonusRedemptions.ElementAt(1).Contributions.Count(c => c.Type == ContributionType.Bet).Should().Be(2);
        }

        [Test]
        public void Bet_action_cancellation_increases_RolloverLeft()
        {
            MakeDeposit(PlayerId);

            var gameId = Guid.NewGuid();
            var roundId = Guid.NewGuid();
            var gameActionId = Guid.NewGuid();
            ServiceBus.PublishMessage(new BetPlaced
            {
                PlayerId = PlayerId,
                Amount = 27,
                GameId = gameId,
                RoundId = roundId
            });
            ServiceBus.PublishMessage(new BetWon
            {
                PlayerId = PlayerId,
                Amount = 200,
                Turnover = 27,
                RoundId = roundId,
                GameId = gameId,
                GameActionId = gameActionId
            });

            var bonusRedemption = BonusRedemptions.First();
            bonusRedemption.Contributions.SingleOrDefault(c => c.Contribution == 27).Should().NotBeNull();

            ServiceBus.PublishMessage(new BetCancelled
            {
                PlayerId = PlayerId,
                Turnover = -27,
                RoundId = roundId,
                GameId = gameId,
                RelatedGameActionId = gameActionId
            });

            bonusRedemption.Contributions.Last(c => c.Contribution == -27).Should().NotBeNull();
        }

        [Test]
        public void Bet_action_cancellation_doesnt_increase_RolloverLeft_if_rollover_isnt_Active()
        {
            MakeDeposit(PlayerId);

            var gameId = Guid.NewGuid();
            var roundId = Guid.NewGuid();
            var gameActionId = Guid.NewGuid();
            ServiceBus.PublishMessage(new BetPlaced
            {
                PlayerId = PlayerId,
                Amount = 300,
                GameId = gameId,
                RoundId = roundId
            });
            ServiceBus.PublishMessage(new BetWon
            {
                PlayerId = PlayerId,
                Amount = 200,
                Turnover = 300,
                RoundId = roundId,
                GameId = gameId,
                GameActionId = gameActionId
            });

            var bonusRedemption = BonusRedemptions.First();
            bonusRedemption.Contributions.SingleOrDefault(c => c.Contribution == 300).Should().NotBeNull();
            bonusRedemption.RolloverState.Should().Be(RolloverStatus.Completed);

            ServiceBus.PublishMessage(new BetCancelled
            {
                PlayerId = PlayerId,
                Turnover = -500,
                RoundId = roundId,
                GameId = gameId,
                RelatedGameActionId = gameActionId
            });

            bonusRedemption.Contributions.Count.Should().Be(1);
        }

        [Test]
        public void All_contribution_are_calculated_as_100_percents_when_no_game_to_rollover_contributions_defined()
        {
            MakeDeposit(PlayerId);
            PlaceAndLoseBet(27, PlayerId);

            BonusRedemptions.First().Contributions.SingleOrDefault(c => c.Contribution == 27).Should().NotBeNull();
        }

        [Test]
        public void Contribution_is_calculated_using_multiplier_when_game_to_rollover_contributions_are_defined()
        {
            var gameId = Guid.NewGuid();
            _bonus.Template.Wagering.GameContributions.Add(new GameContribution
            {
                Contribution = 0.5m,
                GameId = gameId
            });
            MakeDeposit(PlayerId);

            var roundId = Guid.NewGuid();
            PlaceBet(27, PlayerId, gameId, roundId);
            LoseBet(PlayerId, roundId, 27);

            BonusRedemptions.First().Contributions.SingleOrDefault(c => c.Contribution == 13.5m).Should().NotBeNull();
        }

        [Test]
        public void Contribution_is_calculated_as_100_percents_for_game_that_is_not_on_GameContributions_list()
        {
            _bonus.Template.Wagering.GameContributions.Add(new GameContribution
            {
                Contribution = 0.5m,
                GameId = Guid.NewGuid()
            });
            MakeDeposit(PlayerId);
            PlaceAndLoseBet(27, PlayerId);

            BonusRedemptions.First().Contributions.SingleOrDefault(c => c.Contribution == 27).Should().NotBeNull();
        }

        [Test]
        public void Winnings_from_bonus_are_transferred_to_main_once_wagering_is_completed()
        {
            _bonus.Template.Info.Mode = IssuanceMode.ManualByPlayer;

            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Rules.RewardTiers.First().BonusTiers.First().Reward = 100;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = WageringMethod.Bonus;
            bonus.Template.Wagering.Multiplier = 2.75m;
            MakeDeposit(PlayerId, 200, bonus.Code);

            PlaceAndLoseBet(250, PlayerId);
            PlaceAndWinBet(27, 1000, PlayerId);

            _wallet.Main.Should().Be(1000m);
            _wallet.Bonus.Should().Be(0m);
            _wallet.NonTransferableBonus.Should().Be(23m);
        }

        [Test]
        public void Winnings_from_bonus_funds_and_bonus_are_transferred_to_main_once_wagering_is_completed()
        {
            _bonus.Template.Info.Mode = IssuanceMode.ManualByPlayer;

            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Rules.RewardTiers.First().BonusTiers.First().Reward = 100;
            bonus.Template.Info.IsWithdrawable = true;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = WageringMethod.Bonus;
            bonus.Template.Wagering.Multiplier = 2.75m;
            MakeDeposit(PlayerId, 200, bonus.Code);

            PlaceAndLoseBet(250, PlayerId);
            PlaceAndWinBet(27, 1000, PlayerId);

            _wallet.Main.Should().Be(1011.5m);
            _wallet.Bonus.Should().Be(11.5m);
            _wallet.NonTransferableBonus.Should().Be(0m);
        }

        [Test]
        public void Bonus_redemption_without_rollover_has_None_rollover_status_after_activation()
        {
            _bonus.Template.Wagering.HasWagering = false;

            MakeDeposit(PlayerId);

            Assert.AreEqual(RolloverStatus.None, BonusRedemptions.First().RolloverState);
        }

        [Test]
        public void Bonus_redemption_with_rollover_has_Active_rollover_status_after_activation()
        {
            MakeDeposit(PlayerId);

            Assert.AreEqual(RolloverStatus.Active, BonusRedemptions.First().RolloverState);
        }

        [Test]
        public void Wagering_completed_sets_rollover_status_to_Completed()
        {
            _bonus.IsActive = false;
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Info.IsWithdrawable = true;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = WageringMethod.Bonus;
            bonus.Template.Wagering.Multiplier = 1;

            MakeDeposit(PlayerId, bonusCode: bonus.Code);
            PlaceAndLoseBet(27, PlayerId);

            Assert.AreEqual(RolloverStatus.Completed, BonusRedemptions.First(br => br.Rollover == 27).RolloverState);
        }
    }
}