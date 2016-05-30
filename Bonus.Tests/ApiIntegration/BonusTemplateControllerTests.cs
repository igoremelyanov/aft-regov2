using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Interface.Data;
using AFT.RegoV2.Bonus.Core.Interface.Data.Commands;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Shared.ApiDataFiltering;
using AFT.RegoV2.Tests.Common;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.ApiIntegration
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
            var result = await ApiProxy.GetFilteredBonusTemplatesAsync(request);

            result.Should().NotBeNull();
            result.Rows.Count.Should().Be(1);
        }

        [Test]
        public async void Can_get_template_add_data()
        {
            var result = await ApiProxy.GetBonusTemplateRelatedDataAsync();

            result.Should().NotBeNull();
            result.Template.Should().BeNull();
            result.Bonuses.Should().NotBeNull("Can not check for empty because it can be actually empty.");
        }

        [Test]
        public async void Can_get_template_edit_and_view_data()
        {
            var template = CreateFirstDepositTemplate();

            var result = await ApiProxy.GetBonusTemplateRelatedDataAsync(template.Id);

            result.Should().NotBeNull();
            result.Template.Should().NotBeNull();
            result.Bonuses.Should().NotBeNull("Can not check for empty because it can be actually empty.");
        }

        [Test]
        public void Cannot_get_template_edit_and_view_data_with_invalid_brand()
        {
            LogInApi(Guid.Empty);
            var template = CreateFirstDepositTemplate();

            AssertActionIsForbidden(() => ApiProxy.GetBonusTemplateRelatedDataAsync(template.Id));
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
        public void Cannot_add_bonus_template_with_invalid_brand()
        {
            LogInApi(Guid.Empty);
            var walletTemplate = BonusRepository.Brands.Single(b => b.Id == DefaultBrandId).WalletTemplates.Single(wt => wt.IsMain);
            AssertActionIsForbidden(() => ApiProxy.CreateUpdateBonusTemplateAsync(new CreateUpdateTemplate
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
            }));
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
        public void Cannot_edit_template_with_invalid_brand()
        {
            LogInApi(Guid.Empty);
            var template = CreateFirstDepositTemplate();

            AssertActionIsForbidden(() => ApiProxy.CreateUpdateBonusTemplateAsync(new CreateUpdateTemplate
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
            }));
        }

        [Test]
        public async void Can_delete_template()
        {
            var template = CreateFirstDepositTemplate();

            var result = await ApiProxy.DeleteBonusTemplateAsync(new DeleteTemplate { TemplateId = template.Id });
            result.Success.Should().BeTrue();
        }

        [Test]
        public void Cannot_delete_template_with_invalid_brand()
        {
            LogInApi(Guid.Empty);
            var template = CreateFirstDepositTemplate();

            AssertActionIsForbidden(() => ApiProxy.DeleteBonusTemplateAsync(new DeleteTemplate { TemplateId = template.Id }));
        }
    }
}
