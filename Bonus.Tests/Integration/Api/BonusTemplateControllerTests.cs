using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Api.Interface.Requests;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Shared.ApiDataFiltering;
using AFT.RegoV2.Tests.Common;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Integration.Api
{
    public class BonusTemplateControllerTests : ApiIntegrationTestBase
    {
        [Test]
        public async void Can_get_filtered_templates()
        {
            var template = CreateFirstDepositTemplate();

            var request = new FilteredDataRequest
            {
                PageIndex = 1,
                RowCount = 20,
                SortSord = "asc",
                SortColumn = "Info.Name",
                TopRecords = 20,
                Filters = new[]
                {
                    new Filter
                    {
                        Field = "Info.Name",
                        Comparison = ComparisonOperator.Eq,
                        Data = template.Info.Name
                    }
                }
            };
            var result = await ApiProxy.GetFilteredBonusTemplatesAsync(new BrandFilteredDataRequest
            {
                DataRequest = request,
                BrandFilters = new List<Guid> { DefaultBrandId }
            });

            result.Should().NotBeNull();
            result.Rows.Count.Should().Be(1);
        }

        [Test]
        public async void Can_get_completed_templates()
        {
            var template = CreateFirstDepositTemplate();

            var result = await ApiProxy.GetCompletedTemplatesAsync();

            result.Should().NotBeEmpty();
            result.Any(t => t.Id == template.Id).Should().BeTrue();
        }

        [Test]
        public async void Can_add_bonus_template()
        {
            var walletTemplate = BonusRepository.Brands.Single(b => b.Id == DefaultBrandId).WalletTemplates.Single(wt => wt.IsMain);
            var result = await ApiProxy.CreateUpdateBonusTemplateAsync(new CreateUpdateTemplate
            {
                Id = Guid.Empty,
                Info = new CreateUpdateTemplateInfo
                {
                    Name = TestDataGenerator.GetRandomString(),
                    TemplateType = BonusType.FirstDeposit,
                    BrandId = DefaultBrandId,
                    WalletTemplateId = walletTemplate.Id,
                    Mode = IssuanceMode.AutomaticWithCode
                }
            });

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            result = await ApiProxy.CreateUpdateBonusTemplateAsync(new CreateUpdateTemplate
            {
                Id = result.Id.Value,
                Version = result.Version.Value,
                Availability = new CreateUpdateTemplateAvailability()
            });

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            result = await ApiProxy.CreateUpdateBonusTemplateAsync(new CreateUpdateTemplate
            {
                Id = result.Id.Value,
                Version = result.Version.Value,
                Rules = new CreateUpdateTemplateRules
                {
                    RewardTiers = new List<CreateUpdateRewardTier>
                    {
                        new CreateUpdateRewardTier
                        {
                            CurrencyCode = "CAD",
                            BonusTiers = new List<CreateUpdateTemplateTier>
                            {
                                new CreateUpdateTemplateTier{Reward = 27}
                            }
                        }
                    }
                }
            });

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            result = await ApiProxy.CreateUpdateBonusTemplateAsync(new CreateUpdateTemplate
            {
                Id = result.Id.Value,
                Version = result.Version.Value,
                Wagering = new CreateUpdateTemplateWagering()
            });

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            result = await ApiProxy.CreateUpdateBonusTemplateAsync(new CreateUpdateTemplate
            {
                Id = result.Id.Value,
                Version = result.Version.Value,
                Notification = new CreateUpdateTemplateNotification()
            });

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public async void Can_edit_template()
        {
            var template = CreateFirstDepositTemplate();

            var result = await ApiProxy.CreateUpdateBonusTemplateAsync(new CreateUpdateTemplate
            {
                Id = template.Id,
                Version = 0,
                Info = new CreateUpdateTemplateInfo
                {
                    Name = TestDataGenerator.GetRandomString(),
                    TemplateType = BonusType.FirstDeposit,
                    BrandId = DefaultBrandId,
                    WalletTemplateId = template.Info.WalletTemplateId,
                    Mode = IssuanceMode.AutomaticWithCode
                }
            });

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            result = await ApiProxy.CreateUpdateBonusTemplateAsync(new CreateUpdateTemplate
            {
                Id = result.Id.Value,
                Version = result.Version.Value,
                Availability = new CreateUpdateTemplateAvailability()
            });

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            result = await ApiProxy.CreateUpdateBonusTemplateAsync(new CreateUpdateTemplate
            {
                Id = result.Id.Value,
                Version = result.Version.Value,
                Rules = new CreateUpdateTemplateRules
                {
                    RewardTiers = new List<CreateUpdateRewardTier>
                    {
                        new CreateUpdateRewardTier
                        {
                            CurrencyCode = "CAD",
                            BonusTiers = new List<CreateUpdateTemplateTier>
                            {
                                new CreateUpdateTemplateTier{Reward = 27}
                            }
                        }
                    }
                }
            });

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            result = await ApiProxy.CreateUpdateBonusTemplateAsync(new CreateUpdateTemplate
            {
                Id = result.Id.Value,
                Version = result.Version.Value,
                Wagering = new CreateUpdateTemplateWagering()
            });

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            result = await ApiProxy.CreateUpdateBonusTemplateAsync(new CreateUpdateTemplate
            {
                Id = result.Id.Value,
                Version = result.Version.Value,
                Notification = new CreateUpdateTemplateNotification()
            });

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public async void Can_delete_template()
        {
            var template = CreateFirstDepositTemplate();

            var result = await ApiProxy.DeleteBonusTemplateAsync(new DeleteTemplate { TemplateId = template.Id });
            result.Success.Should().BeTrue();
        }
    }
}
