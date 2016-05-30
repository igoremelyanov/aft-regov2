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
    internal class ToggleStatusTests : UnitTestBase
    {
        [Test]
        public void Can_not_change_status_of_non_existent_bonus()
        {
            var result = BonusQueries.GetValidationResult(new ToggleBonusStatus { Id = Guid.Empty, IsActive = true });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.BonusDoesNotExist);
        }

        [TestCase(BonusType.ReferFriend, Description = "Can not activate second refer friends bonus for brand")]
        [TestCase(BonusType.MobilePlusEmailVerification, Description = "Can not activate second verification bonus for brand")]
        public void Only_one_bonus_can_be_active(BonusType bonusType)
        {
            var bonus1 = CreateFirstDepositBonus();
            bonus1.Template.Info.TemplateType = bonusType;

            var bonus2 = CreateFirstDepositBonus(isActive: false);
            bonus2.Template.Info.TemplateType = bonusType;
            bonus2.Template.Info.Brand = bonus1.Template.Info.Brand;

            var result = BonusQueries.GetValidationResult(new ToggleBonusStatus { Id = bonus2.Id, IsActive = true });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.OneBonusOfATypeCanBeActive);
        }
    }
}