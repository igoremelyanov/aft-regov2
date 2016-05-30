using System.Linq;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Resources;
using AFT.RegoV2.Bonus.Tests.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Validation
{
    internal class TemplateDeletionTests : UnitTestBase
    {
        [Test]
        public void Can_not_delete_non_existent_template()
        {
            var result = BonusQueries.GetValidationResult(new DeleteTemplate());

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.TemplateDoesNotExist);
        }

        [Test]
        public void Can_not_delete_template_if_there_are_bonuses_using_it()
        {
            var bonus = CreateFirstDepositBonus();
            BonusRepository.Templates.Add(bonus.Template);
            var result = BonusQueries.GetValidationResult(new DeleteTemplate { TemplateId = bonus.Template.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.TemplateIsInUse);
        }
    }
}