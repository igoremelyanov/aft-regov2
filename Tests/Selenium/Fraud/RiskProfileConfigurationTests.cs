using System;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.RiskProfileCheckConfiguration;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium.Fraud
{
    ///Represents tests related to Fraud -> Risk Profile Check Configuration
    [Ignore("Svitlana: 02/15/2016; Ignored without investigation (probably changing in UI), will be investigated when Fraud subdomain will be back; AFTREGO-4260 Hide Fraud / Risk UI components in order to isolate functionality for R1.0")]
    class RiskProfileConfigurationTests : SeleniumBaseForAdminWebsite
    {
        private RiskProfileCheckConfigurationPage _riskProfileCheckConfigurationPage;
        private NewRiskProfileCheckConfigurationForm _newRPCForm;
        private DashboardPage _dashboardPage;
        private RiskProfileCheckConfigurationData _riskProfileCheckData;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private const string DefaultCurrency = "CAD";
        protected readonly Guid DefaultBrandId = new Guid("00000000-0000-0000-0000-000000000138");
        private VipLevel _vipLevel;
     
        public override void BeforeAll()
        {
            base.BeforeAll();
         
            //create a not default VIP Level for Brand
            _vipLevel = _container.Resolve<BrandTestHelper>().CreateNotDefaultVipLevel(DefaultBrandId);

            //generate risk profile check configuration form data
            _riskProfileCheckData = TestDataGenerator.CreateRiskProfileCheckConfigurationData(
                 DefaultLicensee,
                 DefaultBrand,
                 DefaultCurrency,
                 _vipLevel.Name
               );

            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _riskProfileCheckConfigurationPage = _dashboardPage.Menu.ClickRiskProfileCheckConfiguration();
            _newRPCForm = _riskProfileCheckConfigurationPage.OpenNewRiskProfileCheckConfigurationForm();
            _newRPCForm.SetRiskProfileCheckConfigurationFields(_riskProfileCheckData);
            _newRPCForm.SubmitRiskProfileCheckConfiguration().CloseTab("View Risk Profile Check Configuration");
            
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _riskProfileCheckConfigurationPage = _dashboardPage.Menu.ClickRiskProfileCheckConfiguration();
           
        }

        [Test]
        public void Can_edit_risk_profile_check_configuration()
        {
            //generate risk profile check configuration edited form data
            //create a not default VIP Level for Brand
            var _vipLevelEdited = _container.Resolve<BrandTestHelper>().CreateNotDefaultVipLevel(DefaultBrandId);
            var rpcDataEdited = TestDataGenerator.CreateRiskProfileCheckConfigurationData(
                 DefaultLicensee,
                 DefaultBrand,
                 "RMB",
                 _vipLevelEdited.Name
               );

            //edit risk profile check configuration
            var editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(_riskProfileCheckData);
            editRPCForm.EditRiskProfileConfigurationFields(_riskProfileCheckData,rpcDataEdited);
            var viewRPCForm = editRPCForm.Submit();

            Assert.AreEqual("Risk Profile Check Configuration has been sucessfully updated.", viewRPCForm.SuccessAlert.Text);
            Assert.AreEqual(rpcDataEdited.Licensee, viewRPCForm.Licensee.Text);
            Assert.AreEqual(rpcDataEdited.Brand, viewRPCForm.Brand.Text);
            Assert.AreEqual(rpcDataEdited.Currency, viewRPCForm.Currency.Text);
            Assert.AreEqual(rpcDataEdited.VipLevel, viewRPCForm.GetSelectedVipLevels()[0]);

            viewRPCForm.CloseTab("View Risk Profile Check Configuration");

            //move to the initial state
            editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(rpcDataEdited);
            editRPCForm.EditRiskProfileConfigurationFields(rpcDataEdited,_riskProfileCheckData);
            viewRPCForm = editRPCForm.Submit();

            Assert.AreEqual("Risk Profile Check Configuration has been sucessfully updated.", viewRPCForm.SuccessAlert.Text);
        }

        [Test]
        public void Can_not_create_duplicate_risk_profile_check_configuration()
        {
            _newRPCForm = _riskProfileCheckConfigurationPage.OpenNewRiskProfileCheckConfigurationForm();
            _newRPCForm.SetRiskProfileCheckConfigurationFields(_riskProfileCheckData);
            _newRPCForm.SubmitRiskProfileCheckConfiguration();

            var failRPC = new RiskProfileCheckConfigurationFailure(_driver);

            Assert.True(failRPC.ErrorAlert.Displayed);
            Assert.AreEqual(failRPC.ErrorAlert.Text, "You have already set up Risk Profile Check " +
                                                     "Configuration with the selected Brand, Currency and Vip level." +
                                                     " Please, update the existing one or change your form data.");
        }

        [TestCase(">=", "25")]
        public void Can_set_risk_profile_account_age_criteria(string criteria, string days)
        {
            var editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(_riskProfileCheckData);
            editRPCForm.SetAccountAgeCheck(criteria, days);
            var viewRPCForm = editRPCForm.Submit();

            Assert.AreEqual("Risk Profile Check Configuration has been sucessfully updated.", viewRPCForm.SuccessAlert.Text);
            Assert.AreEqual(criteria, viewRPCForm.DropdownAccountAgeSelected.Text);
            Assert.AreEqual(days, viewRPCForm.InputDays.Text);

            viewRPCForm.CloseTab("View Risk Profile Check Configuration");
            editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(_riskProfileCheckData);

            Assert.AreEqual(criteria, editRPCForm.DropdownAccountAgeSelected);
            Assert.AreEqual(days, editRPCForm.InputDaysValue);

            //go to the initial state
            editRPCForm.UncheckAccountAge();
        }

        [TestCase("<=", "")]
        public void Can_not_set_risk_profile_account_age_criteria(string criteria, string days)
        {
            var editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(_riskProfileCheckData);
            editRPCForm.SetAccountAgeCheck(criteria, days);
            editRPCForm.Submit();

            Assert.AreEqual("Count must be greater or equals to 1", editRPCForm.ValidationMessage.Text);

            //go to the initial state
            editRPCForm.CloseTab("Edit Risk Profile Check Configuration");

        }

        [TestCase(">=", "25")]
        public void Can_set_risk_profile_deposit_count_criteria(string criteria, string count)
        {
            var editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(_riskProfileCheckData);
            editRPCForm.SetDepositCountCheck(criteria, count);
            var viewRPCForm = editRPCForm.Submit();

            Assert.AreEqual("Risk Profile Check Configuration has been sucessfully updated.", viewRPCForm.SuccessAlert.Text);
            Assert.AreEqual(criteria, viewRPCForm.DropdownDepositCountSelected.Text);
            Assert.AreEqual(count, viewRPCForm.InputDepositCount.Text);

            viewRPCForm.CloseTab("View Risk Profile Check Configuration");
            editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(_riskProfileCheckData);

            Assert.AreEqual(criteria, editRPCForm.DropdownDepositCountSelected);
            Assert.AreEqual(count, editRPCForm.DepositCountValue);

            //go to the initial state
            editRPCForm.UncheckDepositCount();
        }

        [TestCase("<=", "")]
        public void Can_not_set_risk_profile_deposit_count_criteria(string criteria, string count)
        {
            var editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(_riskProfileCheckData);
            editRPCForm.SetDepositCountCheck(criteria, count);
            editRPCForm.Submit();

            Assert.AreEqual("Count must be greater or equals to 1", editRPCForm.ValidationMessage.Text);

            //go to the initial state
            editRPCForm.CloseTab("Edit Risk Profile Check Configuration");

        }

        [TestCase(">=", "25")]
        public void Can_set_risk_profile_withdrawal_count_criteria(string criteria, string count)
        {
            var editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(_riskProfileCheckData);
            editRPCForm.SetWithdrawalCountCheck(criteria, count);
            var viewRPCForm = editRPCForm.Submit();

            Assert.AreEqual("Risk Profile Check Configuration has been sucessfully updated.", viewRPCForm.SuccessAlert.Text);
            Assert.AreEqual(criteria, viewRPCForm.DropdownWithdrawalCountSelected.Text);
            Assert.AreEqual(count, viewRPCForm.InputWithdrawalCount.Text);

            viewRPCForm.CloseTab("View Risk Profile Check Configuration");
            editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(_riskProfileCheckData);

            Assert.AreEqual(criteria, editRPCForm.DropDownWithdrawalCountSelected);
            Assert.AreEqual(count, editRPCForm.WithdrawalCountValue);

            //go to the initial state
            editRPCForm.UncheckWithdrawalCount();
        }

        [TestCase("<=", "")]
        public void Can_not_set_risk_profile_withdrawal_count_criteria(string criteria, string count)
        {
            var editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(_riskProfileCheckData);
            editRPCForm.SetWithdrawalCountCheck(criteria, count);
            editRPCForm.Submit();

            Assert.AreEqual("This field is required.", editRPCForm.ValidationMessage.Text);

            //go to the initial state
            editRPCForm.CloseTab("Edit Risk Profile Check Configuration");
         }

        [TestCase("<=", "8968")]
        public void Can_set_risk_profile_win_loss_criteria(string criteria, string count)
        {
            var editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(_riskProfileCheckData);
            editRPCForm.SetWinLossCheck(criteria, count);
            var viewRPCForm = editRPCForm.Submit();

            Assert.AreEqual("Risk Profile Check Configuration has been sucessfully updated.", viewRPCForm.SuccessAlert.Text);
            Assert.AreEqual(criteria, viewRPCForm.DropdownWinLossSelected.Text);
            Assert.AreEqual(count, viewRPCForm.InputWinLossAmount.Text);

            viewRPCForm.CloseTab("View Risk Profile Check Configuration");
            editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(_riskProfileCheckData);

            Assert.AreEqual(criteria, editRPCForm.DropdownWinLossSelected);
            Assert.AreEqual(count, editRPCForm.WinLossAmountValue);

            //go to the initial state
            editRPCForm.UncheckWinLoss();
        }

        [TestCase(">=", "")]
        public void Can_not_set_risk_profile_win_loss_criteria(string criteria, string count)
        {
            var editRPCForm = _riskProfileCheckConfigurationPage.OpenEditRiskProfileCheckConfigurationForm(_riskProfileCheckData);
            editRPCForm.SetWinLossCheck(criteria, count);
            editRPCForm.Submit();

            Assert.AreEqual("This field is required.", editRPCForm.ValidationMessage.Text);

            //go to the initial state
            editRPCForm.CloseTab("Edit Risk Profile Check Configuration");
        }
    }
}
