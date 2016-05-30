using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Licensee = AFT.RegoV2.Core.Brand.Interface.Data.Licensee;

namespace AFT.RegoV2.Tests.Unit.Brand
{
    class BrandSecurityTests : SecurityTestsBase
    {
        private BrandCommands _brandCommands;
        private FakeGameRepository _fakeGameRepository;
        private IActorInfoProvider _actorInfoProvider;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _brandCommands = Container.Resolve<BrandCommands>();

            foreach (var countryCode in TestDataGenerator.CountryCodes)
            {
                BrandRepository.Countries.Add(new Country { Code = countryCode });
            }

            foreach (var cultureCode in TestDataGenerator.CultureCodes.Where(x => x != null))
            {
                BrandRepository.Cultures.Add(new Culture { Code = cultureCode });
            }

            _fakeGameRepository = Container.Resolve<FakeGameRepository>();
            _fakeGameRepository.GameProviderConfigurations.Add(new GameProviderConfiguration
            {
                Id = Guid.NewGuid(),
                Name = "name" + TestDataGenerator.GetRandomAlphabeticString(5)
            });

            _fakeGameRepository.GameProviders.Add(new GameProvider
            {
                Id = Guid.NewGuid(),
                Name = TestDataGenerator.GetRandomAlphabeticString(6),
                GameProviderConfigurations = _fakeGameRepository.GameProviderConfigurations.ToList()
            });

            _fakeGameRepository.BetLimits.Add(new GameProviderBetLimit
            {
                GameProviderId = _fakeGameRepository.GameProviders.First().Id,
                Id = Guid.NewGuid(),
                Code = TestDataGenerator.GetRandomAlphabeticString(5)
            });

            _actorInfoProvider = Container.Resolve<IActorInfoProvider>();
        }

        public Core.Brand.Interface.Data.Brand CreateBrand(Licensee licensee)
        {
            var suffix = TestDataGenerator.GetRandomAlphabeticString(3);

            var brand = new Core.Brand.Interface.Data.Brand
            {
                Id = Guid.NewGuid(),
                Code = "Code" + suffix,
                Name = "Name" + suffix,
                Licensee = licensee,
                Status = BrandStatus.Active,
                TimezoneId = "Pacific Standard Time"
            };

            licensee.Brands.Add(brand);

            BrandRepository.Brands.Add(brand);
            BrandRepository.SaveChanges();

            return brand;
        }

        public Licensee CreateLicensee(int brandCount = 1)
        {
            var licensee = new Licensee
            {
                Id = Guid.NewGuid(),
                AllowedBrandCount = brandCount,
                Status = LicenseeStatus.Active
            };

            BrandRepository.Licensees.Add(licensee);
            BrandRepository.SaveChanges();

            return licensee;
        }

        [Test]
        public void Cannot_access_single_licensee_brands_that_are_not_allowed_for_admin()
        {
            /*** Arrange ***/
            var licensee = CreateLicensee(15);

            var notAllowedBrands = new List<Guid>();
            var allowedBrands = new List<Guid>();

            // Generate 10 brands that are not allowed to user
            for (var i = 0; i < 10; i++)
            {
                var brand = CreateBrand(licensee);
                notAllowedBrands.Add(brand.Id);
            }

            var currentUser = SecurityRepository.GetAdminById(_actorInfoProvider.Actor.Id);
            var currentUserAllowedBrands = currentUser.AllowedBrands.Select(b => b.Id).ToList();
            currentUserAllowedBrands.AddRange(notAllowedBrands);
            currentUser.SetAllowedBrands(currentUserAllowedBrands);
            SecurityTestHelper.SignInAdmin(currentUser);

            var admin = SecurityTestHelper.CreateAdmin();

            admin.Licensees.Clear();
            admin.AllowedBrands.Clear();

            // Generate 5 brands that are allowed to user
            for (var i = 0; i < 5; i++)
            {
                var brand = CreateBrand(licensee);
                admin.AddAllowedBrand(brand.Id);
                allowedBrands.Add(brand.Id);
            }

            SecurityTestHelper.SignInAdmin(admin);

            /*** Act ***/
            var brands = BrandQueries.GetAllBrands();

            /*** Assert ***/
            Assert.AreEqual(brands.Count(), allowedBrands.Count);
            // Check if filtered brands are same with allowed brands
            Assert.True(!brands.Select(b => b.Id).Except(allowedBrands).Any());
            // Check if there are no forbidden brands among filtered for user
            Assert.False(brands.Select(b => b.Id).Intersect(notAllowedBrands).Any());
        }

        [Test]
        public void Cannot_access_multiple_licensees_brands_that_are_not_allowed_for_admin()
        {
            var notAllowedBrands = new List<Guid>();
            var allowedBrands = new List<Guid>();

            // Generate 10 brands that are not allowed to user
            for (var i = 0; i < 10; i++)
            {
                // Generate licensee for brand
                var licensee = CreateLicensee();

                var brand = CreateBrand(licensee);
                notAllowedBrands.Add(brand.Id);
            }

            var currentUser = SecurityRepository.GetAdminById(_actorInfoProvider.Actor.Id);
            var currentUserAllowedBrands = currentUser.AllowedBrands.Select(b => b.Id).ToList();
            currentUserAllowedBrands.AddRange(notAllowedBrands);
            currentUser.SetAllowedBrands(currentUserAllowedBrands);
            SecurityTestHelper.SignInAdmin(currentUser);

            var admin = SecurityTestHelper.CreateAdmin();

            admin.Licensees.Clear();
            admin.AllowedBrands.Clear();

            // Generate 5 brands that are allowed to user
            for (var i = 0; i < 5; i++)
            {
                var licensee = CreateLicensee();

                var brand = CreateBrand(licensee);
                admin.AddAllowedBrand(brand.Id);
                allowedBrands.Add(brand.Id);
            }

            SecurityTestHelper.SignInAdmin(admin);

            /*** Act ***/
            var brands = BrandQueries.GetAllBrands();

            /*** Assert ***/
            Assert.AreEqual(brands.Count(), allowedBrands.Count);
            // Check if filtered brands are same with allowed brands
            Assert.True(!brands.Select(b => b.Id).Except(allowedBrands).Any());
            // Check if there are no forbidden brands among filtered for user
            Assert.False(brands.Select(b => b.Id).Intersect(notAllowedBrands).Any());
        }

        private VipLevelViewModel CreateAddVipLevelCommand(Core.Brand.Interface.Data.Brand brand)
        {
            BrandRepository.SaveChanges();

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
                        GameProviderId = _fakeGameRepository.GameProviders.First().Id,
                        CurrencyCode = "CAD",
                        BetLimitId = _fakeGameRepository.BetLimits.First().Id
                    }
                }
            };

            while (BrandQueries.GetAllVipLevels().Any(x => x.Brand.Id == brand.Id && x.Rank == vipLevel.Rank))
            {
                vipLevel.Rank = TestDataGenerator.GetRandomNumber(1000);
            }

            return vipLevel;
        }

        [Test]
        public void Cannot_access_vip_levels_that_are_not_allowed_to_admin()
        {
            /*** Arrange ***/
            var licensee = CreateLicensee(2);

            var role = SecurityTestHelper.CreateRole();
            var admin = SecurityTestHelper.CreateAdmin(Licensee.Id, roleId: role.Id);
            role.Id = RoleIds.SingleBrandManagerId;

            admin.Licensees.Clear();
            admin.AllowedBrands.Clear();
            admin.Currencies.Clear();

            admin.SetLicensees(new[] { licensee.Id });

            // Create 10 vip levels for brand that is restricted to user
            var restrictedBrand = CreateBrand(licensee);
            admin.AddAllowedBrand(restrictedBrand.Id);
            SecurityTestHelper.SignInAdmin(admin);

            for (var i = 0; i < 10; i++)
            {
                var addVipLevelCommand = CreateAddVipLevelCommand(restrictedBrand);
                _brandCommands.AddVipLevel(addVipLevelCommand);
                var vipLevel = BrandQueries.GetVipLevels().FirstOrDefault(x => x.Code == addVipLevelCommand.Code);
                restrictedBrand.VipLevels.Add(vipLevel);
            }

            // Create 10 vip levels for brand that is allowed to user
            admin.AllowedBrands.Clear();
            var allowedBrand = CreateBrand(licensee);
            admin.AddAllowedBrand(allowedBrand.Id);
            SecurityTestHelper.SignInAdmin(admin);

            for (var i = 0; i < 10; i++)
            {
                var addVipLevelCommand = CreateAddVipLevelCommand(allowedBrand);
                _brandCommands.AddVipLevel(addVipLevelCommand);
                var vipLevel = BrandQueries.GetVipLevels().FirstOrDefault(x => x.Code == addVipLevelCommand.Code);
                allowedBrand.VipLevels.Add(vipLevel);
            }

            /*** Act ***/
            var vipLevels = BrandQueries.GetAllVipLevels();

            /*** Assert ***/
            Assert.IsNotNull(vipLevels);
            Assert.IsNotEmpty(vipLevels);
            // Check if filtered vip levels are same with vip levels of allowed brands
            Assert.True(!vipLevels.Select(v => v.Id)
                .Except(
                    allowedBrand.VipLevels.Select(v => v.Id))
                .Any());
            // Check if there are no restricted vip levels brands among filtered for user
            Assert.False(vipLevels.Select(v => v.Id)
                .Intersect(
                    restrictedBrand.VipLevels.Select(v => v.Id))
                .Any());
        }

        [Test]
        public void Cannot_access_licensees_that_are_not_allowed_to_admin()
        {
            /*** Arrange ***/
            var admin = SecurityTestHelper.CreateAdmin();

            var allowedLicensees = new List<Guid>();

            for (var i = 0; i < 10; i++)
            {
                var licensee = CreateLicensee();
                allowedLicensees.Add(licensee.Id);
            }

            admin.SetLicensees(allowedLicensees);

            var restrictedLicensees = new List<Guid>();

            for (var i = 0; i < 10; i++)
            {
                var licensee = CreateLicensee();
                restrictedLicensees.Add(licensee.Id);
            }

            /*** Act ***/
            SecurityTestHelper.SignInAdmin(admin);

            var filteredLicensees = BrandQueries.GetAllLicensees();

            /*** Assert ***/
            Assert.IsNotNull(filteredLicensees);
            Assert.IsNotEmpty(filteredLicensees);
            // Check if filtered licensees are same with allowed ones
            Assert.True(!filteredLicensees.Select(l => l.Id)
                .Except(allowedLicensees)
                .Any());
            // Check if there are no restricted licensees among filtered for user
            Assert.False(filteredLicensees.Select(l => l.Id)
                .Intersect(restrictedLicensees)
                .Any());
        }

        [Test]
        public void Cannot_access_countries_that_are_not_allowed_to_admin()
        {
            /*** Arrange ***/
            var admin = SecurityTestHelper.CreateAdmin();

            var allowedLicensee = CreateLicensee();
            var allowedBrand = CreateBrand(allowedLicensee);

            BrandRepository.Brands.Single(x => x.Id == Brand.Id).BrandCountries.Clear();

            var allowedCountry = BrandRepository.Countries.Single(c => c.Code == "US");
            allowedLicensee.Countries.Add(allowedCountry);
            allowedBrand.BrandCountries.Add(new BrandCountry 
            {
                BrandId = allowedBrand.Id, 
                Country = allowedCountry, 
                CountryCode = allowedCountry.Code 
            });

            admin.SetLicensees(new[] { allowedLicensee.Id });

            var restrictedLicensee = CreateLicensee();
            var restrictedBrand = CreateBrand(restrictedLicensee);

            var restrictedCountry = BrandRepository.Countries.Single(c => c.Code == "CN");
            restrictedLicensee.Countries.Add(restrictedCountry);
            restrictedBrand.BrandCountries.Add(new BrandCountry
            {
                BrandId = restrictedBrand.Id, 
                Country = restrictedCountry, 
                CountryCode = restrictedCountry.Code
            });

            /*** Act ***/
            SecurityTestHelper.SignInAdmin(admin);

            var filteredAllowedCountries = BrandQueries.GetAllBrandCountries().Where(c => c.BrandId == allowedBrand.Id);
            var filteredRestrictedCountries = BrandQueries.GetAllBrandCountries().Where(c => c.BrandId == restrictedBrand.Id);

            var filteredAllowedBrandCountries = BrandQueries.GetAllBrandCountries().Where(c => c.BrandId == allowedBrand.Id);
            var filteredRestrictedBrandCountries = BrandQueries.GetAllBrandCountries().Where(c => c.BrandId == restrictedBrand.Id);

            /*** Assert ***/
            Assert.IsNotNull(filteredAllowedCountries);
            Assert.IsNotEmpty(filteredAllowedCountries);
            Assert.AreEqual(filteredAllowedCountries.Count(), 1);
            Assert.True(filteredAllowedCountries.Any(ac => ac.CountryCode == allowedCountry.Code));

            Assert.IsNotNull(filteredRestrictedCountries);
            Assert.IsEmpty(filteredRestrictedCountries);

            Assert.IsNotNull(filteredAllowedBrandCountries);
            Assert.IsNotEmpty(filteredAllowedBrandCountries);
            Assert.AreEqual(filteredAllowedBrandCountries.Count(), 1);
            Assert.True(filteredAllowedBrandCountries.Any(ac => ac.CountryCode == allowedCountry.Code));

            Assert.IsNotNull(filteredRestrictedBrandCountries);
            Assert.IsEmpty(filteredRestrictedBrandCountries);
        }

        [Test]
        public void Cannot_access_cultures_that_are_not_allowed_to_admin()
        {
            /*** Arrange ***/
            var admin = SecurityTestHelper.CreateAdmin();

            var allowedLicensee = CreateLicensee();
            var allowedBrand = CreateBrand(allowedLicensee);

            BrandRepository.Brands.Single(x => x.Id == Brand.Id).BrandCultures.Clear();

            var allowedCulture = BrandRepository.Cultures.Single(c => c.Code == "en-US");
            allowedLicensee.Cultures.Add(allowedCulture);
            allowedBrand.BrandCultures.Add(new BrandCulture
            {
                BrandId = allowedBrand.Id, 
                Culture = allowedCulture,
                CultureCode = allowedCulture.Code
            });

            admin.SetLicensees(new[] { allowedLicensee.Id });

            var restrictedLicensee = CreateLicensee();
            var restrictedBrand = CreateBrand(restrictedLicensee);

            var restrictedCulture = BrandRepository.Cultures.Single(c => c.Code == "zh-TW");
            restrictedLicensee.Cultures.Add(restrictedCulture);
            restrictedBrand.BrandCultures.Add(new BrandCulture
            {
                BrandId = restrictedBrand.Id, 
                Culture = restrictedCulture,
                CultureCode = restrictedCulture.Code
            });

            /*** Act ***/
            SecurityTestHelper.SignInAdmin(admin);

            var filteredAllowedCultures = BrandQueries.GetAllBrandCultures().Where(c => c.BrandId == allowedBrand.Id);
            var filteredRestrictedCultures = BrandQueries.GetAllBrandCultures().Where(c => c.BrandId == restrictedBrand.Id);

            var filteredAllowedBrandCultures = BrandQueries.GetAllBrandCultures().Where(c => c.BrandId == allowedBrand.Id);
            var filteredRestrictedBrandCultures = BrandQueries.GetAllBrandCultures().Where(c => c.BrandId == restrictedBrand.Id);

            /*** Assert ***/
            Assert.IsNotNull(filteredAllowedCultures);
            Assert.IsNotEmpty(filteredAllowedCultures);
            Assert.AreEqual(filteredAllowedCultures.Count(), 1);
            Assert.True(filteredAllowedCultures.Any(ac => ac.CultureCode == allowedCulture.Code));

            Assert.IsNotNull(filteredRestrictedCultures);
            Assert.IsEmpty(filteredRestrictedCultures);

            Assert.IsNotNull(filteredAllowedBrandCultures);
            Assert.IsNotEmpty(filteredAllowedBrandCultures);
            Assert.AreEqual(filteredAllowedBrandCultures.Count(), 1);
            Assert.True(filteredAllowedBrandCultures.Any(ac => ac.CultureCode == allowedCulture.Code));

            Assert.IsNotNull(filteredRestrictedBrandCultures);
            Assert.IsEmpty(filteredRestrictedBrandCultures);
        }

        [Test]
        public void Can_filter_licensees()
        {
            // *** Arrange ***
            var licensees = CreateLicensees(3);

            var userLicensee = licensees.First();
            var admin = SecurityTestHelper.CreateAdmin(licenseeId: userLicensee.Id);
            SecurityTestHelper.SignInAdmin(admin);

            // *** Act ***
            var filtered = BrandQueries.GetFilteredLicensees();

            // *** Assert ***
            Assert.IsNotNull(filtered);
            Assert.True(filtered.Count() == 1);
            Assert.True(filtered.Any(l => l.Id == userLicensee.Id));
        }

        [Test]
        public void Can_filter_non_active_licensees()
        {
            // *** Arrange ***
            const int activeLicenseeCount = 3;

            var activeLicensees = CreateLicensees(activeLicenseeCount);
            var inactiveLicensees = CreateLicensees(4, LicenseeStatus.Inactive);
            var deactivatedLicensee = CreateLicensees(2, LicenseeStatus.Deactivated);
            var licensees = activeLicensees.Concat(inactiveLicensees).Concat(deactivatedLicensee).ToArray();

            var admin = SecurityTestHelper.CreateAdmin(licensees.Select(x => x.Id).ToArray());
            SecurityTestHelper.SignInAdmin(admin);

            // *** Act ***
            var filtered = BrandQueries.GetFilteredLicensees();

            // *** Assert ***
            Assert.IsNotNull(filtered);
            Assert.True(filtered.Count() == activeLicenseeCount);
        }

        [Test]
        public void SuperAdmin_can_access_all_licensees()
        {
            // *** Arrange ***
            const int licenseeCount = 3;
            CreateLicensees(licenseeCount);

            // *** Act ***
            var filtered = BrandQueries.GetFilteredLicensees();

            // *** Assert ***
            Assert.IsNotNull(filtered);
            //Before each creates 1 licensee
            Assert.True(filtered.Count() == licenseeCount + 1);
        }

        [Test]
        public void Can_filter_brands()
        {
            // *** Arrange ***
            var admin = SecurityTestHelper.CreateAdmin(licenseeId: Licensee.Id);

            admin.AllowedBrands.Clear();

            var brands = new List<Core.Brand.Interface.Data.Brand>();

            const int brandCount = 20;
            const int userBrandCount = 10;

            // Create user brands
            for (var i = 0; i < userBrandCount; i++)
            {
                var brand = CreateBrand();

                brands.Add(brand);

                AdminCommands.AddBrandToAdmin(admin.Id, brand.Id);
            }

            // Create other brands
            for (var i = 0; i < brandCount - userBrandCount; i++)
            {
                var brand = CreateBrand();

                brands.Add(brand);
            }

            SecurityTestHelper.SignInAdmin(admin);

            // *** Act ***
            var filtered = BrandQueries.GetFilteredBrands(brands, admin.Id);

            // *** Assert ***
            Assert.IsNotNull(filtered);
            Assert.True(filtered.Count() == userBrandCount);

            var userBrandIds = admin.AllowedBrands.Select(b => b.Id);
            var filteredBrandIds = filtered.Select(b => b.Id);
            var isEqual = userBrandIds.OrderBy(a => a).SequenceEqual(filteredBrandIds.OrderBy(a => a));

            Assert.True(isEqual);
        }

        [Test]
        public void SuperAdmin_can_access_all_brands()
        {
            // *** Arrange ***
            const int brandCount = 20;
            var brands = new List<Core.Brand.Interface.Data.Brand>();

            for (var i = 0; i < brandCount; i++)
            {
                brands.Add(CreateBrand());
            }

            var admin = SecurityRepository.GetAdminById(_actorInfoProvider.Actor.Id);
            var allowedBrands = brands.Select(b => b.Id).ToList();
            allowedBrands.AddRange(admin.AllowedBrands.Select(b => b.Id));
            admin.SetAllowedBrands(allowedBrands);
            SecurityTestHelper.SignInAdmin(admin);

            // *** Act ***
            var filtered = BrandQueries.GetFilteredBrands(brands, admin.Id);

            // *** Assert ***
            Assert.IsNotNull(filtered);
            Assert.True(filtered.Count() == brandCount);
        }

        private IList<Licensee> CreateLicensees(int count = 1, LicenseeStatus status = LicenseeStatus.Active)
        {
            var result = new List<Licensee>();
            for (var i = 0; i < count; i++)
            {
                var licensee = BrandHelper.CreateLicensee();
                licensee.Status = status;
                result.Add(licensee);
            }

            return result;
        }

        public Core.Brand.Interface.Data.Brand CreateBrand()
        {
            var suffix = TestDataGenerator.GetRandomAlphabeticString(3);

            var brand = new Core.Brand.Interface.Data.Brand
            {
                Id = Guid.NewGuid(),
                Code = "Code" + suffix,
                Name = "Name" + suffix
            };

            BrandRepository.Brands.Add(brand);
            BrandRepository.SaveChanges();

            return brand;
        }
    }
}