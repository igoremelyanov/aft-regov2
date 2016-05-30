using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Resources;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Tests.Common;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Validation
{
    internal class DepositApplicationTests : UnitTestBase
    {
        [Test]
        public void Player_does_not_exist()
        {
            var result = BonusQueries.GetValidationResult(new DepositBonusApplication
            {
                PlayerId = Guid.NewGuid(),
                DepositId = Guid.NewGuid()
            });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerDoesNotExist);
        }

        [Test]
        public void Bonus_does_not_exist_if_BonusId_is_passed()
        {
            var result = BonusQueries.GetValidationResult(new DepositBonusApplication
            {
                PlayerId = PlayerId, 
                BonusId = Guid.NewGuid(), 
                DepositId = Guid.NewGuid()
            });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.BonusDoesNotExist);
        }

        [Test]
        public void Bonus_does_not_exist_if_BonusCode_is_passed()
        {
            var result = BonusQueries.GetValidationResult(new DepositBonusApplication
            {
                PlayerId = PlayerId, 
                BonusCode = TestDataGenerator.GetRandomString(),
                DepositId = Guid.NewGuid()
            });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.BonusDoesNotExist);
        }

        [Test]
        public void Player_is_not_qualified()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().From = 20;
            var result = BonusQueries.GetValidationResult(new DepositBonusApplication
            {
                BonusId = bonus.Id, 
                PlayerId = PlayerId, 
                Amount = 10,
                DepositId = Guid.NewGuid()
            });

            result.Errors.Single().ErrorMessage.Should().Be("Deposit amount is not qualified for this bonus");
        }

        [Test]
        public void DepositId_is_empty()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            var result = BonusQueries.GetValidationResult(new DepositBonusApplication
            {
                BonusId = bonus.Id, 
                PlayerId = PlayerId, 
                Amount = 10,
                DepositId = Guid.Empty
            });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.DepositIdIsEmpty);
        }
    }
}