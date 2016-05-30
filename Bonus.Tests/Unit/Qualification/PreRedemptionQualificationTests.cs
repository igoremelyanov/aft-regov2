using System;
using System.Linq;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Tests.Common;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Qualification
{
    internal class PreRedemptionQualificationTests : UnitTestBase
    {
        [Test]
        public void Can_get_qualified_bonuses_list()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            BonusQueries.GetDepositQualifiedBonuses(PlayerId)
                .ToList()
                .Single()
                .Name
                .Should()
                .Be(bonus.Name);
        }

        [Test]
        public void Qualified_bonuses_list_does_not_contain_expired_bonuses()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.ActiveFrom = DateTimeOffset.MaxValue;

            BonusQueries.GetDepositQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .BeEmpty();
        }

        [Test]
        public void Qualified_bonuses_list_does_not_contain_inactive_bonuses()
        {
            CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode, isActive: false);

            BonusQueries.GetDepositQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .BeEmpty();
        }

        [Test]
        public void Qualified_bonuses_list_contain_current_bonus_version()
        {
            var bonusManagementCommands = Container.Resolve<BonusManagementCommands>();
            var template = CreateFirstDepositTemplate(mode: IssuanceMode.AutomaticWithCode);
            var now = DateTimeOffset.Now.ToBrandOffset(template.Info.Brand.TimezoneId).Date;
            var uiModel1 = new CreateUpdateBonus
            {
                Id = Guid.Empty,
                TemplateId = template.Id,
                Name = TestDataGenerator.GetRandomString(),
                Code = TestDataGenerator.GetRandomString(),
                Description = TestDataGenerator.GetRandomString(),
                ActiveFrom = now,
                ActiveTo = now.AddDays(1)
            };
            var bonusId = bonusManagementCommands.AddUpdateBonus(uiModel1);

            bonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatus
            {
                Id = bonusId,
                IsActive = true
            });
            bonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatus
            {
                Id = bonusId,
                IsActive = false
            });

            var uiModel2 = new CreateUpdateBonus
            {
                Id = bonusId,
                Version = 2,
                TemplateId = template.Id,
                Name = TestDataGenerator.GetRandomString(),
                Code = TestDataGenerator.GetRandomString(),
                Description = TestDataGenerator.GetRandomString(),
                ActiveFrom = now,
                ActiveTo = now.AddDays(1)
            };
            bonusManagementCommands.AddUpdateBonus(uiModel2);
            bonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatus
            {
                Id = bonusId,
                IsActive = true
            });

            BonusQueries
                .GetDepositQualifiedBonuses(PlayerId)
                .Single()
                .Code
                .Should()
                .Be(uiModel2.Code);
        }
    }
}