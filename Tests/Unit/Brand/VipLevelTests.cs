using System;
using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Brand
{
    internal class VipLevelTests : SecurityTestsBase
    {
        private FakeGameRepository _gameRepository;
        private IBrandRepository _brandRepository;
        private BrandCommands _brandCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _gameRepository = Container.Resolve<FakeGameRepository>();
            _brandCommands = Container.Resolve<BrandCommands>();
            _brandRepository = Container.Resolve<IBrandRepository>();
            FillGamesRepository();
        }

        private void FillGamesRepository()
        {
            _gameRepository.GameProviderConfigurations.Add(new GameProviderConfiguration
                {
                    Id = Guid.NewGuid(),
                    Name = "name" + TestDataGenerator.GetRandomAlphabeticString(5)
                });

            _gameRepository.GameProviders.Add(new GameProvider
            {
                Id = Guid.NewGuid(),
                Name = TestDataGenerator.GetRandomAlphabeticString(6),
                GameProviderConfigurations = _gameRepository.GameProviderConfigurations.ToList()
            });

            _gameRepository.BetLimits.Add(new GameProviderBetLimit
            {
                GameProviderId = _gameRepository.GameProviders.First().Id,
                Id = Guid.NewGuid(),
                Code = TestDataGenerator.GetRandomAlphabeticString(5)
            });
        }

        [Test]
        public void Can_create_VIP_level()
        {
            var role = SecurityTestHelper.CreateRole();
            var admin = SecurityTestHelper.CreateAdmin(Licensee.Id, roleId: role.Id);
            role.Id = RoleIds.SingleBrandManagerId;

            admin.Licensees.Clear();
            admin.AllowedBrands.Clear();
            admin.Currencies.Clear();

            admin.SetLicensees(new[] { Licensee.Id });
            admin.AddAllowedBrand(Brand.Id);
            SecurityTestHelper.SignInAdmin(admin);

            var addVipLevelCommand = CreateAddVipLevelCommand(Brand);

            _brandCommands.AddVipLevel(addVipLevelCommand);

            var vipLevel = BrandQueries.GetVipLevels().FirstOrDefault(x => x.Code == addVipLevelCommand.Code);

            Assert.That(vipLevel, Is.Not.Null);
            Assert.That(vipLevel.Name, Is.EqualTo(addVipLevelCommand.Name));
            Assert.That(vipLevel.VipLevelGameProviderBetLimits.Count, Is.EqualTo(1));
            Assert.That(vipLevel.VipLevelGameProviderBetLimits.First().Currency.Code, Is.EqualTo(addVipLevelCommand.Limits.First().CurrencyCode));
            Assert.That(vipLevel.VipLevelGameProviderBetLimits.First().GameProviderId, Is.EqualTo(addVipLevelCommand.Limits.First().GameProviderId));
            //            Assert.That(vipLevel.VipLevelLimits.First().Minimum, Is.EqualTo(addVipLevelCommand.Limits.First().Minimum));
        }

        [Test]
        public void Can_not_create_two_default_vip_levels()
        {
            var role = SecurityTestHelper.CreateRole();
            var admin = SecurityTestHelper.CreateAdmin(Licensee.Id, roleId: role.Id);
            role.Id = RoleIds.SingleBrandManagerId;

            admin.Licensees.Clear();
            admin.AllowedBrands.Clear();
            admin.Currencies.Clear();

            admin.SetLicensees(new[] { Licensee.Id });
            admin.AddAllowedBrand(Brand.Id);
            SecurityTestHelper.SignInAdmin(admin);

            var vipLevel = CreateAddVipLevelCommand(Brand);
            var vipLevel2 = CreateAddVipLevelCommand(Brand);
            vipLevel2.IsDefault = true;

            _brandCommands.AddVipLevel(vipLevel);

            Action action = () =>_brandCommands.AddVipLevel(vipLevel2);

            action.ShouldThrow<RegoValidationException>()
                .Where(x => x.Message.Contains("Default vip level for this brand already exists"));
        }

        [Test]
        public void Can_deactivate_Default_VIP_Level()
        {
            var role = SecurityTestHelper.CreateRole();
            var admin = SecurityTestHelper.CreateAdmin(Licensee.Id, roleId: role.Id);
            role.Id = RoleIds.SingleBrandManagerId;

            admin.Licensees.Clear();
            admin.AllowedBrands.Clear();
            admin.Currencies.Clear();

            admin.SetLicensees(new[] { Licensee.Id });
            admin.AddAllowedBrand(Brand.Id);
            SecurityTestHelper.SignInAdmin(admin);

            var newVipLevel = CreateAddVipLevelCommand(Brand);
            var defaultVipLevelId = Brand.DefaultVipLevelId;
            var newVipLevelId = _brandCommands.AddVipLevel(newVipLevel);

            Assert.DoesNotThrow(() =>
            {
                _brandCommands.DeactivateVipLevel(defaultVipLevelId.Value, "-", newVipLevelId);
            });


            var oldDefaultVipLevel = BrandRepository.VipLevels.Single(o => o.Id == defaultVipLevelId);
            var newDefaultVipLevel = BrandRepository.VipLevels.Single(o => o.Id == newVipLevelId);
            var brand = BrandRepository.Brands.Single(o => o.Id == Brand.Id);

            Assert.True(oldDefaultVipLevel.Status == VipLevelStatus.Inactive);
            Assert.True(brand.DefaultVipLevelId == newDefaultVipLevel.Id && newDefaultVipLevel.Status == VipLevelStatus.Active);
        }

        [Test]
        public void Can_activate_vip_level()
        {
            var addVipLevelCommand = CreateAddVipLevelCommand(Brand);
            var id = _brandCommands.AddVipLevel(addVipLevelCommand);

            BrandRepository.VipLevels.Single(x => x.Id == id).Status = VipLevelStatus.Inactive;

            _brandCommands.ActivateVipLevel(id, "");

            var status = BrandRepository.VipLevels.Single(x => x.Id == id).Status;

            Assert.That(status, Is.EqualTo(VipLevelStatus.Active));
        }

        [Test]
        public void Can_create_two_vip_levels_for_brand_with_different_name_and_code()
        {
            var role = SecurityTestHelper.CreateRole();
            var admin = SecurityTestHelper.CreateAdmin(Licensee.Id, roleId: role.Id);
            role.Id = RoleIds.SingleBrandManagerId;

            admin.Licensees.Clear();
            admin.AllowedBrands.Clear();
            admin.Currencies.Clear();

            admin.SetLicensees(new[] { Licensee.Id });
            admin.AddAllowedBrand(Brand.Id);
            SecurityTestHelper.SignInAdmin(admin);

            var vipLevel1 = CreateAddVipLevelCommand(Brand);
            var vipLevel2 = CreateAddVipLevelCommand(Brand);

            _brandCommands.AddVipLevel(vipLevel1);
            _brandCommands.AddVipLevel(vipLevel2);

            var vipLevels = BrandQueries.GetVipLevels().Where(x => x.Id == vipLevel1.Id || x.Id == vipLevel2.Id).ToList();

            Assert.That(vipLevels.Count, Is.EqualTo(2));
        }

        [Test]
        public void Can_not_create_two_vip_levels_with_same_name_for_one_brand()
        {
            var role = SecurityTestHelper.CreateRole();
            var admin = SecurityTestHelper.CreateAdmin(Licensee.Id, roleId: role.Id);
            role.Id = RoleIds.SingleBrandManagerId;

            admin.Licensees.Clear();
            admin.AllowedBrands.Clear();
            admin.Currencies.Clear();

            admin.SetLicensees(new[] { Licensee.Id });
            admin.AddAllowedBrand(Brand.Id);
            SecurityTestHelper.SignInAdmin(admin);

            var vipLevel1 = CreateAddVipLevelCommand(Brand);
            var vipLevel2 = CreateAddVipLevelCommand(Brand, vipLevel1.Name);

            _brandCommands.AddVipLevel(vipLevel1);

            Action action = () => _brandCommands.AddVipLevel(vipLevel2);

            action.ShouldThrow<RegoValidationException>()
                .Where(x => x.Message.Contains("common.nameUnique"));
        }

        [Test]
        public void Can_not_create_two_vip_levels_with_same_code_for_one_brand()
        {
            var role = SecurityTestHelper.CreateRole();
            var admin = SecurityTestHelper.CreateAdmin(Licensee.Id, roleId: role.Id);
            role.Id = RoleIds.SingleBrandManagerId;

            admin.Licensees.Clear();
            admin.AllowedBrands.Clear();
            admin.Currencies.Clear();

            admin.SetLicensees(new[] { Licensee.Id });
            admin.AddAllowedBrand(Brand.Id);
            SecurityTestHelper.SignInAdmin(admin);

            var vipLevel1 = CreateAddVipLevelCommand(Brand);
            var vipLevel2 = CreateAddVipLevelCommand(Brand, code: vipLevel1.Code);

            _brandCommands.AddVipLevel(vipLevel1);

            Action action = () => _brandCommands.AddVipLevel(vipLevel2);

            action.ShouldThrow<RegoValidationException>()
                .Where(x => x.Message.Contains("common.codeUnique"));
        }

        [Test]
        public void Can_create_two_vip_levels_with_same_code_and_name_for_different_brands()
        {
            var brand2 = BrandHelper.CreateBrand(Brand.Licensee);
            SecurityTestHelper.CreateBrand(brand2.Id, brand2.LicenseeId, brand2.TimezoneId);

            var role = SecurityTestHelper.CreateRole();
            var admin = SecurityTestHelper.CreateAdmin(Licensee.Id, roleId: role.Id);
            role.Id = RoleIds.SingleBrandManagerId;

            admin.Licensees.Clear();
            admin.AllowedBrands.Clear();
            admin.Currencies.Clear();

            admin.SetLicensees(new[] { Licensee.Id });
            admin.AddAllowedBrand(Brand.Id);
            admin.AddAllowedBrand(brand2.Id);
            SecurityTestHelper.SignInAdmin(admin);

            var vipLevel1 = CreateAddVipLevelCommand(Brand);
            var vipLevel2 = CreateAddVipLevelCommand(brand2, vipLevel1.Name, vipLevel1.Code);

            _brandCommands.AddVipLevel(vipLevel1);
            _brandCommands.AddVipLevel(vipLevel2);
            
            var vipLevels = BrandQueries.GetVipLevels().Where(x => x.Id == vipLevel1.Id || x.Id == vipLevel2.Id).ToList();

            Assert.That(vipLevels.Count, Is.EqualTo(2));
        }

        private VipLevelViewModel CreateAddVipLevelCommand(Core.Brand.Interface.Data.Brand brand, string name = "", string code = "")
        {
            var suffix = TestDataGenerator.GetRandomAlphabeticString(5);
            int rank;
            do
            {
                rank = TestDataGenerator.GetRandomNumber(1000);
            }
            while (_brandRepository.VipLevels.Any(vl =>vl.Rank == rank && vl.BrandId==brand.Id));

            var vipLevel = new VipLevelViewModel
            {
                Id = Guid.NewGuid(),
                Brand = brand.Id,
                Code = string.IsNullOrEmpty(code) ? "code" + suffix : code,
                Name = string.IsNullOrEmpty(name) ? "name" + suffix : name,
                Description = "description" + suffix,
                Rank = rank,
                Color = TestDataGenerator.GetRandomColor(),
                Limits = new[]
                {
                    new VipLevelLimitViewModel
                    {
                        GameProviderId = _gameRepository.GameProviders.First().Id,
                        CurrencyCode = "CAD",
                        BetLimitId = _gameRepository.BetLimits.First().Id
                    }
                }
            };

            return vipLevel;
        }
    }
}