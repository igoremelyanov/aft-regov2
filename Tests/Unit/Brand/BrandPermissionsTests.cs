using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Country = AFT.RegoV2.Core.Brand.Interface.Data.Country;
using Culture = AFT.RegoV2.Core.Common.Brand.Data.Culture;
using Currency = AFT.RegoV2.Core.Brand.Interface.Data.Currency;

namespace AFT.RegoV2.Tests.Unit.Brand
{
    class 
        BrandPermissionsTests : BrandTestsBase
    {
        private Country Country { get; set; }
        private Culture Culture { get; set; }
        private Currency Currency { get; set; }
        private FakeGameRepository _fakeGsiRepository;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _fakeGsiRepository = Container.Resolve<FakeGameRepository>();
            FillFakeGsiRepository();

            Country = BrandHelper.CreateCountry("CA", "Canada");
            Culture = BrandHelper.CreateCulture("en-CA", "English (Canada)");
            Currency = BrandHelper.CreateCurrency("CAD", "Canadian Dollar");
        }

        private void FillFakeGsiRepository()
        {
            _fakeGsiRepository.GameProviderConfigurations.Add(new GameProviderConfiguration
            {
                Id = Guid.NewGuid(),
                Name = "name" + TestDataGenerator.GetRandomAlphabeticString(5)
            });

            _fakeGsiRepository.GameProviders.Add(new GameProvider
            {
                Id = Guid.NewGuid(),
                Name = TestDataGenerator.GetRandomAlphabeticString(6),
                GameProviderConfigurations = _fakeGsiRepository.GameProviderConfigurations.ToList()
            });

            _fakeGsiRepository.BetLimits.Add(new GameProviderBetLimit
            {
                GameProviderId = _fakeGsiRepository.GameProviders.First().Id,
                Id = Guid.NewGuid(),
                Code = TestDataGenerator.GetRandomAlphabeticString(5)
            });
        }

        [Test]
        public void Cannot_execute_BrandCommands_without_permissions()
        {
            /* Arrange */
            LogWithNewAdmin(Modules.VipLevelManager, Permissions.View);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => BrandCommands.AddVipLevel(new VipLevelViewModel()));
            Assert.Throws<InsufficientPermissionsException>(() => BrandCommands.EditVipLevel(new VipLevelViewModel()));

            Assert.Throws<InsufficientPermissionsException>(() => BrandCommands.ActivateVipLevel(new Guid(), "remarks"));
            Assert.Throws<InsufficientPermissionsException>(() => BrandCommands.DeactivateVipLevel(new Guid(), "remarks", null));

            Assert.Throws<InsufficientPermissionsException>(() => BrandCommands.CreateCountry(TestDataGenerator.GetRandomString(2), TestDataGenerator.GetRandomString(5)));
            Assert.Throws<InsufficientPermissionsException>(() => BrandCommands.UpdateCountry(TestDataGenerator.GetRandomString(2), TestDataGenerator.GetRandomString(5)));
            Assert.Throws<InsufficientPermissionsException>(() => BrandCommands.DeleteCountry(TestDataGenerator.GetRandomString(2)));

            Assert.Throws<InsufficientPermissionsException>(() => BrandCommands.ActivateCulture(TestDataGenerator.GetRandomString(2), "remark"));
            Assert.Throws<InsufficientPermissionsException>(() => BrandCommands.DeactivateCulture(TestDataGenerator.GetRandomString(2), "remark"));

            Assert.Throws<InsufficientPermissionsException>(() => BrandHelper.CreateBrand());
        }

        [Test]
        public void Cannot_execute_BrandQueries_without_permissions()
        {
            /* Arrange */
            LogWithNewAdmin(Modules.VipLevelManager, Permissions.Create);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => BrandQueries.GetVipLevels());
            Assert.Throws<InsufficientPermissionsException>(() => BrandQueries.GetAllLicensees());
        }

        [Test]
        public void Cannot_add_vip_level_with_invalid_brand()
        {
            // Arrange
            var brand = BrandTestHelper.CreateBrand();
            var addVipLevelCommand = CreateAddVipLevelCommand(brand);

            LogWithNewAdmin(Modules.VipLevelManager, Permissions.Create);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => BrandCommands.AddVipLevel(addVipLevelCommand));
        }

        [Test]
        public void Cannot_edit_vip_level_with_invalid_brand()
        {
            // Arrange
            var brand = BrandTestHelper.CreateBrand();
            var addVipLevelCommand = CreateAddVipLevelCommand(brand);

            LogWithNewAdmin(Modules.VipLevelManager, Permissions.Update);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => BrandCommands.EditVipLevel(addVipLevelCommand));
        }

        private VipLevelViewModel CreateAddVipLevelCommand(Core.Brand.Interface.Data.Brand brand)
        {
            var suffix = TestDataGenerator.GetRandomAlphabeticString(5);

            var vipLevel = new VipLevelViewModel
            {
                Brand = brand.Id,
                Code = "code" + suffix,
                Name = "name" + suffix,
                Description = "description" + suffix,
                Rank = TestDataGenerator.GetRandomNumber(1000),
                Color = TestDataGenerator.GetRandomColor(),
                Limits = new[]
                {
                    new VipLevelLimitViewModel
                    {
                        GameProviderId = _fakeGsiRepository.GameProviders.First().Id,
                        CurrencyCode = "CAD",
                        BetLimitId = _fakeGsiRepository.BetLimits.First().Id
                    }
                }
            };

            return vipLevel;
        }
    }
}
