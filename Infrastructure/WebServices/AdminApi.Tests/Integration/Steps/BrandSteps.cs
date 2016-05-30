using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using AFT.RegoV2.AdminApi.Interface.Brand;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Proxy;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Data.Brand.ContentTranslations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.AdminApi.Tests.Integration.Steps
{
    public class BrandSteps : BaseSteps
    {
        private BrandTestHelper BrandTestHelper { get; set; }
        private SecurityTestHelper SecurityTestHelper { get; set; }

        public BrandSteps()
        {
            SecurityTestHelper = Container.Resolve<SecurityTestHelper>();
            SecurityTestHelper.SignInClaimsSuperAdmin();
            BrandTestHelper = Container.Resolve<BrandTestHelper>();
        }

        [Given(@"I am logged in and have access token")]
        public void GivenIAmLoggedInAndHaveAccessToken()
        {
            var username = ScenarioContext.Current.ContainsKey("username") ? ScenarioContext.Current.Get<string>("username") : "SuperAdmin";
            var password = ScenarioContext.Current.ContainsKey("password") ? ScenarioContext.Current.Get<string>("password") : "SuperAdmin";

            LogInAdminApi(username, password);
            Token.Should().NotBeNullOrWhiteSpace();
        }

        [Given(@"New user with (.*) permission in (.*) module is created")]
        public void GivenNewUserWithSufficientPermissionsIsCreated(string permissionName, string module)
        {
            var permissions = new[] { permissionName };
            var licensee = BrandTestHelper.CreateLicensee();
            var brand = BrandTestHelper.CreateBrand(licensee, isActive: true);
            var brands = new[] { brand };
            var password = TestDataGenerator.GetRandomString(6);

            var user = SecurityTestHelper.CreateAdmin(module, permissions, brands, password);

            ScenarioContext.Current.Add("username", user.Username);
            ScenarioContext.Current.Add("password", password);
        }

        [Given(@"I am logged in as a user with insufficient permissions")]
        public void GivenIAmLoggedInAsAUserWithInsufficientPermissions()
        {
            LogWithNewUser(Modules.VipLevelManager, Permissions.View);
        }

        [Given(@"I am not logged in and I do not have valid token")]
        public void GivenIAmNotLoggedInAndIDoNotHaveValidToken()
        {
            SetInvalidToken();
        }

        [Given(@"New (.*) brand is created")]
        public void GivenNewDeactivatedBrandIsCreated(string activationStatus)
        {
            var isActive = activationStatus.Equals("activated");
            var licensee = BrandTestHelper.CreateLicensee();
            ScenarioContext.Current.Add("licenseeId", licensee.Id);
            ScenarioContext.Current.Add("brandId", BrandTestHelper.CreateBrand(licensee, isActive: isActive).Id);
        }

        [When(@"New (.*) brand is created")]
        public void WhenNewBrandIsCreated(string activationStatus)
        {
            var isActive = activationStatus.Equals("activated");
            var licensee = BrandTestHelper.CreateLicensee();
            var brand = BrandTestHelper.CreateBrand(licensee, isActive: isActive);
            ScenarioContext.Current.Add("licenseeId", licensee.Id);
            ScenarioContext.Current.Add("brandId", brand.Id);
            
            SecurityTestHelper.CreateBrand(brand.Id, brand.LicenseeId, brand.TimezoneId);
        }

        [When(@"New country is created")]
        public void WhenNewCountryIsCreated()
        {
            ScenarioContext.Current.Add("countryCode", BrandTestHelper.CreateCountry(TestDataGenerator.GetRandomString(2), TestDataGenerator.GetRandomString(5)).Code);
        }

        [When(@"New culture is created")]
        public void WhenNewCultureIsCreated()
        {
            ScenarioContext.Current.Add("cultureCode", BrandTestHelper.CreateCulture(TestDataGenerator.GetRandomString(3), TestDataGenerator.GetRandomString(5)).Code);
        }

        [When(@"New brand currency is created")]
        public void WhenNewBrandCurrencyIsCreated()
        {
            ScenarioContext.Current.Add("currencyCode", BrandTestHelper.CreateCurrency(TestDataGenerator.GetRandomString(3), TestDataGenerator.GetRandomString(5)).Code);
        }

        [Then(@"Available brands are visible to me")]
        public void ThenAvailableBrandsAreVisibleToMe()
        {
            var result = AdminApiProxy.GetUserBrands();

            result.Should().NotBeNull();
            result.Brands.Should().NotBeEmpty();
        }

        [Then(@"Required data to add new brand is visible to me")]
        public void ThenRequiredDataToAddNewBrandIsVisibleToMe()
        {
            var result = AdminApiProxy.GetBrandAddData();

            result.Should().NotBeNull();
            result.Licensees.Should().NotBeEmpty();
            result.Types.Should().NotBeEmpty();
            result.TimeZones.Should().NotBeEmpty();
            result.PlayerActivationMethods.Should().NotBeEmpty();
        }

        [Then(@"Required data to edit that brand is visible to me")]
        public void ThenRequiredDataToEditThatBrandIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandEditData(brandId);

            result.Should().NotBeNull();
            result.Brand.Should().NotBeNull();
            result.Licensees.Should().NotBeEmpty();
            result.Types.Should().NotBeEmpty();
            result.TimeZones.Should().NotBeEmpty();
            result.PlayerActivationMethods.Should().NotBeEmpty();
        }

        [Then(@"I am forbidden to get brand edit data")]
        public void ThenIAmForbiddenToGetBrandEditData()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandEditData(brandId));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"Required brand data is visible to me")]
        public void ThenRequiredBrandDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandViewData(brandId);

            result.Should().NotBeNull();
        }

        [Then(@"I am forbidden to get brand view data")]
        public void ThenIAmForbiddenToGetBrandViewData()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandViewData(brandId));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"New brand is successfully added")]
        public void ThenNewBrandIsSuccessfullyAdded()
        {
            var licensee = BrandTestHelper.CreateLicensee();

            var data = new AddBrandRequest()
            {
                Code = TestDataGenerator.GetRandomString(),
                InternalAccounts = 1,
                EnablePlayerPrefix = true,
                PlayerPrefix = TestDataGenerator.GetRandomString(3),
                Licensee = licensee.Id,
                Name = TestDataGenerator.GetRandomString(),
                PlayerActivationMethod = PlayerActivationMethod.Automatic,
                TimeZoneId = TestDataGenerator.GetRandomTimeZone().Id,
                Type = BrandType.Integrated,
                Email = TestDataGenerator.GetRandomEmail(),
                SmsNumber = TestDataGenerator.GetRandomPhoneNumber(useDashes: false),
                WebsiteUrl = TestDataGenerator.GetRandomWebsiteUrl()
            };

            var result = AdminApiProxy.AddBrand(data);

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Then(@"Brand data is successfully edited")]
        public void ThenBrandDataIsSuccessfullyEdited()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var newBrandName = TestDataGenerator.GetRandomString(20);
            var newBrandCode = TestDataGenerator.GetRandomString(20);
            var newTimeZoneId = TestDataGenerator.GetRandomTimeZone().Id;
            var newInternalAccountCount = TestDataGenerator.GetRandomNumber(10, 0);
            const string remarks = "Test updating brand";

            var data = new EditBrandRequest
            {
                Brand = brandId,
                Code = newBrandCode,
                EnablePlayerPrefix = true,
                InternalAccounts = newInternalAccountCount,
                Licensee = licenseeId,
                Name = newBrandName,
                PlayerPrefix = "AAA",
                TimeZoneId = newTimeZoneId,
                Type = BrandType.Deposit,
                Remarks = remarks,
                Email = TestDataGenerator.GetRandomEmail(),
                SmsNumber = TestDataGenerator.GetRandomPhoneNumber(false),
                WebsiteUrl = TestDataGenerator.GetRandomWebsiteUrl()
            };

            var result = AdminApiProxy.EditBrand(data);

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Then(@"I am forbidden to edit brand")]
        public void ThenIAmForbiddenToEditBrand()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.EditBrand(new EditBrandRequest(){ Brand = brandId }));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"Brand countries are visible to me")]
        public void ThenBrandCountriesAreVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandCountries(brandId);

            result.Should().NotBeNull();
            result.Countries.Should().NotBeEmpty();
        }

        [Then(@"I am forbidden to get brand countries")]
        public void ThenIAmForbiddenToGetBrandCountries()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandCountries(brandId));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"Brand is successfully activated")]
        public void ThenBrandIsSuccessfullyActivated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var data = new ActivateBrandRequest
            {
                BrandId = brandId,
                Remarks = "Some remark"
            };

            var result = AdminApiProxy.ActivateBrand(data);

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Then(@"I am forbidden to activate brand")]
        public void ThenIAmForbiddenToActivateBrand()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.ActivateBrand(new ActivateBrandRequest(){ BrandId = brandId }));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"Brand is successfully deactivated")]
        public void ThenBrandIsSuccessfullyDeactivated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var data = new DeactivateBrandRequest
            {
                BrandId = brandId,
                Remarks = "Some remark"
            };

            var result = AdminApiProxy.DeactivateBrand(data);

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Then(@"I am forbidden to deactivate brand")]
        public void ThenIAmForbiddenToDeactivateBrand()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.DeactivateBrand(new DeactivateBrandRequest() { BrandId = brandId }));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"Brands are visible to me")]
        public void ThenBrandsAreVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var result = AdminApiProxy.GetBrands(false, new[] {licenseeId});

            result.Should().NotBeNull();
            result.Brands.Should().NotBeEmpty();
        }

        [Then(@"Brand country assign data is visible to me")]
        public void ThenBrandCountryAssignDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandCountryAssignData(brandId);

            result.Should().NotBeNull();
            result.AssignedCountries.Should().NotBeNull();
            result.AvailableCountries.Should().NotBeNull();
        }

        [Then(@"I am forbidden get brands countries assign data")]
        public void ThenIAmForbiddenGetBrandsCountriesAssignData()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandCountryAssignData(brandId));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"Brand country is successfully added")]
        public void ThenBrandCountryIsSuccessfullyAdded()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            ScenarioContext.Current.Should().ContainKey("countryCode");
            var code = ScenarioContext.Current.Get<string>("countryCode");

            var data = new AssignBrandCountryRequest()
            {
                Brand = brandId,
                Countries = new[] { code, "CA" }
            };

            var result = AdminApiProxy.AssignBrandCountry(data);

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Then(@"I am forbidden to assign country to the brand")]
        public void ThenIAmForbiddenToAssignCountryToTheBrand()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            ScenarioContext.Current.Should().ContainKey("countryCode");
            var code = ScenarioContext.Current.Get<string>("countryCode");

            var data = new AssignBrandCountryRequest()
            {
                Brand = brandId,
                Countries = new[] { code }
            };

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.AssignBrandCountry(data));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"Brand culture assign data is visible to me")]
        public void ThenBrandCultureAssignDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandCultureAssignData(brandId);

            result.Should().NotBeNull();
            result.DefaultCulture.Should().NotBeNullOrWhiteSpace();
            result.AssignedCultures.Should().NotBeNull();
            result.AvailableCultures.Should().NotBeNull();
        }

        [Then(@"I am forbidden get brands cultures assign data")]
        public void ThenIAmForbiddenGetBrandsCulturesAssignData()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandCultureAssignData(brandId));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"Brand culture is successfully added")]
        public void ThenBrandCultureIsSuccessfullyAdded()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var data = new AssignBrandCultureRequest()
            {
                Brand = brandId,
                Cultures = new[] { "en-CA", "en-US" },
                DefaultCulture = "en-CA"
            };

            var result = AdminApiProxy.AssignBrandCulture(data);

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Then(@"I am forbidden to assign culture to the brand")]
        public void ThenIAmForbiddenToAssignCultureToTheBrand()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            ScenarioContext.Current.Should().ContainKey("cultureCode");
            var code = ScenarioContext.Current.Get<string>("cultureCode");

            var data = new AssignBrandCultureRequest
            {
                Brand = brandId,
                Cultures = new[] { code },
                DefaultCulture = code
            };

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.AssignBrandCulture(data));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"Brand currencies are visible to me")]
        public void ThenBrandCurrenciesAreVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandCurrencies(brandId);

            result.Should().NotBeNull();
            result.CurrencyCodes.Should().NotBeNullOrEmpty();
        }

        [Then(@"I am forbidden get brand currencies")]
        public void ThenIAmForbiddenGetBrandCurrencies()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandCurrencies(brandId));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"Brand currencies with names are visible to me")]
        public void ThenBrandCurrenciesWithNamesAreVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandCurrenciesWithNames(brandId);

            result.Should().NotBeNull();
            result.CurrencyCodes.Should().NotBeNullOrEmpty();
        }

        [Then(@"I am forbidden get brand currencies with names")]
        public void ThenIAmForbiddenGetBrandCurrenciesWithNames()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandCurrenciesWithNames(brandId));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"Brand currency assign data is visible to me")]
        public void ThenBrandCurrencyAssignDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandCurrencyAssignData(brandId);

            result.Should().NotBeNull();
            result.DefaultCurrency.Should().NotBeNullOrWhiteSpace();
            result.BaseCurrency.Should().NotBeNullOrWhiteSpace();
            result.AssignedCurrencies.Should().NotBeNull();
            result.AvailableCurrencies.Should().NotBeNull();
        }

        [Then(@"I am forbidden get brand currency assign data")]
        public void ThenIAmForbiddenGetBrandCurrencyAssignData()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandCurrencyAssignData(brandId));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"Brand currency is successfully added")]
        public void ThenBrandCurrencyIsSuccessfullyAdded()
        {
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            BrandTestHelper.AssignLicenseeCurrency(licenseeId, "CAD");
            BrandTestHelper.AssignLicenseeCurrency(licenseeId, "USD");

            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var data = new AssignBrandCurrencyRequest
            {
                Brand = brandId,
                Currencies = new[] { "USD", "CAD" },
                BaseCurrency = "CAD",
                DefaultCurrency = "CAD"
            };

            var result = AdminApiProxy.AssignBrandCurrency(data);

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Then(@"I am forbidden to assign currency to the brand")]
        public void ThenIAmForbiddenToAssignCurrencyToTheBrand()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var data = new AssignBrandCurrencyRequest
            {
                Brand = brandId
            };

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.AssignBrandCurrency(data));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"Brand product assign data is visible to me")]
        public void ThenBrandProductAssignDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandProductAssignData(brandId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand product is successfully assigned")]
        public void ThenBrandProductIsSuccessfullyAssigned()
        {
            var licensee = BrandTestHelper.CreateLicensee();
            var brand = BrandTestHelper.CreateBrand(licensee, isActive: true);

            var data = new AssignBrandProductModel
            {
                Brand = brand.Id,
                Products = brand.Products.Select(b => b.BrandId.ToString()).ToArray()
            };

            var result = AdminApiProxy.AssignBrandProduct(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand product bet levels are visible to me")]
        public void ThenBrandProductBetLevelsAreVisibleToMe()
        {
            var licensee = BrandTestHelper.CreateLicensee();
            var brand = BrandTestHelper.CreateBrand(licensee, isActive: true);

            var result = AdminApiProxy.GetBrandProductBetLevels(brand.Id, brand.Products.First().ProductId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"New content translation is successfully created")]
        public void ThenNewContentTranslationIsSuccessfullyCreated()
        {
            var contentName = TestDataGenerator.GetRandomString();
            var contentSource = TestDataGenerator.GetRandomString();

            ScenarioContext.Current.Should().ContainKey("cultureCode");
            var cultureCode = ScenarioContext.Current.Get<string>("cultureCode");

            var translation = new AddContentTranslationData()
            {
                ContentName = contentName,
                ContentSource = contentSource,
                Language = cultureCode,
                Translation = TestDataGenerator.GetRandomString()
            };

            var data = new AddContentTranslationModel()
            {
                Languages = new List<string>(),
                Translations = new[] {translation},
                ContentName = contentName,
                ContentSource = contentSource
            };

            var result = AdminApiProxy.CreateContentTranslation(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"I am forbidden to execute permission protected brand methods with insufficient permissions")]
        public void ThenIAmForbiddenToExecutePermissionProtectedBrandMethodsWithInsufficientPermissions()
        {
            LogWithNewUser(Modules.VipLevelManager, Permissions.View);

            const int statusCode = (int)HttpStatusCode.Forbidden;

            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandAddData()).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandEditData(new Guid())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandViewData(new Guid())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandCountries(new Guid())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandCountryAssignData(new Guid())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandCultureAssignData(new Guid())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandCurrencies(new Guid())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandCurrenciesWithNames(new Guid())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.GetBrandCurrencyAssignData(new Guid())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.AddBrand(new AddBrandRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.EditBrand(new EditBrandRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.ActivateBrand(new ActivateBrandRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.DeactivateBrand(new DeactivateBrandRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.AssignBrandCountry(new AssignBrandCountryRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.AssignBrandCulture(new AssignBrandCultureRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.AssignBrandCurrency(new AssignBrandCurrencyRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.CreateContentTranslation(new AddContentTranslationModel())).GetHttpCode(), Is.EqualTo(statusCode));
        }

        [Then(@"I am unauthorized to execute brand methods with invalid token")]
        public void ThenIAmUnauthorizedToExecuteBrandMethodsWithInvalidToken()
        {
            const HttpStatusCode statusCode = HttpStatusCode.Unauthorized;

            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetUserBrands()).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetBrandAddData()).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetBrandEditData(new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetBrandViewData(new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.AddBrand(new AddBrandRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.EditBrand(new EditBrandRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetBrandCountries(new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.ActivateBrand(new ActivateBrandRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.DeactivateBrand(new DeactivateBrandRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetBrands(false, new[] { new Guid() })).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetBrandCountryAssignData(new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.AssignBrandCountry(new AssignBrandCountryRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.AssignBrandCulture(new AssignBrandCultureRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.AssignBrandCurrency(new AssignBrandCurrencyRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetBrandCurrencies(new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetBrandCurrenciesWithNames(new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetBrandCurrencyAssignData(new Guid())).StatusCode, Is.EqualTo(statusCode));
        }

        [Then(@"I am not allowed to execute brand methods using GET")]
        public void ThenIAmNotAllowedToExecuteBrandMethodsUsingGET()
        {
            const HttpStatusCode statusCode = HttpStatusCode.MethodNotAllowed;

            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<AddBrandResponse>(AdminApiRoutes.AddBrand, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<EditBrandResponse>(AdminApiRoutes.EditBrand, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<ActivateBrandResponse>(AdminApiRoutes.ActivateBrand, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<DeactivateBrandResponse>(AdminApiRoutes.DeactivateBrand, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<AssignBrandCountryResponse>(AdminApiRoutes.AssignBrandCountry, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<AssignBrandCultureRequest>(AdminApiRoutes.AssignBrandCulture, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<AssignBrandCurrencyRequest>(AdminApiRoutes.AssignBrandCurrency, string.Empty)).StatusCode, Is.EqualTo(statusCode));
        }

        [Then(@"I am not allowed to execute brand methods using POST")]
        public void ThenIAmNotAllowedToExecuteBrandMethodsUsingPOST()
        {
            const HttpStatusCode statusCode = HttpStatusCode.MethodNotAllowed;

            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonPost<object, UserBrandsResponse>(AdminApiRoutes.GetUserBrands, new object())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonPost<object, BrandAddDataResponse>(AdminApiRoutes.GetBrandAddData, new object())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonPost<Guid, BrandEditDataResponse>(AdminApiRoutes.GetBrandEditData + "?id=", new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonPost<Guid, BrandEditDataResponse>(AdminApiRoutes.GetBrandViewData + "?id=", new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonPost<Guid, BrandCountriesResponse>(AdminApiRoutes.GetBrandCountries + "?brandId=", new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonPost<object, BrandsResponse>(AdminApiRoutes.GetBrands + "?useFilter=&licensees=", new object())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonPost<Guid, BrandCountryAssignDataResponse>(AdminApiRoutes.GetBrandCountryAssignData + "?brandId=", new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonPost<Guid, BrandCultureAssignDataResponse>(AdminApiRoutes.GetBrandCultureAssignData + "?brandId=", new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonPost<Guid, BrandCurrencyAssignDataResponse>(AdminApiRoutes.GetBrandCurrencyAssignData + "?brandId=", new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonPost<Guid, GetBrandCurrenciesResponse>(AdminApiRoutes.GetBrandCurrencies + "?brandId=", new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonPost<Guid, GetBrandCurrenciesWithNamesResponse>(AdminApiRoutes.GetBrandCurrenciesWithNames + "?brandId=", new Guid())).StatusCode, Is.EqualTo(statusCode));
        }
    }
}