using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Utils;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class BonusTestHelper
    {
        private readonly IBonusApiProxy _bonusApiProxy;
        private readonly BrandQueries _brandQueries;

        public BonusTestHelper(IBonusApiProxy bonusApiProxy, BrandQueries brandQueries)
        {
            _bonusApiProxy = bonusApiProxy;
            _brandQueries = brandQueries;
        }

        public async Task<Bonus.Core.Models.Data.Bonus> CreateBonus(Template bonusTemplate, bool isActive = true)
        {
            var brand = _brandQueries.GetBrand(bonusTemplate.Info.BrandId);
            var now = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId).Date;
            var model = new CreateUpdateBonus
            {
                Id = Guid.Empty,
                Name = TestDataGenerator.GetRandomString(5),
                Code = TestDataGenerator.GetRandomString(5),
                TemplateId = bonusTemplate.Id,
                ActiveFrom = now,
                ActiveTo = now.AddDays(1),
                DurationStart = now,
                DurationEnd = now.AddDays(1)
            };
            var response = await _bonusApiProxy.CreateUpdateBonusAsync(model);
            if (response.Success == false)
                throw new Exception(response.Errors.First().ErrorMessage);

            if (isActive)
            {
                await _bonusApiProxy.ChangeBonusStatusAsync(new ToggleBonusStatus
                {
                    Id = response.BonusId.Value,
                    IsActive = true
                });
            }

            return await _bonusApiProxy.GetBonusOrNull(response.BonusId.Value);
        }

        public async Task<Template> CreateFirstDepositTemplateAsync(string brandName, IssuanceMode mode = IssuanceMode.Automatic)
        {
            var brand = _brandQueries.GetBrands().Single(b => b.Name == brandName);
            var model = new CreateUpdateTemplate
            {
                Id = Guid.Empty,
                Info = new CreateUpdateTemplateInfo
                {
                    Name = TestDataGenerator.GetRandomString(),
                    TemplateType = BonusType.FirstDeposit,
                    BrandId = brand.Id,
                    WalletTemplateId = brand.WalletTemplates.First().Id,
                    Mode = mode
                }
            };
            var response = await _bonusApiProxy.CreateUpdateBonusTemplateAsync(model);
            if (response.Success == false)
                throw new Exception(response.Errors.First().ErrorMessage);

            model = new CreateUpdateTemplate
            {
                Id = response.Id.Value,
                Version = response.Version.Value,
                Availability = new CreateUpdateTemplateAvailability(),
                Rules = new CreateUpdateTemplateRules
                {
                    RewardTiers = new List<CreateUpdateRewardTier>
                    {
                        new CreateUpdateRewardTier
                        {
                            CurrencyCode = "CAD",
                            BonusTiers = new List<CreateUpdateTemplateTier> {new CreateUpdateTemplateTier {Reward = 27}}
                        }
                    }
                },
                Wagering = new CreateUpdateTemplateWagering(),
                Notification = new CreateUpdateTemplateNotification()
            };
            response = await _bonusApiProxy.CreateUpdateBonusTemplateAsync(model);
            if (response.Success == false)
                throw new Exception(response.Errors.First().ErrorMessage);

            return await _bonusApiProxy.GetTemplateOrNull(response.Id.Value);
        }

        public async Task<Template> CreateTemplate(
            CreateUpdateTemplateInfo info = null,
            CreateUpdateTemplateAvailability availability = null,
            CreateUpdateTemplateRules rules = null,
            CreateUpdateTemplateWagering wagering = null,
            CreateUpdateTemplateNotification notification = null
            )
        {
            var brand = _brandQueries.GetBrands().First();
            var model = new CreateUpdateTemplate
            {
                Id = Guid.Empty,
                Info = info ?? new CreateUpdateTemplateInfo
                {
                    Name = TestDataGenerator.GetRandomString(),
                    TemplateType = BonusType.FirstDeposit,
                    BrandId = brand.Id,
                    WalletTemplateId = brand.WalletTemplates.First().Id,
                    Mode = IssuanceMode.Automatic
                }
            };
            var response = await _bonusApiProxy.CreateUpdateBonusTemplateAsync(model);
            if (response.Success == false)
                throw new Exception(response.Errors.First().ErrorMessage);

            model = new CreateUpdateTemplate
            {
                Id = response.Id.Value,
                Version = response.Version.Value,
                Availability = availability ?? new CreateUpdateTemplateAvailability(),
                Rules = rules ?? new CreateUpdateTemplateRules
                {
                    RewardTiers = new List<CreateUpdateRewardTier>
                    {
                        new CreateUpdateRewardTier
                        {
                            CurrencyCode = "CAD",
                            BonusTiers = new List<CreateUpdateTemplateTier> {new CreateUpdateTemplateTier {Reward = 29}}
                        }
                    }
                },
                Wagering = wagering ?? new CreateUpdateTemplateWagering(),
                Notification = notification ?? new CreateUpdateTemplateNotification()
            };
            response = await _bonusApiProxy.CreateUpdateBonusTemplateAsync(model);
            if (response.Success == false)
                throw new Exception(response.Errors.First().ErrorMessage);

            return await _bonusApiProxy.GetTemplateOrNull(response.Id.Value);
        }
    }
}