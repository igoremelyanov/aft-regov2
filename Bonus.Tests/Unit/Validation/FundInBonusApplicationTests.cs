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
    internal class FundInApplicationTests : UnitTestBase
    {
        [Test]
        public void Player_does_not_exist()
        {
            var result = BonusQueries.GetValidationResult(new FundInBonusApplication
            {
                PlayerId = Guid.NewGuid()
            });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerDoesNotExist);
        }

        [Test]
        public void Bonus_does_not_exist_if_BonusId_is_passed()
        {
            var result = BonusQueries.GetValidationResult(new FundInBonusApplication
            {
                PlayerId = PlayerId, 
                BonusId = Guid.NewGuid()
            });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.BonusDoesNotExist);
        }

        [Test]
        public void Bonus_does_not_exist_if_BonusCode_is_passed()
        {
            var result = BonusQueries.GetValidationResult(new FundInBonusApplication
            {
                PlayerId = PlayerId, 
                BonusCode = TestDataGenerator.GetRandomString()
            });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.BonusDoesNotExist);
        }

        [Test]
        public void Player_is_not_qualified()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Info.TemplateType = BonusType.FundIn;
            var result = BonusQueries.GetValidationResult(new FundInBonusApplication
            {
                BonusId = bonus.Id, 
                PlayerId = PlayerId, 
                Amount = 10,
                DestinationWalletTemplateId = Guid.NewGuid()
            });

            result.Errors.Single().ErrorMessage.Should().Be("Fund in wallet is not qualified for this bonus");
        }
    }
}