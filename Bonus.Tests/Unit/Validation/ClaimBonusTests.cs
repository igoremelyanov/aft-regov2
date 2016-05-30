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
    internal class ClaimTests : UnitTestBase
    {
        [Test]
        public void Player_does_not_exist_validation()
        {
            var validationResult = BonusQueries.GetValidationResult(new ClaimBonusRedemption { PlayerId = Guid.NewGuid(), RedemptionId = Guid.NewGuid() });

            validationResult.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerDoesNotExist);
        }

        [Test]
        public void Can_not_claim_a_non_existent_bonus()
        {
            var validationResult = BonusQueries.GetValidationResult(new ClaimBonusRedemption { PlayerId = PlayerId, RedemptionId = Guid.NewGuid() });

            validationResult.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.BonusRedemptionDoesNotExist);
        }

        [TestCase(ActivationStatus.Pending, ExpectedResult = false)]
        [TestCase(ActivationStatus.Activated, ExpectedResult = false)]
        [TestCase(ActivationStatus.Claimable, ExpectedResult = true)]
        [TestCase(ActivationStatus.Negated, ExpectedResult = false)]
        [TestCase(ActivationStatus.Canceled, ExpectedResult = false)]
        public bool Status_of_bonus_redemption_is_validated_during_cancellation(ActivationStatus status)
        {
            CreateFirstDepositBonus();

            MakeDeposit(PlayerId);
            var bonusRedemption = BonusRedemptions.First();
            bonusRedemption.ActivationState = status;

            var validationResult = BonusQueries.GetValidationResult(new ClaimBonusRedemption
            {
                PlayerId = PlayerId,
                RedemptionId = bonusRedemption.Id
            });

            if(validationResult.IsValid == false)
                validationResult.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.BonusRedemptionStatusIsIncorrectForClaim);

            return validationResult.IsValid;
        }
    }
}