using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Data.Security.Roles;
using AFT.RegoV2.Core.Common.Data.Security.Users;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations;
using AFT.RegoV2.Core.Security.ApplicationServices.IpRegulations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Helpers;
using AutoMapper;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Brand = AFT.RegoV2.Core.Brand.Interface.Data.Brand;
using TransactionType = AFT.RegoV2.Core.Common.Data.Player.TransactionType;

namespace AFT.RegoV2.AdminApi.Tests.Integration.Steps
{
    public class AdminSteps : BaseSteps
    {
        private BrandTestHelper BrandHelper { get; set; }
        private PaymentTestHelper PaymentHelper { get; set; }
        private SecurityTestHelper SecurityTestHelper { get; set; }
        private BackendIpRegulationService BackendIpRegulationService { get; set; }
        private BrandIpRegulationService BrandIpRegulationService { get; set; }

        public AdminSteps()
        {
            SecurityTestHelper = Container.Resolve<SecurityTestHelper>();
            SecurityTestHelper.SignInClaimsSuperAdmin();
            BrandHelper = Container.Resolve<BrandTestHelper>();
            PaymentHelper = Container.Resolve<PaymentTestHelper>();
            BackendIpRegulationService = Container.Resolve<BackendIpRegulationService>();
            BrandIpRegulationService = Container.Resolve<BrandIpRegulationService>();
        }

        [When(@"New country is created for admin")]
        public void WhenNewCountryIsCreatedForAdmin()
        {
            ScenarioContext.Current.Add("countryCode", BrandHelper.CreateCountry(TestDataGenerator.GetRandomString(2), TestDataGenerator.GetRandomString(5)).Code);
        }

        [When(@"New culture is created for admin")]
        public void WhenNewCultureIsCreatedForAdmin()
        {
            ScenarioContext.Current.Add("cultureCode", BrandHelper.CreateCulture(TestDataGenerator.GetRandomString(3), TestDataGenerator.GetRandomString(5)).Code);
        }

        [When(@"New (.*) user is created")]
        public void WhenNewUserIsCreated(string activationStatus)
        {
            var isActive = activationStatus.Equals("activated");

            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var user = SecurityTestHelper.CreateAdmin(licenseeId, new[] { new Brand { Id = brandId } }, isActive: isActive);

            ScenarioContext.Current.Add("userId", user.Id);
        }

        //[Then(@"Available countries are visible to me")]
        //public void ThenAvailableCountriesAreVisibleToMe()
        //{
        //    var searchPackage = new SearchPackage
        //    {
        //        PageIndex = 0,
        //        RowCount = 10,
        //        SortASC = true,
        //        SortColumn = "",
        //        SortSord = "",
        //        SingleFilter = null,
        //        AdvancedFilter = null
        //    };

        //    var result = AdminApiProxy.GetCountriesList(searchPackage);

        //    result.Should().NotBeNull();
        //    result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        //}

        [Then(@"Country by code is visible to me")]
        public void ThenCountryByCodeIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("countryCode");
            var countryCode = ScenarioContext.Current.Get<string>("countryCode");

            var result = AdminApiProxy.GetCountryByCode(countryCode);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Country data is successfully saved")]
        public void ThenCountryDataIsSuccessfullySaved()
        {
            var data = new EditCountryData
            {
                Code = TestDataGenerator.GetRandomString(3),
                Name = TestDataGenerator.GetRandomString(),
            };

            var result = AdminApiProxy.SaveCountry(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Country is successfully deleted")]
        public void ThenCountryIsSuccessfullyDeleted()
        {
            ScenarioContext.Current.Should().ContainKey("countryCode");
            var countryCode = ScenarioContext.Current.Get<string>("countryCode");

            var data = new DeleteCountryData() {Code = countryCode};

            var result = AdminApiProxy.DeleteCountry(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        //[Then(@"Available cultures are visible to me")]
        //public void ThenAvailableCulturesAreVisibleToMe()
        //{
        //    ScenarioContext.Current.Pending();
        //}

        [Then(@"Culture by code is visible to me")]
        public void ThenCultureByCodeIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("cultureCode");
            var cultureCode = ScenarioContext.Current.Get<string>("cultureCode");

            var result = AdminApiProxy.GetCultureByCode(cultureCode);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Culture is successfully activated")]
        public void ThenCultureIsSuccessfullyActivated()
        {
            ScenarioContext.Current.Should().ContainKey("cultureCode");
            var cultureCode = ScenarioContext.Current.Get<string>("cultureCode");

            var deactivateCulturedata = new DeactivateCultureData()
            {
                Code = cultureCode,
                Remarks = TestDataGenerator.GetRandomString()
            };

            AdminApiProxy.DeactivateCulture(deactivateCulturedata);

            var activateCulturedata = new ActivateCultureData()
            {
                Code = cultureCode, 
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.ActivateCulture(activateCulturedata);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Culture is successfully deactivated")]
        public void ThenCultureIsSuccessfullyDeactivated()
        {
            ScenarioContext.Current.Should().ContainKey("cultureCode");
            var cultureCode = ScenarioContext.Current.Get<string>("cultureCode");

            var data = new DeactivateCultureData()
            {
                Code = cultureCode,
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.DeactivateCulture(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Culture data is successfully saved")]
        public void ThenCultureDataIsSuccessfullySaved()
        {
            var data = new EditCultureData
            {
                Code = TestDataGenerator.GetRandomString(3),
                Name = TestDataGenerator.GetRandomString(),
                NativeName = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.SaveCulture(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [When(@"New currency is created")]
        public void WhenNewCurrencyIsCreated()
        {
            ScenarioContext.Current.Add("currencyCode", PaymentHelper.CreateCurrency(TestDataGenerator.GetRandomString(3), TestDataGenerator.GetRandomString(5)).Code);
        }

        [Then(@"Currency by code is visible to me")]
        public void ThenCurrencyByCodeIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("currencyCode");
            var currencyCode = ScenarioContext.Current.Get<string>("currencyCode");

            var result = AdminApiProxy.GetCurrencyByCode(currencyCode);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Currency is successfully activated")]
        public void ThenCurrencyIsSuccessfullyActivated()
        {
            ScenarioContext.Current.Should().ContainKey("currencyCode");
            var currencyCode = ScenarioContext.Current.Get<string>("currencyCode");

            var data = new ActivateCurrencyData()
            {
                Code = currencyCode,
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.ActivateCurrency(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Currency is successfully deactivated")]
        public void ThenCurrencyIsSuccessfullyDeactivated()
        {
            ScenarioContext.Current.Should().ContainKey("currencyCode");
            var currencyCode = ScenarioContext.Current.Get<string>("currencyCode");

            var activateCurrencyData = new ActivateCurrencyData()
            {
                Code = currencyCode,
                Remarks = TestDataGenerator.GetRandomString()
            };

            AdminApiProxy.ActivateCurrency(activateCurrencyData);

            var deactivateCurrencyData = new DeactivateCurrencyData()
            {
                Code = currencyCode,
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.DeactivateCurrency(deactivateCurrencyData);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Currency data is successfully saved")]
        public void ThenCurrencyDataIsSuccessfullySaved()
        {
            var data = new EditCurrencyData
            {
                Code = TestDataGenerator.GetRandomAlphabeticString(3),
                Name = TestDataGenerator.GetRandomAlphabeticString(7),
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.SaveCurrency(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"New user is successfully created")]
        public void ThenNewUserIsSuccessfullyCreated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var data = new AddAdminData
            {
                Username = TestDataGenerator.GetRandomString(),
                FirstName = "User",
                LastName = "123",
                Password = "Password123",
                Language = "English",
                IsActive = true,
                AssignedLicensees = new[] { licenseeId },
                AllowedBrands = new[] { brandId },
                Currencies = new[] { "CAD" },
                RoleId = SecurityTestHelper.CreateRole().Id
            };

            var result = AdminApiProxy.CreateUserInAdminManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"User is successfully updated")]
        public void ThenUserIsSuccessfullyUpdated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var user = SecurityTestHelper.CreateAdmin(licenseeId, new[] { new Brand(){ Id = brandId } });
            user.FirstName = TestDataGenerator.GetRandomString();

            var data = Mapper.DynamicMap<EditAdminData>(user);

            var result = AdminApiProxy.UpdateUserInAdminManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"User password is successfully reset")]
        public void ThenUserPasswordIsSuccessfullyReset()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var user = SecurityTestHelper.CreateAdmin(licenseeId, new[] { new Brand() { Id = brandId } });

            var data = Mapper.DynamicMap<AddAdminData>(user);
            data.Password = TestDataGenerator.GetRandomString();
            data.PasswordConfirmation = data.Password;

            var result = AdminApiProxy.ResetPasswordInAdminManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"User is successfully activated")]
        public void ThenUserIsSuccessfullyActivated()
        {
            ScenarioContext.Current.Should().ContainKey("userId");
            var userId = ScenarioContext.Current.Get<Guid>("userId");

            var data = new ActivateUserData()
            {
                Id = userId
            };

            var result = AdminApiProxy.ActivateUserInAdminManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"User is successfully deactivated")]
        public void ThenUserIsSuccessfullyDeactivated()
        {
            ScenarioContext.Current.Should().ContainKey("userId");
            var userId = ScenarioContext.Current.Get<Guid>("userId");

            var data = new DeactivateUserData()
            {
                Id = userId
            };

            var result = AdminApiProxy.DeactivateUserInAdminManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"User edit data is visible to me")]
        public void ThenUserEditDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("userId");
            var userId = ScenarioContext.Current.Get<Guid>("userId");

            var result = AdminApiProxy.GetUserEditDataInAdminManager(userId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Licensee data is visible to me")]
        public void ThenLicenseeDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");
            
            var getLicenseeData = new GetLicenseeData();
            getLicenseeData.Licensees.Add(licenseeId);
            var result = AdminApiProxy.GetLicenseeDataInAdminManager(getLicenseeData);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand filter selection is successfully saved")]
        public void ThenBrandFilterSelectionIsSuccessfullySaved()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var data = new BrandFilterSelectionData()
            {
                Brands = new[] { brandId }
            };

            var result = AdminApiProxy.SaveBrandFilterSelectionInAdminManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Licensee filter selection is successfully saved")]
        public void ThenLicenseeFilterSelectionIsSuccessfullySaved()
        {
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var data = new LicenseeFilterSelectionData()
            {
                Licensees = new[] { licenseeId }
            };

            var result = AdminApiProxy.SaveLicenseeFilterSelectionInAdminManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"New role is successfully created")]
        public void ThenNewRoleIsSuccessfullyCreated()
        {
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var data = new AddRoleData
            {
                Code = "Role-" + TestDataGenerator.GetRandomString(5),
                Name = "Role-" + TestDataGenerator.GetRandomString(5),
                Description = TestDataGenerator.GetRandomString(),
                AssignedLicensees = new[] { licenseeId },
                CheckedPermissions = Container.Resolve<IAuthQueries>().GetPermissions().Select(p => p.Id).ToList()
            };

            var result = AdminApiProxy.CreateRoleInRoleManager(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [When(@"New role is created")]
        public void WhenNewRoleIsCreated()
        {
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var role = SecurityTestHelper.CreateRole(new[] {licenseeId});

            ScenarioContext.Current.Add("roleId", role.Id);
        }

        [Then(@"Role is visible to me")]
        public void ThenRoleIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("roleId");
            var roleId = ScenarioContext.Current.Get<Guid>("roleId");

            var result = AdminApiProxy.GetRoleInRoleManager(roleId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Role edit data is visible to me")]
        public void ThenRoleEditDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("roleId");
            var roleId = ScenarioContext.Current.Get<Guid>("roleId");

            var result = AdminApiProxy.GetEditDataInRoleManager(roleId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Role is successfully updated")]
        public void ThenRoleIsSuccessfullyUpdated()
        {
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var role = SecurityTestHelper.CreateRole(new[] { licenseeId });

            var roleData = Mapper.DynamicMap<EditRoleData>(role);
            roleData.Description = TestDataGenerator.GetRandomString();
            roleData.CheckedPermissions = Container.Resolve<IAuthQueries>().GetPermissions().Select(p => p.Id).ToList();

            var result = AdminApiProxy.UpdateRoleInRoleManager(roleData);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Role manager licensee data is visible to me")]
        public void ThenRoleManagerLicenseeDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var result = AdminApiProxy.GetLicenseeDataInRoleManager(new[] { licenseeId });

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"New identification settings is successfully created")]
        public void ThenNewIdentificationSettingsIsSuccessfullyCreated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");
            ScenarioContext.Current.Should().ContainKey("bankAccountId");
            var bankAccountId = ScenarioContext.Current.Get<Guid>("bankAccountId");

            var data = new IdentificationDocumentSettingsData
            {
                LicenseeId = licenseeId,
                BrandId = brandId,
                TransactionType = TransactionType.Deposit,
                PaymentGatewayBankAccountId = bankAccountId,
                PaymentGatewayMethod = PaymentMethod.OfflineBank,
                IdFront = true,
                IdBack = true,
                CreditCardFront = true,
                CreditCardBack = true,
                POA = true,
                DCF = true,
                Remark = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.CreateSettingInIdentificationDocumentSettings(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [When(@"New identification settings is created")]
        public void WhenNewIdentificationSettingsIsCreated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");
            ScenarioContext.Current.Should().ContainKey("bankAccountId");
            var bankAccountId = ScenarioContext.Current.Get<Guid>("bankAccountId");

            var data = new IdentificationDocumentSettingsData
            {
                LicenseeId = licenseeId,
                BrandId = brandId,
                TransactionType = TransactionType.Deposit,
                PaymentGatewayBankAccountId = bankAccountId,
                PaymentGatewayMethod = PaymentMethod.OfflineBank,
                IdFront = true,
                IdBack = true,
                CreditCardFront = true,
                CreditCardBack = true,
                POA = true,
                DCF = true,
                Remark = TestDataGenerator.GetRandomString()
            };

            var identificationDocumentSettings = Container.Resolve<IdentificationDocumentSettingsService>().CreateSetting(data);

            ScenarioContext.Current.Add("identificationDocumentSettingsId", identificationDocumentSettings.Id);
        }

        [Then(@"Identification settings edit data is visible to me")]
        public void ThenIdentificationSettingsEditDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("identificationDocumentSettingsId");
            var identificationDocumentSettingsId = ScenarioContext.Current.Get<Guid>("identificationDocumentSettingsId");

            var result = AdminApiProxy.GetEditDataInIdentificationDocumentSettings(identificationDocumentSettingsId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Identification settings is successfully updated")]
        public void ThenIdentificationSettingsIsSuccessfullyUpdated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");
            ScenarioContext.Current.Should().ContainKey("bankAccountId");
            var bankAccountId = ScenarioContext.Current.Get<Guid>("bankAccountId");

            var data = new IdentificationDocumentSettingsData
            {
                LicenseeId = licenseeId,
                BrandId = brandId,
                TransactionType = TransactionType.Deposit,
                PaymentGatewayMethod = PaymentMethod.OfflineBank,
                PaymentGatewayBankAccountId = bankAccountId,
                IdFront = true,
                IdBack = true,
                CreditCardFront = true,
                CreditCardBack = true,
                POA = true,
                DCF = true,
                Remark = TestDataGenerator.GetRandomString()
            };

            var identificationDocumentSettings = Container.Resolve<IdentificationDocumentSettingsService>().CreateSetting(data);

            var commandData = Mapper.DynamicMap<IdentificationDocumentSettingsData>(identificationDocumentSettings);
            commandData.Remark = TestDataGenerator.GetRandomString();

            var result = AdminApiProxy.UpdateSettingInIdentificationDocumentSettings(commandData);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Identification document settings licensee brands data is visible to me")]
        public void ThenIdentificationDocumentSettingsLicenseeBrandsDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var result = AdminApiProxy.GetLicenseeBrandsInIdentificationDocumentSettings(licenseeId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Submitted ip address is unique")]
        public void ThenSubmittedIpAddressIsUnique()
        {
            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            var result = AdminApiProxy.IsIpAddressUniqueInAdminIpRegulations(ipAddress);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo("true");
        }

        [Then(@"Submitted ip address batch is unique")]
        public void ThenSubmittedIpAddressBatchIsUnique()
        {
            const string ipAddressBatch = "192.168.5.17-25";

            var result = AdminApiProxy.IsIpAddressBatchUniqueInAdminIpRegulations(ipAddressBatch);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo("true");
        }

        [When(@"New admin ip regulation is created")]
        public void WhenNewAdminIpRegulationIsCreated()
        {
            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            var data = new AddBackendIpRegulationData
            {
                IpAddress = ipAddress
            };

            var adminIpRegulation = BackendIpRegulationService.CreateIpRegulation(data);

            ScenarioContext.Current.Add("adminIpRegulationId", adminIpRegulation.Id);
        }

        [Then(@"Admin ip regulation edit data is visible to me")]
        public void ThenAdminIpRegulationEditDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("adminIpRegulationId");
            var adminIpRegulationId = ScenarioContext.Current.Get<Guid>("adminIpRegulationId");

            var result = AdminApiProxy.GetEditDataInAdminIpRegulations(adminIpRegulationId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"New admin ip regulation is successfully created")]
        public void ThenNewAdminIpRegulationIsSuccessfullyCreated()
        {
            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            var data = new EditAdminIpRegulationData
            {
                IpAddress = ipAddress,
                Description = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.CreateIpRegulationInAdminIpRegulations(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Admin ip regulation is successfully updated")]
        public void ThenAdminIpRegulationIsSuccessfullyUpdated()
        {
            ScenarioContext.Current.Should().ContainKey("adminIpRegulationId");
            var adminIpRegulationId = ScenarioContext.Current.Get<Guid>("adminIpRegulationId");

            var data = new EditAdminIpRegulationData
            {
                Id = adminIpRegulationId,
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                Description = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.UpdateIpRegulationInAdminIpRegulations(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Admin ip regulation is successfully deleted")]
        public void ThenAdminIpRegulationIsSuccessfullyDeleted()
        {
            ScenarioContext.Current.Should().ContainKey("adminIpRegulationId");
            var adminIpRegulationId = ScenarioContext.Current.Get<Guid>("adminIpRegulationId");

            var data = new DeleteAdminIpRegulationData()
            {
                Id = adminIpRegulationId
            };

            var result = AdminApiProxy.DeleteIpRegulationInAdminIpRegulations(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Submitted brand ip address is unique")]
        public void ThenSubmittedBrandIpAddressIsUnique()
        {
            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            var result = AdminApiProxy.IsIpAddressUniqueInBrandIpRegulations(ipAddress);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo("true");
        }

        [Then(@"Submitted brand ip address batch is unique")]
        public void ThenSubmittedBrandIpAddressBatchIsUnique()
        {
            const string ipAddressBatch = "192.168.5.17-25";

            var result = AdminApiProxy.IsIpAddressBatchUniqueInBrandIpRegulations(ipAddressBatch);

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo("true");
        }

        [Then(@"Licensee brands are visible to me")]
        public void ThenLicenseeBrandsAreVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var result = AdminApiProxy.GetLicenseeBrandsInBrandIpRegulations(licenseeId, false);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [When(@"New brand ip regulation is created")]
        public void WhenNewBrandIpRegulationIsCreated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var data = new AddBrandIpRegulationData
            {
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                BrandId = brandId,
                LicenseeId = licenseeId,
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = "google.com"
            };

            var brandIpRegulation = BrandIpRegulationService.CreateIpRegulation(data);

            ScenarioContext.Current.Add("brandIpRegulationId", brandIpRegulation.Id);
        }

        [Then(@"Brand ip regulation edit data is visible to me")]
        public void ThenBrandIpRegulationEditDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandIpRegulationId");
            var brandIpRegulationId = ScenarioContext.Current.Get<Guid>("brandIpRegulationId");

            var result = AdminApiProxy.GetEditDataInBrandIpRegulations(brandIpRegulationId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"New brand ip regulation is successfully created")]
        public void ThenNewBrandIpRegulationIsSuccessfullyCreated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var data = new AddBrandIpRegulationData
            {
                BrandId = brandId,
                LicenseeId = licenseeId,
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                Description = TestDataGenerator.GetRandomString(),
                AssignedBrands = new []{ brandId }
            };

            var result = AdminApiProxy.CreateIpRegulationInBrandIpRegulations(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand ip regulation is successfully updated")]
        public void ThenBrandIpRegulationIsSuccessfullyUpdated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");
            ScenarioContext.Current.Should().ContainKey("brandIpRegulationId");
            var brandIpRegulationId = ScenarioContext.Current.Get<Guid>("brandIpRegulationId");

            var data = new RegoV2.Core.Common.Data.Admin.EditBrandIpRegulationData
            {
                Id = brandIpRegulationId,
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                Description = TestDataGenerator.GetRandomString(),
                BrandId = brandId,
                LicenseeId = licenseeId
            };

            var result = AdminApiProxy.UpdateIpRegulationInBrandIpRegulations(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand ip regulation is successfully deleted")]
        public void ThenBrandIpRegulationIsSuccessfullyDeleted()
        {
            ScenarioContext.Current.Should().ContainKey("brandIpRegulationId");
            var brandIpRegulationId = ScenarioContext.Current.Get<Guid>("brandIpRegulationId");

            var data = new DeleteBrandIpRegulationData()
            {
                Id = brandIpRegulationId
            };

            var result = AdminApiProxy.DeleteIpRegulationInBrandIpRegulations(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"I can not execute protected admin methods with insufficient permissions")]
        public void ThenICanNotExecuteProtectedAdminMethodsWithInsufficientPermissions()
        {
            LogWithNewUser(Modules.VipLevelManager, Permissions.View);

            const int statusCode = (int)HttpStatusCode.Forbidden;

            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.SaveCountry(new EditCountryData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.DeleteCountry(new DeleteCountryData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.ActivateCulture(new ActivateCultureData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.DeactivateCulture(new DeactivateCultureData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.SaveCulture(new EditCultureData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.ActivateCurrency(new ActivateCurrencyData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.DeactivateCulture(new DeactivateCultureData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.SaveCurrency(new EditCurrencyData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.CreateUserInAdminManager(new AddAdminData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.UpdateUserInAdminManager(new EditAdminData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.ResetPasswordInAdminManager(new AddAdminData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.ActivateUserInAdminManager(new ActivateUserData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.DeactivateUserInAdminManager(new DeactivateUserData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.GetLicenseeDataInAdminManager(new GetLicenseeData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.CreateRoleInRoleManager(new AddRoleData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.UpdateRoleInRoleManager(new EditRoleData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.GetLicenseeDataInRoleManager(new List<Guid>())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.CreateSettingInIdentificationDocumentSettings(new IdentificationDocumentSettingsData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.UpdateSettingInIdentificationDocumentSettings(new IdentificationDocumentSettingsData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.CreateIpRegulationInAdminIpRegulations(new EditAdminIpRegulationData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.UpdateIpRegulationInAdminIpRegulations(new EditAdminIpRegulationData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.DeleteIpRegulationInAdminIpRegulations(new DeleteAdminIpRegulationData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.CreateIpRegulationInBrandIpRegulations(new AddBrandIpRegulationData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.UpdateIpRegulationInBrandIpRegulations(new RegoV2.Core.Common.Data.Admin.EditBrandIpRegulationData())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.DeleteIpRegulationInBrandIpRegulations(new DeleteBrandIpRegulationData())).GetHttpCode(), Is.EqualTo(statusCode));
        }
    }
}