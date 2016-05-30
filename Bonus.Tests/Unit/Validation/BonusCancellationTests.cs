using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Resources;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Validation
{
    internal class CancellationTests : UnitTestBase
    {
        [Test]
        public void Player_does_not_exist_validation()
        {
            var validationResult = BonusQueries.GetValidationResult(new CancelBonusRedemption { PlayerId = Guid.NewGuid(), RedemptionId = Guid.NewGuid() });

            validationResult.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerDoesNotExist);
        }

        [Test]
        public void Can_not_cancel_a_non_existent_bonus()
        {
            var validationResult = BonusQueries.GetValidationResult(new CancelBonusRedemption { PlayerId = PlayerId, RedemptionId = Guid.NewGuid()});

            validationResult.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.BonusRedemptionDoesNotExist);
        }

        [TestCase(RolloverStatus.Active, ExpectedResult = true)]
        [TestCase(RolloverStatus.Completed, ExpectedResult = false)]
        [TestCase(RolloverStatus.None, ExpectedResult = false)]
        [TestCase(RolloverStatus.ZeroedOut, ExpectedResult = false)]
        public bool Status_of_bonus_redemption_is_validated_during_cancellation(RolloverStatus status)
        {
            var bonus = CreateFirstDepositBonus();
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Multiplier = 3m;

            MakeDeposit(PlayerId);
            var bonusRedemption = BonusRedemptions.First();
            bonusRedemption.RolloverState = status;

            var validationResult = BonusQueries.GetValidationResult(new CancelBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            });

            if(validationResult.IsValid == false)
                validationResult.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.BonusRedemptionStatusIsIncorrectForCancellation);

            return validationResult.IsValid;
        }
    }
}