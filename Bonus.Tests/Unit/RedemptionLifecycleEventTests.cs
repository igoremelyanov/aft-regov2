using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Models.Events.Redemption;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Core.Common.EventStore;
using FluentAssertions;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit
{
    internal class RedemptionLifecycleEventTests: UnitTestBase
    {
        private Core.Data.Bonus _bonus;
        private IEventRepository _eventRepository;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _bonus = CreateFirstDepositBonus();
            _eventRepository = Container.Resolve<IEventRepository>();
            _eventRepository.Events.ToList().ForEach(b => _eventRepository.Events.Remove(b));
        }

        [Test]
        public void BonusRedeemed_is_saved_in_event_store()
        {
            MakeDeposit(PlayerId);

            var bonusRedemption = BonusRedemptions.Single();

            var theEvent = _eventRepository.Events.Single(e => e.DataType == typeof (BonusRedeemed).Name);
            var data = JsonConvert.DeserializeObject<BonusRedeemed>(theEvent.Data);
            data.AggregateId.Should().Be(bonusRedemption.Id);
            data.PlayerId.Should().Be(PlayerId);
            data.BonusId.Should().Be(_bonus.Id);
            data.Amount.Should().Be(bonusRedemption.Amount);
        }

        [Test]
        public void BonusRedeemedByCs_is_saved_in_event_store()
        {
            _bonus.Template.Info.Mode = IssuanceMode.ManualByCs;
            MakeDeposit(PlayerId);
            var transaction = BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();

            BonusCommands.IssueBonusByCs(new IssueBonusByCs
            {
                BonusId = _bonus.Id,
                PlayerId = PlayerId,
                TransactionId = transaction.Id
            });

            var bonusRedemption = BonusRedemptions.Single();

            var theEvent = _eventRepository.Events.Single(e => e.DataType == typeof(BonusRedeemed).Name);
            var data = JsonConvert.DeserializeObject<BonusRedeemed>(theEvent.Data);
            data.AggregateId.Should().Be(bonusRedemption.Id);
            data.PlayerId.Should().Be(PlayerId);
            data.BonusId.Should().Be(_bonus.Id);
            data.Amount.Should().Be(bonusRedemption.Amount);
            data.IssuedByCs.Should().BeTrue();
        }

        [Test]
        public void RedemptionIsClaimable_is_saved_in_event_store()
        {
            MakeDeposit(PlayerId);

            var bonusRedemption = BonusRedemptions.Single();

            var theEvent = _eventRepository.Events.Single(e => e.DataType == typeof(RedemptionIsClaimable).Name);
            var data = JsonConvert.DeserializeObject<RedemptionIsClaimable>(theEvent.Data);
            data.AggregateId.Should().Be(bonusRedemption.Id);
        }

        [Test]
        public void RedemptionClaimed_is_saved_in_event_store()
        {
            MakeDeposit(PlayerId);

            var bonusRedemption = BonusRedemptions.Single();

            var theEvent = _eventRepository.Events.Single(e => e.DataType == typeof(RedemptionClaimed).Name);
            var data = JsonConvert.DeserializeObject<RedemptionClaimed>(theEvent.Data);
            data.AggregateId.Should().Be(bonusRedemption.Id);
            data.Amount.Should().Be(bonusRedemption.Amount);
        }

        [Test]
        public void RedemptionCanceled_is_saved_in_event_store()
        {
            _bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Multiplier = 3m;

            MakeDeposit(PlayerId, 100);

            var bonusRedemption = BonusRedemptions.Single();

            PlaceAndWinBet(20, 40, PlayerId);
            PlaceAndLoseBet(50, PlayerId);
            var model = new CancelBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            };
            BonusCommands.CancelBonusRedemption(model);

            var theEvent = _eventRepository.Events.Single(e => e.DataType == typeof(RedemptionCanceled).Name);
            var data = JsonConvert.DeserializeObject<RedemptionCanceled>(theEvent.Data);
            data.AggregateId.Should().Be(bonusRedemption.Id);
            data.MainBalanceAdjustment.Should().Be(40);
            data.BonusBalanceAdjustment.Should().Be(-40);
            data.NonTransferableAdjustment.Should().Be(-100);
            data.UnlockedAmount.Should().Be(bonusRedemption.LockedAmount);
        }

        [Test]
        public void RedemptionNegated_is_saved_in_event_store_when_qualification_fails()
        {
            var riskLevelId = _bonus.Template.Info.Brand.RiskLevels.Single().Id;
            _bonus.Template.Availability.ExcludeRiskLevels = new List<RiskLevelExclude> { new RiskLevelExclude { ExcludedRiskLevelId = riskLevelId } };

            var depositId = SubmitDeposit(PlayerId);
            TagPlayerWithFraudRiskLevel(PlayerId, riskLevelId);
            ApproveDeposit(depositId, PlayerId, 200);

            var bonusRedemption = BonusRedemptions.Single();

            var theEvent = _eventRepository.Events.Single(e => e.DataType == typeof(RedemptionNegated).Name);
            var data = JsonConvert.DeserializeObject<RedemptionNegated>(theEvent.Data);
            data.AggregateId.Should().Be(bonusRedemption.Id);
            data.Reasons.Single().Should().Be("Player is in excluded risk level");
        }

        [Test]
        public void RedemptionNegated_is_saved_in_event_store_when_deposit_is_unverified()
        {
            var depositId = SubmitDeposit(PlayerId);
            UnverifyDeposit(depositId, PlayerId);

            var bonusRedemption = BonusRedemptions.Single();

            var theEvent = _eventRepository.Events.Single(e => e.DataType == typeof(RedemptionNegated).Name);
            var data = JsonConvert.DeserializeObject<RedemptionNegated>(theEvent.Data);
            data.AggregateId.Should().Be(bonusRedemption.Id);
            data.Reasons.Single().Should().Be("Deposit unverified");
        }

        [Test]
        public void RedemptionRolloverIssued_is_saved_in_event_store()
        {
            _bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Multiplier = 2m;
            MakeDeposit(PlayerId);

            var bonusRedemption = BonusRedemptions.Single();

            var theEvent = _eventRepository.Events.Single(e => e.DataType == typeof(RedemptionRolloverIssued).Name);
            var data = JsonConvert.DeserializeObject<RedemptionRolloverIssued>(theEvent.Data);
            data.AggregateId.Should().Be(bonusRedemption.Id);
            data.WageringRequrement.Should().Be(200);
            data.LockedAmount.Should().Be(300);
        }

        [Test]
        public void RedemptionRolloverDecreased_is_saved_in_event_store()
        {
            _bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Multiplier = 2m;
            MakeDeposit(PlayerId);

            PlaceAndWinBet(50, 50, PlayerId);

            var bonusRedemption = BonusRedemptions.Single();

            var theEvent = _eventRepository.Events.Single(e => e.DataType == typeof(RedemptionRolloverDecreased).Name);
            var data = JsonConvert.DeserializeObject<RedemptionRolloverDecreased>(theEvent.Data);
            data.AggregateId.Should().Be(bonusRedemption.Id);
            data.Decreasement.Should().Be(50);
        }

        [Test]
        public void RedemptionRolloverCompleted_is_saved_in_event_store()
        {
            _bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Multiplier = 2m;
            MakeDeposit(PlayerId);

            PlaceAndWinBet(200, 200, PlayerId);

            var bonusRedemption = BonusRedemptions.Single();

            var theEvent = _eventRepository.Events.Single(e => e.DataType == typeof(RedemptionRolloverCompleted).Name);
            var data = JsonConvert.DeserializeObject<RedemptionRolloverCompleted>(theEvent.Data);
            data.AggregateId.Should().Be(bonusRedemption.Id);
            data.UnlockedAmount.Should().Be(bonusRedemption.LockedAmount);
            data.BonusBalanceAdjustment.Should().Be(-200);
            data.MainBalanceAdjustment.Should().Be(200);
        }

        [Test]
        public void RedemptionRolloverZeroedOut_is_saved_in_event_store()
        {
            _bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 50;
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Multiplier = 20m;
            _bonus.Template.Wagering.Threshold = 150m;
            MakeDeposit(PlayerId);

            PlaceAndWinBet(250, 100, PlayerId);

            var bonusRedemption = BonusRedemptions.Single();

            var theEvent = _eventRepository.Events.Single(e => e.DataType == typeof(RedemptionRolloverZeroedOut).Name);
            var data = JsonConvert.DeserializeObject<RedemptionRolloverZeroedOut>(theEvent.Data);
            data.AggregateId.Should().Be(bonusRedemption.Id);
            data.UnlockedAmount.Should().Be(bonusRedemption.LockedAmount);
            data.BonusBalanceAdjustment.Should().Be(-100);
            data.MainBalanceAdjustment.Should().Be(100);
        }
    }
}
