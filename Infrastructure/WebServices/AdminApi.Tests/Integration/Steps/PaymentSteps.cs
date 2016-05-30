using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Payment;
using AFT.RegoV2.AdminApi.Interface.Proxy;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Base;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Helpers;
using AutoMapper;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Bank = AFT.RegoV2.Core.Payment.Data.Bank;

namespace AFT.RegoV2.AdminApi.Tests.Integration.Steps
{
    public class PaymentSteps : BaseSteps
    {
        #region fields
        private BrandTestHelper BrandTestHelper { get; set; }
        private SecurityTestHelper SecurityTestHelper { get; set; }
        private PaymentTestHelper PaymentTestHelper { get; set; }
        private PlayerTestHelper PlayerHelper { get; set; }
        private const string KeyPaymentGatewaySettingsId = "KeyPaymentGatewaySettingsId";
        private const string KeyPaymentGatewaySettingsData = "KeyPaymentGatewaySettingsData";
        private const string KeyPaymentGatewaySettingsRequest = "KeyPaymentGatewaySettingsRequest";
        private const string KeyValidationResponseBase = "KeyValidationResponseBase";
        private const string KeyOnlineDepositData = "KeyOnlineDepositData";

        private const string KeyBankId = "KeyBankId";
        private const string KeyBankData = "KeyBankData";
        private const string KeyBankAccountId = "KeyBankAccountId";
        private const string KeyBankAccountData = "KeyBankAccountData";
        private const string KeyPlayerBankAccountId = "KeyPlayerBankAccountId";
        private const string KeyPlayerBankAccountData = "KeyPlayerBankAccountData";
        private const string KeyOfflineDepositId = "KeyOfflineDepositId";
        private const string KeyOfflineDepositData = "KeyOfflineDepositData";

        private readonly Guid _notExistId = new Guid("48CFD8CF-C42C-4273-82D2-F8E7A1BA844A");
        protected readonly Guid DefaultBrandId = new Guid("00000000-0000-0000-0000-000000000138");
        protected readonly Guid DefaultLicenseeId = new Guid("4A557EA9-E6B7-4F1F-AEE5-49E170ADB7E0");
        protected readonly Guid BankSE45Id = Guid.Parse("4f299e19-ecd0-4095-b61b-e6945374fd88");
        protected readonly Guid BankAccountTypeVIPId =new Guid("00000000-0000-0000-0000-000000000003");
        #endregion
        public PaymentSteps()
        {
            SecurityTestHelper = Container.Resolve<SecurityTestHelper>();
            SecurityTestHelper.SignInClaimsSuperAdmin();
            PaymentTestHelper = Container.Resolve<PaymentTestHelper>();
            BrandTestHelper = Container.Resolve<BrandTestHelper>();
            PlayerHelper = Container.Resolve<PlayerTestHelper>();

            Mapper.CreateMap<PaymentGatewaySettings, SavePaymentGatewaySettingsRequest>()
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src =>src.BrandId ));

            Mapper.CreateMap<Bank, EditBankRequest>();                

            Mapper.CreateMap<BankAccount, EditBankAccountRequest>()
                .ForMember(dest => dest.Bank, opt => opt.MapFrom(src => src.Bank.Id))
                .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => src.AccountType.Id));
        }

        [Given(@"New user with (.*) permission in (.*) module login for payment")]
        public void GivenNewUserWithSufficientPermissionsLoginForPayment(string permissionName, string module)
        {
            LogWithNewUser(module, permissionName);         
        }

        [Given(@"I am logged in with insufficientPermissions for payment")]
        public void GivenIAmLoggedInWithInsufficientPermissionsForPayment()
        {
            LogWithNewUser(Modules.AdminActivityLog, Permissions.View);
        }

        [When(@"New player is created with default brand")]
        public void WhenNewPlayerIsCreatedWithDefaultBrand()
        {
            var brandId = DefaultBrandId;
            var player = PlayerHelper.CreatePlayer(true, brandId);
            ScenarioContext.Current.Add("playerId", player.Id);
        }

        [Then(@"I can not execute protected payment methods with insufficient permissions")]
        public void ThenICatNotExecuteProtectedPaymentMethodsWithInsufficientPermissions()
        {
            const int statusCode = (int)HttpStatusCode.Forbidden;
            //TODO:ONLINEDEPOSIT check PaymentGatewaySettings.List

            Assert.That(Assert.Throws<HttpException>(
                () => AdminApiProxy.AddPaymentGatewaySettings(new SavePaymentGatewaySettingsRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(
                () => AdminApiProxy.EditPaymentGatewaySettings(new SavePaymentGatewaySettingsRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(
                () => AdminApiProxy.ActivatePaymentGatewaySettings(new ActivatePaymentGatewaySettingsRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(
                () => AdminApiProxy.DeactivatePaymentGatewaySettings(new DeactivatePaymentGatewaySettingsRequest())).GetHttpCode(), Is.EqualTo(statusCode));

            Assert.That(Assert.Throws<HttpException>(
                () => AdminApiProxy.RejectOnlineDeposit(new RejectOnlineDepositRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(
                () => AdminApiProxy.ApproveOnlineDeposit(new ApproveOnlineDepositRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(
                () => AdminApiProxy.VerifyOnlineDeposit(new VerifyOnlineDepositRequest())).GetHttpCode(), Is.EqualTo(statusCode));

            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.AddBank(new AddBankRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.EditBank(new EditBankRequest())).GetHttpCode(), Is.EqualTo(statusCode));

            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.AddBankAccount(new AddBankAccountRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.EditBankAccount(new EditBankAccountRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.ActivateBankAccount(new ActivateBankAccountRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.DeactivateBankAccount(new DeactivateBankAccountRequest())).GetHttpCode(), Is.EqualTo(statusCode));

            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.VerifyPlayerBankAccount(new VerifyPlayerBankAccountRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.RejectPlayerBankAccount(new RejectPlayerBankAccountRequest())).GetHttpCode(), Is.EqualTo(statusCode));

            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.CreateOfflineDeposit(new CreateOfflineDepositRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.ConfirmOfflineDeposit(new ConfirmOfflineDepositRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.VerifyOfflineDeposit(new VerifyOfflineDepositRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.UnverifyOfflineDeposit(new UnverifyOfflineDepositRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.ApproveOfflineDeposit(new ApproveOfflineDepositRequest())).GetHttpCode(), Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<HttpException>(() => AdminApiProxy.RejectOfflineDeposit(new RejectOfflineDepositRequest())).GetHttpCode(), Is.EqualTo(statusCode));
        }

        [Then(@"I am unauthorized to execute payment methods with invalid token")]
        public void ThenIAmUnauthorizedToExecutePaymentMethodsWithInvalidToken()
        {
            const HttpStatusCode statusCode = HttpStatusCode.Unauthorized;
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.AddPaymentGatewaySettings(new SavePaymentGatewaySettingsRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.EditPaymentGatewaySettings(new SavePaymentGatewaySettingsRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.ActivatePaymentGatewaySettings(new ActivatePaymentGatewaySettingsRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.DeactivatePaymentGatewaySettings(new DeactivatePaymentGatewaySettingsRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetPaymentGatewaySettingsById(new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetOnlineDepositById(new Guid())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.VerifyOnlineDeposit(new VerifyOnlineDepositRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.ApproveOnlineDeposit(new ApproveOnlineDepositRequest())).StatusCode, Is.EqualTo(statusCode));

            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.AddBank(new AddBankRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.EditBank(new EditBankRequest())).StatusCode, Is.EqualTo(statusCode));

            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.AddBankAccount(new AddBankAccountRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.EditBankAccount(new EditBankAccountRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.ActivateBankAccount(new ActivateBankAccountRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.DeactivateBankAccount(new DeactivateBankAccountRequest())).StatusCode, Is.EqualTo(statusCode));

            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.VerifyPlayerBankAccount(new VerifyPlayerBankAccountRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.RejectPlayerBankAccount(new RejectPlayerBankAccountRequest())).StatusCode, Is.EqualTo(statusCode));

            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CreateOfflineDeposit(new CreateOfflineDepositRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.ConfirmOfflineDeposit(new ConfirmOfflineDepositRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.VerifyOfflineDeposit(new VerifyOfflineDepositRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.UnverifyOfflineDeposit(new UnverifyOfflineDepositRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.ApproveOfflineDeposit(new ApproveOfflineDepositRequest())).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.RejectOfflineDeposit(new RejectOfflineDepositRequest())).StatusCode, Is.EqualTo(statusCode));
        }
  
        [Then(@"I am not allowed to execute payment methods using GET")]
        public void ThenIAmNotAllowedToExecutePaymentMethodsUsingGET()
        {
            const HttpStatusCode statusCode = HttpStatusCode.MethodNotAllowed;            
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<SavePaymentGatewaySettingsResponse>(AdminApiRoutes.AddPaymentGatewaySettings, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<SavePaymentGatewaySettingsResponse>(AdminApiRoutes.EditPaymentGatewaySettings, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<ActivatePaymentGatewaySettingsResponse>(AdminApiRoutes.ActivatePaymentGatewaySettings, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<DeactivatePaymentGatewaySettingsResponse>(AdminApiRoutes.DeactivatePaymentGatewaySettings, string.Empty)).StatusCode, Is.EqualTo(statusCode));            
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<VerifyOnlineDepositResponse>(AdminApiRoutes.VerifyOnlineDeposit, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<RejectOnlineDepositResponse>(AdminApiRoutes.RejectOnlineDeposit, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<ApproveOnlineDepositResponse>(AdminApiRoutes.ApproveOnlineDeposit, string.Empty)).StatusCode, Is.EqualTo(statusCode));

            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<SaveBankResponse>(AdminApiRoutes.AddBank, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<SaveBankResponse>(AdminApiRoutes.EditBank, string.Empty)).StatusCode, Is.EqualTo(statusCode));

            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<SaveBankAccountResponse>(AdminApiRoutes.AddBankAccount, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<SaveBankAccountResponse>(AdminApiRoutes.EditBankAccount, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<ActivateBankAccountResponse>(AdminApiRoutes.ActivateBankAccount, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<DeactivateBankAccountResponse>(AdminApiRoutes.DeactivateBankAccount, string.Empty)).StatusCode, Is.EqualTo(statusCode));

            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<ValidationResponseBase>(AdminApiRoutes.VerifyPlayerBankAccount, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<ValidationResponseBase>(AdminApiRoutes.RejectPlayerBankAccount, string.Empty)).StatusCode, Is.EqualTo(statusCode));

            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<ValidationResponseBase>(AdminApiRoutes.CreateOfflineDeposit, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<ValidationResponseBase>(AdminApiRoutes.ConfirmOfflineDeposit, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<ValidationResponseBase>(AdminApiRoutes.VerifyOfflineDeposit, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<ValidationResponseBase>(AdminApiRoutes.UnverifyOfflineDeposit, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<ValidationResponseBase>(AdminApiRoutes.ApproveOfflineDeposit, string.Empty)).StatusCode, Is.EqualTo(statusCode));
            Assert.That(Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<ValidationResponseBase>(AdminApiRoutes.RejectOfflineDeposit, string.Empty)).StatusCode, Is.EqualTo(statusCode));

        }
           
        #region PaymentGatewaySettingsController

        #region Add
        [When(@"New (.*) PaymentGatewaySettings is created")]
        public void WhenNewPaymentGatewaySettingsIsCreated(string activationStatus)
        {
            var isActive = activationStatus.Equals("activated");
            var brandId = DefaultBrandId;
            ScenarioContext.Current.Add(KeyPaymentGatewaySettingsData, PaymentTestHelper.CreatePaymentGatewaySettings(brandId, isActive));
        }

        [Then(@"New PaymentGatewaySettings is successfully added")]
        public void ThenNewPaymentGatewaySettingsIsSuccessfullyAdded()
        {
            var brandId = DefaultBrandId;

            var model = new SavePaymentGatewaySettingsRequest
            {
                Brand = brandId,
                OnlinePaymentMethodName = TestDataGenerator.GetRandomString(20),
                Channel = TestDataGenerator.GetRandomNumber(99999),
                PaymentGatewayName = "XPAY",
                EntryPoint = "http://shop.domain.com/payment/issue",
                Remarks = "Add remarks"
            };

            var result = AdminApiProxy.AddPaymentGatewaySettings(model);
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Success.Should().Be(true);

            ScenarioContext.Current[KeyPaymentGatewaySettingsId] = result.Id;
            ScenarioContext.Current[KeyPaymentGatewaySettingsRequest] = model;
        }

        [Then(@"I am forbidden to add PaymentGatewaySettings with invalid brand")]
        public void ThenIAmForbiddenToAddPaymentGatewaySettingsWithInvalidBrand()
        {
            var model = new SavePaymentGatewaySettingsRequest
            {
                Brand = new Guid("00000000-0000-0000-0000-000000000000"),
                OnlinePaymentMethodName = TestDataGenerator.GetRandomString(20),
                Channel = TestDataGenerator.GetRandomNumber(99999),
                PaymentGatewayName = "XPAY",
                EntryPoint = "http://shop.domain.com/payment/issue",
                Remarks = "Add remarks"
            };

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.AddPaymentGatewaySettings(model));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [When(@"Add PaymentGatewaySettings with existed onlinePaymentMethodName")]
        public void WhenAddPayemntGatewaySettingsWithExistedOnlinePaymentMethodName()
        {
            var model = ScenarioContext.Current.Get<SavePaymentGatewaySettingsRequest>(KeyPaymentGatewaySettingsRequest);
            model.Channel = 9876;
            var result = AdminApiProxy.AddPaymentGatewaySettings(model);
            ScenarioContext.Current[KeyValidationResponseBase] = result;
        }

        [When(@"Add PaymentGatewaySettings with the same settings")]
        public void WhenAddPayemntGatewaySettingsWithTheSameSettings()
        {
            var model = ScenarioContext.Current.Get<SavePaymentGatewaySettingsRequest>(KeyPaymentGatewaySettingsRequest);
            model.OnlinePaymentMethodName = "IntegrationTest2" + DateTime.Now.ToString("yyyyMMddHHmmss");
            var result = AdminApiProxy.AddPaymentGatewaySettings(model);
            ScenarioContext.Current[KeyValidationResponseBase] = result;
        }

        [Then(@"The PaymentGatewaySettings can not be saved due to (.*)")]
        public void ThenThePaymentGatewaySettingsCanNotBeSaved(string errorMessage)
        {
            var result = ScenarioContext.Current.Get<ValidationResponseBase>(KeyValidationResponseBase);
            result.Success.Should().BeFalse();
            result.Errors.Should().NotBeNull();
            result.Errors.FirstOrDefault().ErrorMessage.Should().Be(errorMessage);
        }
        #endregion 

        #region Edit
        [Then(@"PaymentGatewaySettings data is successfully edited")]
        public void ThenPaymentGatewaySettingsIsSuccessfullyEdited()
        {
            ScenarioContext.Current.Should().ContainKey(KeyPaymentGatewaySettingsData);
            var settings = ScenarioContext.Current.Get<PaymentGatewaySettings>(KeyPaymentGatewaySettingsData);

            var editData = Mapper.Map<SavePaymentGatewaySettingsRequest>(settings);

            editData.EntryPoint = "http://edited.domain.com";
            editData.Remarks = "edited remark";

            var result = AdminApiProxy.EditPaymentGatewaySettings(editData);
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Success.Should().Be(true);
            ScenarioContext.Current[KeyPaymentGatewaySettingsRequest] = editData;
        }

        [Then(@"I am forbidden to edit PaymentGatewaySettings with invalid brand")]
        public void ThenIAmForbiddenToEditPaymentGatewaySettingsWithInvalidBrand()
        {
            ScenarioContext.Current.Should().ContainKey(KeyPaymentGatewaySettingsData);
            var settings = ScenarioContext.Current.Get<PaymentGatewaySettings>(KeyPaymentGatewaySettingsData);

            var editData = Mapper.Map<SavePaymentGatewaySettingsRequest>(settings);
            editData.Brand = new Guid("00000000-0000-0000-0000-000000000000");
            editData.EntryPoint = "http://edited.domain.com";
            editData.Remarks = "edited remark";

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.EditPaymentGatewaySettings(editData));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [When(@"Edit the PaymentGatewaySettings with wrong id")]
        public void WhenEditThePaymnetGatewaySettingsWithWrongId()
        {
            ScenarioContext.Current.Should().ContainKey(KeyPaymentGatewaySettingsId);
            var editData = ScenarioContext.Current.Get<SavePaymentGatewaySettingsRequest>(KeyPaymentGatewaySettingsRequest);
            editData.Id = _notExistId;
            var result = AdminApiProxy.EditPaymentGatewaySettings(editData);

            ScenarioContext.Current[KeyValidationResponseBase] = result;
        }

        #endregion

        #region Activate
        

        [Then(@"I am forbidden to activate PaymentGatewaySettings with invalid brand")]
        public void ThenIAmForbiddenToActivatePaymentGatewaySettingsWithInvalidBrand()
        {
            ScenarioContext.Current.Should().ContainKey(KeyPaymentGatewaySettingsData);
            var settings = ScenarioContext.Current.Get<PaymentGatewaySettings>(KeyPaymentGatewaySettingsData);

            var request = new ActivatePaymentGatewaySettingsRequest
            {
                Id = settings.Id,
                Remarks = "Activate remarks"
            };

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.ActivatePaymentGatewaySettings(request));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"PaymentGatewaySettings is successfully activated")]
        public void ThenPaymentGatewaySettingsIsSuccessfullyActivated()
        {
            ScenarioContext.Current.Should().ContainKey(KeyPaymentGatewaySettingsData);
            var settings = ScenarioContext.Current.Get<PaymentGatewaySettings>(KeyPaymentGatewaySettingsData);

            var result = AdminApiProxy.ActivatePaymentGatewaySettings(
                new ActivatePaymentGatewaySettingsRequest
                {
                    Id = settings.Id,
                    Remarks = "activiate"
                });
            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }

        [When(@"Activate the PaymentGatewaySettings with wrong id")]
        public void WhenActivateThePaymnetGatewaySettingsWithWrongId()
        {
            var result = AdminApiProxy.ActivatePaymentGatewaySettings(
                new ActivatePaymentGatewaySettingsRequest
                {
                    Id = _notExistId,
                    Remarks = "activate"
                });
            ScenarioContext.Current[KeyValidationResponseBase] = result;
        }

        [Then(@"The PaymentGatewaySettings can not be activated due to (.*)")]
        public void ThenThePaymentGatewaySettingsCanNotBeActivated(string errorMessage)
        {
            var result = ScenarioContext.Current.Get<ValidationResponseBase>(KeyValidationResponseBase);
            result.Success.Should().BeFalse();
            result.Errors.Should().NotBeNull();
            result.Errors.FirstOrDefault().ErrorMessage.Should().Be(errorMessage);
        }
        #endregion 

        #region Deactivate

        [Then(@"I am forbidden to deactivate PaymentGatewaySettings with invalid brand")]
        public void ThenIAmForbiddenToDeactivatePaymentGatewaySettingsWithInvalidBrand()
        {
            ScenarioContext.Current.Should().ContainKey(KeyPaymentGatewaySettingsData);
            var settings = ScenarioContext.Current.Get<PaymentGatewaySettings>(KeyPaymentGatewaySettingsData);

            var request = new DeactivatePaymentGatewaySettingsRequest
            {
                Id = settings.Id,
                Remarks = "Deactivate remarks"
            };

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.DeactivatePaymentGatewaySettings(request));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"PaymentGatewaySettings is successfully deactivated")]
        public void ThenPaymentGatewaySettingsIsSuccessfullyDeactivated()
        {
            ScenarioContext.Current.Should().ContainKey(KeyPaymentGatewaySettingsData);
            var settings = ScenarioContext.Current.Get<PaymentGatewaySettings>(KeyPaymentGatewaySettingsData);

            var result = AdminApiProxy.DeactivatePaymentGatewaySettings(
                new DeactivatePaymentGatewaySettingsRequest
                {
                    Id = settings.Id,
                    Remarks = "activiate"
                });
            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }

        [When(@"Deactivate the PaymentGatewaySettings with wrong id")]
        public void WhenDeactivateThePaymnetGatewaySettingsWithWrongId()
        {
            var result = AdminApiProxy.DeactivatePaymentGatewaySettings(
                new DeactivatePaymentGatewaySettingsRequest
                {
                    Id = _notExistId,
                    Remarks = "deactivate"
                });
            ScenarioContext.Current[KeyValidationResponseBase] = result;
        }
      
        [Then(@"The PaymentGatewaySettings can not be deactivated due to (.*)")]
        public void ThenThePaymentGatewaySettingsCanNotBeDeactivated(string errorMessage)
        {
            var result = ScenarioContext.Current.Get<ValidationResponseBase>(KeyValidationResponseBase);
            result.Success.Should().BeFalse();
            result.Errors.Should().NotBeNull();
            result.Errors.FirstOrDefault().ErrorMessage.Should().Be(errorMessage);
        }
        #endregion            
  
        #region GetById
        [Then(@"The PaymentGatewaySettings is visible to me")]
        public void ThenThePayemntGatewaySettingsIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey(KeyPaymentGatewaySettingsId);
            var paymentGatewaySettingsId = ScenarioContext.Current.Get<Guid>(KeyPaymentGatewaySettingsId);

            var requestData = ScenarioContext.Current.Get<SavePaymentGatewaySettingsRequest>(KeyPaymentGatewaySettingsRequest);
            var result = AdminApiProxy.GetPaymentGatewaySettingsById(paymentGatewaySettingsId);
            result.Should().NotBeNull();
            result.Id.Should().Be(paymentGatewaySettingsId);
            result.OnlinePaymentMethodName.Should().Be(requestData.OnlinePaymentMethodName);
            result.Channel.Should().Be(requestData.Channel);
            result.PaymentGatewayName.Should().Be(requestData.PaymentGatewayName);
            result.EntryPoint.Should().Be(requestData.EntryPoint);
            result.Remarks.Should().Be(requestData.Remarks);
        }

        [Then(@"The status of PaymentGatewaySettings is (.*)")]
        public void ThenTheStatusOfPayemntGatewaySettingsIs(string activationStatus)
        {
            ScenarioContext.Current.Should().ContainKey(KeyPaymentGatewaySettingsId);
            var paymentGatewaySettingsId = ScenarioContext.Current.Get<Guid>(KeyPaymentGatewaySettingsId);

            var result = AdminApiProxy.GetPaymentGatewaySettingsById(paymentGatewaySettingsId);
            result.Should().NotBeNull();
            result.Status.ToString().ToLower().Should().Be(activationStatus.ToLower());
        }      
        #endregion

        #region GetPaymentGateways
        [Then(@"The PaymentGateways is visible to me")]
        public void ThenThePayemntGatewaysIsVisibleToMe()
        {            
            var brandId = DefaultBrandId;
            var result = AdminApiProxy.GetPaymentGateways(new GetPaymentGatewaysRequest
            {
                BrandId = brandId
            });

            result.Should().NotBeNull();
            result.PaymentGateways.Any().Should().BeTrue();
        }

        [Then(@"I am forbidden to get PaymentGateways with invalid brand")]
        public void ThenIAmForbiddenToGetPaymentGatewaysWithInvalidBrand()
        {
            var brandId = Guid.NewGuid();
            var request = new GetPaymentGatewaysRequest
            {
                BrandId = brandId
            };

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.GetPaymentGateways(request));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }
        #endregion

        #endregion

        #region OnlineDepositController
        #region GetById
        [When(@"New OnlineDeposit is created")]
        public void WhenNewOnlineDepositIsCreated()
        {                        
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");
            var data = PaymentTestHelper.CreateOnlineDeposit(playerId, 250);

            ScenarioContext.Current.Add(KeyOnlineDepositData, data);
        }

        [Then(@"The OnlineDeposit is visible to me")]
        public void ThenTheOnlineDepositIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey(KeyOnlineDepositData);
            var deposit = ScenarioContext.Current.Get<OnlineDeposit>(KeyOnlineDepositData);
            
            var result = AdminApiProxy.GetOnlineDepositById(deposit.Id);
            result.Should().NotBeNull();
            result.Id.Should().Be(deposit.Id);
            result.PaymentMethod.Should().Be(deposit.Method);
            result.Amount.Should().Be(deposit.Amount);
        }

        [Then(@"I am forbidden to get OnlineDeposit with invalid brand")]
        public void ThenIAmForbiddenToGetOnlineDepositWithInvalidBrand()
        {
            ScenarioContext.Current.Should().ContainKey(KeyOnlineDepositData);
            var deposit = ScenarioContext.Current.Get<OnlineDeposit>(KeyOnlineDepositData);

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.GetOnlineDepositById(deposit.Id));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        #endregion

        #region Verify
        [Then(@"OnlineDeposit is successfully verified")]
        public void ThenOnlineDepositIsSuccessfullyVerified()
        {
            ScenarioContext.Current.Should().ContainKey(KeyOnlineDepositData);
            var deposit = ScenarioContext.Current.Get<OnlineDeposit>(KeyOnlineDepositData);

            var result = AdminApiProxy.VerifyOnlineDeposit(
                new VerifyOnlineDepositRequest
                {
                    Id = deposit.Id,
                    Remarks = "verify"
                });
            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }       

        [Then(@"I am forbidden to verify OnlineDeposit with invalid brand")]
        public void ThenIAmForbiddenToVerifyOnlineDepositWithInvalidBrand()
        {
            ScenarioContext.Current.Should().ContainKey(KeyOnlineDepositData);
            var deposit = ScenarioContext.Current.Get<OnlineDeposit>(KeyOnlineDepositData);
            
            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.VerifyOnlineDeposit(new VerifyOnlineDepositRequest
            {
                Id=deposit.Id,
                Remarks = "verify"
            }));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [When(@"OnlineDeposit is verified")]
        public void WhenOnlineDepositIsVerified()
        {
            ScenarioContext.Current.Should().ContainKey(KeyOnlineDepositData);
            var deposit = ScenarioContext.Current.Get<OnlineDeposit>(KeyOnlineDepositData);

            var data = PaymentTestHelper.VerifyOnlineDeposit(deposit.Id, "verify remarks");

            ScenarioContext.Current[KeyOnlineDepositData] = data;
        }
        #endregion 

        #region Reject
        [Then(@"OnlineDeposit is successfully rejected")]
        public void ThenOnlineDepositIsSuccessfullyRejected()
        {
            ScenarioContext.Current.Should().ContainKey(KeyOnlineDepositData);
            var deposit = ScenarioContext.Current.Get<OnlineDeposit>(KeyOnlineDepositData);

            var result = AdminApiProxy.RejectOnlineDeposit(
                new RejectOnlineDepositRequest
                {
                    Id = deposit.Id,
                    Remarks = "reject"
                });
            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }

        [Then(@"I am forbidden to reject OnlineDeposit with invalid brand")]
        public void ThenIAmForbiddenToRejectOnlineDepositWithInvalidBrand()
        {
            ScenarioContext.Current.Should().ContainKey(KeyOnlineDepositData);
            var deposit = ScenarioContext.Current.Get<OnlineDeposit>(KeyOnlineDepositData);

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.RejectOnlineDeposit(new RejectOnlineDepositRequest
            {
                Id = deposit.Id,
                Remarks = "reject"
            }));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }
        #endregion

        #region Approve
        [Then(@"OnlineDeposit is successfully approved")]
        public void ThenOnlineDepositIsSuccessfullyApproved()
        {
            ScenarioContext.Current.Should().ContainKey(KeyOnlineDepositData);
            var deposit = ScenarioContext.Current.Get<OnlineDeposit>(KeyOnlineDepositData);

            var result = AdminApiProxy.ApproveOnlineDeposit(
                new ApproveOnlineDepositRequest
                {
                    Id = deposit.Id,
                    Remarks = "reject"
                });
            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }     
    
        [Then(@"I am forbidden to approve OnlineDeposit with invalid brand")]
        public void ThenIAmForbiddenToApproveOnlineDepositWithInvalidBrand()
        {
            ScenarioContext.Current.Should().ContainKey(KeyOnlineDepositData);
            var deposit = ScenarioContext.Current.Get<OnlineDeposit>(KeyOnlineDepositData);

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.ApproveOnlineDeposit(new ApproveOnlineDepositRequest
            {
                Id = deposit.Id,
                Remarks = "reject"
            }));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }
        #endregion
        #endregion

        #region BanksController
        #region GetById
        [Then(@"The Bank is visible to me")]
        public void ThenTheBankIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey(KeyBankData);
            var bank = ScenarioContext.Current.Get<Bank>(KeyBankData);

            var result = AdminApiProxy.GetBankById(bank.Id);
            result.Should().NotBeNull();
            result.Bank.Id.Should().Be(bank.Id);
            result.Bank.BankId.Should().Be(bank.BankId);
            result.Bank.BankName.Should().Be(bank.BankName);
        }

        [Then(@"I am forbidden to get Bank with invalid brand")]
        public void ThenIAmForbiddenToGetBankWithInvalidBrand()
        {
            ScenarioContext.Current.Should().ContainKey(KeyBankData);
            var bank = ScenarioContext.Current.Get<Bank>(KeyBankData);

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.GetBankById(bank.Id));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }
        #endregion

        #region Add
        [Then(@"New Bank is successfully added")]
        public void ThenNewBankIsSuccessfullyAdded()
        {
            var brandId = DefaultBrandId;

            var model = new AddBankRequest
            {
                BrandId = brandId,
                BankId = TestDataGenerator.GetRandomString(20),
                BankName = TestDataGenerator.GetRandomString(25),
                CountryCode = "CA",
                Remarks = "Add remarks by integration test"
            };

            var result = AdminApiProxy.AddBank(model);
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Success.Should().Be(true);

            ScenarioContext.Current[KeyBankId] = result.Id;
        }        

        [Then(@"I am forbidden to add Bank with invalid brand")]
        public void ThenIAmForbiddenToAddBankWithInvalidBrand()
        {
            var model = new AddBankRequest
            {
                BrandId = new Guid("00000000-0000-0000-0000-000000000000"),
                BankId = TestDataGenerator.GetRandomString(20),
                BankName = TestDataGenerator.GetRandomString(25),
                CountryCode = "CA",
                Remarks = "Add remarks by integration test"
            };

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.AddBank(model));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [When(@"New Bank is created")]
        public void WhenNewBankIsCreated()
        {
            var brandId = DefaultBrandId;
            ScenarioContext.Current.Add(KeyBankData, PaymentTestHelper.CreateBank(brandId, "GB"));
        }
        #endregion

        #region Edit
        [Then(@"Bank data is successfully edited")]
        public void ThenBankIsSuccessfullyEdited()
        {
            ScenarioContext.Current.Should().ContainKey(KeyBankData);
            var settings = ScenarioContext.Current.Get<Bank>(KeyBankData);

            var editData = Mapper.Map<EditBankRequest>(settings);

            editData.BankName = TestDataGenerator.GetRandomString(25);
            editData.Remarks = "edited remark";

            var result = AdminApiProxy.EditBank(editData);
            result.Should().NotBeNull();            
            result.Success.Should().Be(true);
        }

        [Then(@"I am forbidden to edit Bank with invalid brand")]
        public void ThenIAmForbiddenToEditBankWithInvalidBrand()
        {
            ScenarioContext.Current.Should().ContainKey(KeyBankData);
            var settings = ScenarioContext.Current.Get<Bank>(KeyBankData);

            var editData = Mapper.Map<EditBankRequest>(settings);
            editData.BrandId = new Guid("00000000-0000-0000-0000-000000000000");
            editData.BankName = TestDataGenerator.GetRandomString(25);
            editData.Remarks = "edited remark";

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.EditBank(editData));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }
        #endregion
        #endregion

        #region BankAccountsController
        #region Add
        [Then(@"New BankAccount is successfully added")]
        public void ThenNewBankAccountIsSuccessfullyAdded()
        {
            var brandId = DefaultBrandId;

            var model = new AddBankAccountRequest
            {
                BrandId = brandId.ToString(),
                LicenseeId = DefaultLicenseeId.ToString(),
                AccountId = TestDataGenerator.GetRandomString(10),
                AccountName = TestDataGenerator.GetRandomString(10),
                AccountNumber = TestDataGenerator.GetRandomString(10, "1234567890"),
                Bank = BankSE45Id,
                AccountType = BankAccountTypeVIPId,                
                Branch = "Branch",
                Currency = "RMB",
                Province = "Province",
                SupplierName = "Supplier Name",
                ContactNumber = "911",
                USBCode = "USB001",
                PurchasedDate = DateTime.Now.AddMonths(-2).ToString(CultureInfo.InvariantCulture),
                UtilizationDate = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                ExpirationDate = DateTime.Now.AddYears(1).ToString(CultureInfo.InvariantCulture)
            };

            var result = AdminApiProxy.AddBankAccount(model);
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Success.Should().Be(true);

            ScenarioContext.Current[KeyBankAccountId] = result.Id;            
        }

        [Then(@"I am forbidden to add BankAccount with invalid brand")]
        public void ThenIAmForbiddenToAddBankAccountWithInvalidBrand()
        {
            var model = new AddBankAccountRequest
            {
                BrandId = _notExistId.ToString(),
                LicenseeId = DefaultLicenseeId.ToString(),
                AccountId = TestDataGenerator.GetRandomString(10),
                AccountName = TestDataGenerator.GetRandomString(10),
                AccountNumber = TestDataGenerator.GetRandomString(10, "1234567890"),
                Bank = BankSE45Id,
                AccountType = BankAccountTypeVIPId,
                Branch = "Branch",
                Currency = "RMB",
                Province = "Province",
                SupplierName = "Supplier Name",
                ContactNumber = "911",
                USBCode = "USB001",
                PurchasedDate = DateTime.Now.AddMonths(-2).ToString(CultureInfo.InvariantCulture),
                UtilizationDate = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                ExpirationDate = DateTime.Now.AddYears(1).ToString(CultureInfo.InvariantCulture)
            };

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.AddBankAccount(model));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [When(@"New (.*) BankAccount is created")]
        public void WhenNewBankAccountIsCreated(string activationStatus)
        {
            var isActive = activationStatus.Equals("activated");            
            var brandId = DefaultBrandId;
            ScenarioContext.Current.Add(KeyBankAccountData, PaymentTestHelper.CreateBankAccount(brandId, "CAD", isActive));
        }
        #endregion

        #region Edit
        [Then(@"BankAccount data is successfully edited")]
        public void ThenBankAccountIsSuccessfullyEdited()
        {
            ScenarioContext.Current.Should().ContainKey(KeyBankAccountData);
            var data = ScenarioContext.Current.Get<BankAccount>(KeyBankAccountData);

            var editData = Mapper.Map<EditBankAccountRequest>(data);
            editData.BrandId = DefaultBrandId;
            editData.AccountId = TestDataGenerator.GetRandomString(10);
            editData.AccountName = TestDataGenerator.GetRandomString(10);
            editData.AccountNumber = TestDataGenerator.GetRandomString(10, "1234567890");
            editData.Currency = "RMB";
            editData.Province = TestDataGenerator.GetRandomString(5);
            editData.Remarks = "edited remark";
            editData.PurchasedDate = DateTime.Now.AddMonths(-2).ToString(CultureInfo.InvariantCulture);
            editData.UtilizationDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            editData.ExpirationDate = DateTime.Now.AddYears(1).ToString(CultureInfo.InvariantCulture);
            
            var result = AdminApiProxy.EditBankAccount(editData);
            result.Should().NotBeNull();
            result.Success.Should().Be(true);
            
            
        }

        [Then(@"I am forbidden to edit BankAccount with invalid brand")]
        public void ThenIAmForbiddenToEditBankAccountWithInvalidBrand()
        {
            ScenarioContext.Current.Should().ContainKey(KeyBankAccountData);
            var data = ScenarioContext.Current.Get<BankAccount>(KeyBankAccountData);

            var editData = Mapper.Map<EditBankAccountRequest>(data);
            editData.BrandId = Guid.NewGuid();
            editData.AccountId = TestDataGenerator.GetRandomString(10);
            editData.AccountName = TestDataGenerator.GetRandomString(10);
            editData.AccountNumber = TestDataGenerator.GetRandomString(10, "1234567890");
            editData.Currency = "RMB";
            editData.Province = TestDataGenerator.GetRandomString(5);
            editData.Remarks = "edited remark";
            editData.PurchasedDate = DateTime.Now.AddMonths(-2).ToString(CultureInfo.InvariantCulture);
            editData.UtilizationDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            editData.ExpirationDate = DateTime.Now.AddYears(1).ToString(CultureInfo.InvariantCulture);
            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.EditBankAccount(editData));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }
        #endregion

        #region GetById
        [Then(@"The BankAccount is visible to me")]
        public void ThenTheBankAccountIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey(KeyBankAccountData);
            var bankAccount = ScenarioContext.Current.Get<BankAccount>(KeyBankAccountData);

            var result = AdminApiProxy.GetBankAccountById(bankAccount.Id);
            result.Should().NotBeNull();
            result.BankAccount.Id.Should().Be(bankAccount.Id);
            result.BankAccount.AccountId.Should().Be(bankAccount.AccountId);
            result.BankAccount.AccountName.Should().Be(bankAccount.AccountName);
            result.BankAccount.AccountNumber.Should().Be(bankAccount.AccountNumber);
            result.BankAccount.AccountType.Id.Should().Be(bankAccount.AccountType.Id);
            result.BankAccount.Province.Should().Be(bankAccount.Province);
            result.BankAccount.Branch.Should().Be(bankAccount.Branch);
            result.BankAccount.CurrencyCode.Should().Be(bankAccount.CurrencyCode);
            result.BankAccount.SupplierName.Should().Be(bankAccount.SupplierName);
            result.BankAccount.USBCode.Should().Be(bankAccount.USBCode);
            result.BankAccount.PurchasedDate.Should().Be(bankAccount.PurchasedDate);
            result.BankAccount.UtilizationDate.Should().Be(bankAccount.UtilizationDate);
            result.BankAccount.ExpirationDate.Should().Be(bankAccount.ExpirationDate);
            result.BankAccount.IdFrontImage.Should().Be(bankAccount.IdFrontImage);
            result.BankAccount.IdBackImage.Should().Be(bankAccount.IdBackImage);
            result.BankAccount.ATMCardImage.Should().Be(bankAccount.ATMCardImage);
            result.BankAccount.Remarks.Should().Be(bankAccount.Remarks);
            result.BankAccount.Status.ToString().Should().Be(bankAccount.Status.ToString());
        }

        [Then(@"I am forbidden to get BankAccount with invalid brand")]
        public void ThenIAmForbiddenToGetBankAccountWithInvalidBrand()
        {
            var bankAccount = ScenarioContext.Current.Get<BankAccount>(KeyBankAccountData);

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.GetBankAccountById(bankAccount.Id));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }
        #endregion

        #region Activate
        [Then(@"I am forbidden to activate BankAccount with invalid brand")]
        public void ThenIAmForbiddenToActivateBankAccountWithInvalidBrand()
        {
            var data = ScenarioContext.Current.Get<BankAccount>(KeyBankAccountData);

            var request = new ActivateBankAccountRequest
            {
                Id = data.Id,
                Remarks = "Activate remarks"
            };

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.ActivateBankAccount(request));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"BankAccount is successfully activated")]
        public void ThenBankAccountIsSuccessfullyActivated()
        {            
            var data = ScenarioContext.Current.Get<BankAccount>(KeyBankAccountData);

            var result = AdminApiProxy.ActivateBankAccount(
                new ActivateBankAccountRequest
                {
                    Id = data.Id,
                    Remarks = "activiate"
                });

            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }
        #endregion 

        #region Deactivate

        [Then(@"I am forbidden to deactivate BankAccount with invalid brand")]
        public void ThenIAmForbiddenToDeactivateBankAccountWithInvalidBrand()
        {
            var data = ScenarioContext.Current.Get<BankAccount>(KeyBankAccountData);

            var request = new DeactivateBankAccountRequest
            {
                Id = data.Id,
                Remarks = "Deactivate remarks"
            };

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.DeactivateBankAccount(request));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"BankAccount is successfully deactivated")]
        public void ThenBankAccountIsSuccessfullyDeactivated()
        {
            var data = ScenarioContext.Current.Get<BankAccount>(KeyBankAccountData);

            var result = AdminApiProxy.DeactivateBankAccount(
                new DeactivateBankAccountRequest
                {
                    Id = data.Id,
                    Remarks = "activiate"
                });
            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }
        #endregion
        #endregion

        #region PlayerBankAccountsController
        #region Add

        [When(@"New (.*) PlayerBankAccount is created")]
        public void WhenPlayerBankAccountBankAccountIsCreated(string activationStatus)
        {
            var isActive = activationStatus.Equals("activated");
            var brandId = DefaultBrandId;
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");

            ScenarioContext.Current.Add(KeyPlayerBankAccountData, PaymentTestHelper.CreatePlayerBankAccount(playerId,brandId, isActive));
        }
        #endregion


        #region Verify
        [Then(@"I am forbidden to verify PlayerBankAccount with invalid brand")]
        public void ThenIAmForbiddenToVerifyPlayerBankAccountWithInvalidBrand()
        {
            var data = ScenarioContext.Current.Get<PlayerBankAccount>(KeyPlayerBankAccountData);

            var request = new VerifyPlayerBankAccountRequest
            {
                Id = data.Id,
                Remarks ="remarks"
            };

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.VerifyPlayerBankAccount(request));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"PlayerBankAccount is successfully verified")]
        public void ThenBankAccountIsSuccessfullyVerified()
        {
            var data = ScenarioContext.Current.Get<PlayerBankAccount>(KeyPlayerBankAccountData);

            var result = AdminApiProxy.VerifyPlayerBankAccount(
                new VerifyPlayerBankAccountRequest
                {
                    Id = data.Id,
                    Remarks = "remarks"
                });

            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }
        #endregion 

        #region Deactivate

        [Then(@"I am forbidden to reject PlayerBankAccount with invalid brand")]
        public void ThenIAmForbiddenToRejectPlayerBankAccountWithInvalidBrand()
        {
            var data = ScenarioContext.Current.Get<PlayerBankAccount>(KeyPlayerBankAccountData);

            var request = new RejectPlayerBankAccountRequest
            {
                Id = data.Id,
                Remarks = "remarks"
            };

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.RejectPlayerBankAccount(request));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Then(@"PlayerBankAccount is successfully rejected")]
        public void ThenPlayerBankAccountIsSuccessfullyRejected()
        {
            ScenarioContext.Current.Should().ContainKey(KeyPlayerBankAccountData);
            var data = ScenarioContext.Current.Get<PlayerBankAccount>(KeyPlayerBankAccountData);

            var result = AdminApiProxy.RejectPlayerBankAccount(
                new RejectPlayerBankAccountRequest
                {
                    Id = data.Id,
                    Remarks = "remarks"
                });
            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }
        #endregion
        #endregion

        #region OfflineDepositController
        #region Create
        [Then(@"OfflineDeposit is successfully created")]
        public void ThenOfflineDepositIsSuccessfullyCreated()
        {
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");
            var bankAccount = ScenarioContext.Current.Get<BankAccount>(KeyBankAccountData);
            var request = GetCreateOfflineDepositRequest();

            var result = AdminApiProxy.CreateOfflineDeposit(request);
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Success.Should().Be(true);

            ScenarioContext.Current[KeyOfflineDepositId] = result.Id;
        }

        [When(@"New OfflineDeposit is created")]
        public void WhenNewOfflineDepositIsCreated()
        {
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");
            var data = PaymentTestHelper.CreateOfflineDeposit(playerId, 250);

            ScenarioContext.Current.Add(KeyOfflineDepositData, data);
        }

        [Then(@"The OfflineDeposit is visible to me")]
        public void ThenTheOfflineDepositIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey(KeyOfflineDepositData);
            var deposit = ScenarioContext.Current.Get<OfflineDeposit>(KeyOfflineDepositData);

            var result = AdminApiProxy.GetOfflineDepositById(deposit.Id).OfflineDeposit;
            result.Should().NotBeNull();
            result.Id.Should().Be(deposit.Id);
            result.Amount.Should().Be(deposit.Amount);
            result.PlayerId.Should().Be(deposit.PlayerId);
            result.BankAccountId.Should().Be(deposit.BankAccountId);
            result.CurrencyCode.Should().Be(deposit.CurrencyCode);
            result.TransactionNumber.Should().Be(deposit.TransactionNumber);
            result.Status.ToString().Should().Be(deposit.Status.ToString());
            result.PaymentMethod.ToString().Should().Be(deposit.PaymentMethod.ToString());
            result.TransferType.ToString().Should().Be(deposit.TransferType.ToString());
            result.DepositMethod.ToString().Should().Be(deposit.DepositMethod.ToString());
            result.DepositType.ToString().Should().Be(deposit.DepositType.ToString());
            result.Remark.Should().Be(deposit.Remark);
            result.IdFrontImage.Should().Be(deposit.IdFrontImage);
            result.IdBackImage.Should().Be(deposit.IdBackImage);
            result.ReceiptImage.Should().Be(deposit.ReceiptImage);
        }

        [Then(@"I am forbidden to create OfflineDeposit with invalid brand")]
        public void ThenIAmForbiddenToCreateOfflineDepositWithInvalidBrand()
        {
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");
            var bankAccount = ScenarioContext.Current.Get<BankAccount>(KeyBankAccountData);
            var request = GetCreateOfflineDepositRequest();

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.CreateOfflineDeposit(request));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }
        #endregion

        #region Confirm
        [Then(@"OfflineDeposit is successfully confirmed")]
        public void ThenOfflineDepositIsSuccessfullyConfirmed()
        {
            var request = GetConfirmOfflineDepositRequest();

            var result = AdminApiProxy.ConfirmOfflineDeposit(request);
            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }

        [Then(@"I am forbidden to confirm OfflineDeposit with invalid brand")]
        public void ThenIAmForbiddenToConfirmOfflineDepositWithInvalidBrand()
        {
            var request = GetConfirmOfflineDepositRequest();

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.ConfirmOfflineDeposit(request));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [When(@"OfflineDeposit is confirmed")]
        public void WhenOfflineDepositIsConfirmed()
        {
            var deposit = ScenarioContext.Current.Get<OfflineDeposit>(KeyOfflineDepositData);
            PaymentTestHelper.ConfirmOfflineDeposit(deposit);                        
        }
        #endregion 

        #region Verify
        [Then(@"OfflineDeposit is successfully verified")]
        public void ThenOfflineDepositIsSuccessfullyVerified()
        {
            var result = AdminApiProxy.VerifyOfflineDeposit(GetVerifyOfflineDepositRequest());

            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }        

        [Then(@"I am forbidden to verify OfflineDeposit with invalid brand")]
        public void ThenIAmForbiddenToVerifyOfflineDepositWithInvalidBrand()
        {
            var request = GetVerifyOfflineDepositRequest();

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.VerifyOfflineDeposit(request));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [When(@"OfflineDeposit is verified")]
        public void WhenOfflineDepositIsVerified()
        {
            var deposit = ScenarioContext.Current.Get<OfflineDeposit>(KeyOfflineDepositData);
            PaymentTestHelper.VerifyOfflineDeposit(deposit,true);
        }
        #endregion

        #region Unverify
        [Then(@"OfflineDeposit is successfully unverified")]
        public void ThenOfflineDepositIsSuccessfullyUnverified()
        {
            var result = AdminApiProxy.UnverifyOfflineDeposit(GetUnverifyOfflineDepositRequest());

            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }

        [Then(@"I am forbidden to unverify OfflineDeposit with invalid brand")]
        public void ThenIAmForbiddenToUnverifyOfflineDepositWithInvalidBrand()
        {
            var request = GetUnverifyOfflineDepositRequest();

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.UnverifyOfflineDeposit(request));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }
        #endregion

        #region Approve
        [Then(@"OfflineDeposit is successfully approved")]
        public void ThenOfflineDepositIsSuccessfullyapproved()
        {
            var result = AdminApiProxy.ApproveOfflineDeposit(GetApproveOfflineDepositRequest());

            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }

        [Then(@"I am forbidden to approve OfflineDeposit with invalid brand")]
        public void ThenIAmForbiddenToApproveOfflineDepositWithInvalidBrand()
        {
            var request = GetApproveOfflineDepositRequest();

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.ApproveOfflineDeposit(request));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }
        #endregion

        #region Reject
        [Then(@"OfflineDeposit is successfully rejected")]
        public void ThenOfflineDepositIsSuccessfullyRejectd()
        {
            var result = AdminApiProxy.RejectOfflineDeposit(GetRejectOfflineDepositRequest());

            result.Should().NotBeNull();
            result.Success.Should().Be(true);
        }

        [Then(@"I am forbidden to reject OfflineDeposit with invalid brand")]
        public void ThenIAmForbiddenToRejectOfflineDepositWithInvalidBrand()
        {
            var request = GetRejectOfflineDepositRequest();

            var ex = Assert.Throws<HttpException>(() => AdminApiProxy.RejectOfflineDeposit(request));
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }
        #endregion	

        #endregion end of OfflineDepositController

        #region Methods

        private RejectOfflineDepositRequest GetRejectOfflineDepositRequest()
        {
            var data = ScenarioContext.Current.Get<OfflineDeposit>(KeyOfflineDepositData);
            return new RejectOfflineDepositRequest
            {
                Id = data.Id,               
                Remarks = "remarks"
            };
        }

        private ApproveOfflineDepositRequest GetApproveOfflineDepositRequest()
        {
            var data = ScenarioContext.Current.Get<OfflineDeposit>(KeyOfflineDepositData);
            return new ApproveOfflineDepositRequest
            {
                Id = data.Id,
                ActualAmount = 200,
                Fee = 10,
                PlayerRemark = "player remarks",
                Remark = "remarks"                
            };
        }
        private UnverifyOfflineDepositRequest GetUnverifyOfflineDepositRequest()
        {
            var data = ScenarioContext.Current.Get<OfflineDeposit>(KeyOfflineDepositData);
            return new UnverifyOfflineDepositRequest
            {
                Id = data.Id,                
                Remarks = "remarks",
                UnverifyReason = UnverifyReasons.D00011
            };
        }

        private VerifyOfflineDepositRequest GetVerifyOfflineDepositRequest()
        {
            var data = ScenarioContext.Current.Get<OfflineDeposit>(KeyOfflineDepositData);
            return new VerifyOfflineDepositRequest
            {
                Id = data.Id,
                BankAccountId = data.BankAccountId,
                Remarks = "remarks"
            };
        }

        private CreateOfflineDepositRequest GetCreateOfflineDepositRequest()
        {
            var playerId = ScenarioContext.Current.Get<Guid>("playerId");
            var bankAccount = ScenarioContext.Current.Get<BankAccount>(KeyBankAccountData);
            return new CreateOfflineDepositRequest
            {
                PlayerId = playerId,
                BankAccountId = bankAccount.Id,
                Amount = 200,
                PlayerRemark = "remarks"
            };
        }

        private ConfirmOfflineDepositRequest GetConfirmOfflineDepositRequest()
        {
            var data = ScenarioContext.Current.Get<OfflineDeposit>(KeyOfflineDepositData);

            return new ConfirmOfflineDepositRequest
            {
                Id = data.Id,
                PlayerAccountName = "PlayerAccountName",
                PlayerAccountNumber = TestDataGenerator.GetRandomBankAccountNumber(15),
                ReferenceNumber = TestDataGenerator.GetRandomBankAccountNumber(15),
                Amount = 200,
                TransferType = TransferType.DifferentBank,
                OfflineDepositType = DepositMethod.ATM,
                Remark = "remarks",
                CurrentUser = "testuser",
                IdFrontImage = "IdFronImage",
                IdBackImage = "IdBackImage",
                ReceiptImage = "ReceiptImage",
                IdFrontImageFile = new byte[1],
                IdBackImageFile = new byte[1],
                ReceiptImageFile = new byte[1]
            };
        }
        #endregion
    }
}
