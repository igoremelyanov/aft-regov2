using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Api.Interface.Requests;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Shared.ApiDataFiltering;
using AFT.RegoV2.Tests.Common;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Integration.Api
{
    public class BonusControllerTests : ApiIntegrationTestBase
    {
        [Test]
        public async void Can_get_filtered_bonuses()
        {
            var bonus = CreateFirstDepositBonus(isActive: false);

            var request = new FilteredDataRequest
            {
                PageIndex = 1,
                RowCount = 20,
                SortSord = "asc",
                SortColumn = "Name",
                TopRecords = 20,
                Filters = new[]
                {
                    new Filter
                    {
                        Field = "Name",
                        Comparison = ComparisonOperator.Eq,
                        Data = bonus.Name
                    }
                }
            };
            var result = await ApiProxy.GetFilteredBonusesAsync(new BrandFilteredDataRequest
            {
                DataRequest = request,
                BrandFilters = new List<Guid> { DefaultBrandId }
            });

            result.Should().NotBeNull();
            result.Rows.Count.Should().Be(1);
        }

        [Test]
        public async void Can_get_bonus()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            var result = await ApiProxy.GetBonusOrNull(bonus.Id);

            result.Should().NotBeNull();

            result = await ApiProxy.GetBonusOrNull(Guid.NewGuid());

            result.Should().BeNull();
        }

        [Test]
        public async void Can_get_bonuses()
        {
            var bonus = CreateFirstDepositBonus(mode: IssuanceMode.AutomaticWithCode);

            var result = await ApiProxy.GetBonusesAsync();

            result.Should().NotBeEmpty();
            result.Any(t => t.Id == bonus.Id).Should().BeTrue();
        }

        [Test]
        public async void Can_add_bonus()
        {
            var template = CreateFirstDepositTemplate();
            var now = DateTimeOffset.Now.ToBrandOffset(template.Info.Brand.TimezoneId).Date;
            var result = await ApiProxy.CreateUpdateBonusAsync(new CreateUpdateBonus
            {
                Id = Guid.Empty,
                TemplateId = template.Id,
                Name = TestDataGenerator.GetRandomString(),
                Description = TestDataGenerator.GetRandomString(),
                ActiveFrom = now,
                ActiveTo = now.AddDays(1),
                DaysToClaim = 1
            });

            result.Success.Should().BeTrue();
            result.BonusId.Should().HaveValue();
        }

        [Test]
        public async void Can_edit_bonus()
        {
            var bonus = CreateFirstDepositBonus(isActive: false);
            var now = DateTimeOffset.Now.ToBrandOffset(bonus.Template.Info.Brand.TimezoneId).Date;
            var result = await ApiProxy.CreateUpdateBonusAsync(new CreateUpdateBonus
            {
                Id = bonus.Id,
                TemplateId = bonus.Template.Id,
                Name = TestDataGenerator.GetRandomString(),
                Description = TestDataGenerator.GetRandomString(),
                ActiveFrom = now,
                ActiveTo = now.AddDays(1),
                DaysToClaim = 1
            });

            result.Success.Should().BeTrue();
            result.BonusId.Should().HaveValue();
        }

        [Test]
        public async void Can_toggle_bonus_status()
        {
            var bonus = CreateFirstDepositBonus(isActive: false);

            var result = await ApiProxy.ChangeBonusStatusAsync(new ToggleBonusStatus { Id = bonus.Id, IsActive = true, Remarks = TestDataGenerator.GetRandomString() });
            result.Success.Should().BeTrue();

            result = await ApiProxy.ChangeBonusStatusAsync(new ToggleBonusStatus { Id = bonus.Id, IsActive = false, Remarks = TestDataGenerator.GetRandomString() });
            result.Success.Should().BeTrue();
        }
    }
}
