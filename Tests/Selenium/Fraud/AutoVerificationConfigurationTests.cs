using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud.AutoVerificationConfiguration;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium.Fraud
{
    /// <summary>
    /// Represents tests related to Fraud -> Auto Verification Configuration
    /// </summary>
    ///
    [Ignore("Svitlana: 02/15/2016; Ignored without investigation (probably changing in UI), will be investigated when Fraud subdomain will be back; AFTREGO-4260 Hide Fraud / Risk UI components in order to isolate functionality for R1.0")]
    class AutoVerificationConfigurationTests : SeleniumBaseForAdminWebsite
    {
        private AutoVerificationConfigurationPage _autoVerificationConfigurationPage;
        private NewAutoVerificationConfigurationForm _newAvcForm;
        private DashboardPage _dashboardPage;
        private VipLevelData _vipLevelData;
        private AutoVerificationConfigurationData _avcData;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private const string DefaultCurrency = "CAD";

        public override void BeforeAll()
        {
            base.BeforeAll();

            //create vip level for a brand
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var vipLevelManagerPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            var newVipLevelPage = vipLevelManagerPage.OpenNewVipLevelForm();
            _vipLevelData = TestDataGenerator.CreateValidVipLevelData(DefaultLicensee, DefaultBrand, false);
            var submittedVipLevelForm = newVipLevelPage.Submit(_vipLevelData);
            submittedVipLevelForm.CloseTab("View VIP Level"); 
            _dashboardPage.CloseTab("VIP Level Manager");

            //generate auto verification configuration form data
            _avcData = TestDataGenerator.CreateAutoVerificationConfigurationData(
                 DefaultLicensee,
                 DefaultBrand,
                 DefaultCurrency,
                 _vipLevelData.Name
               );
            
            //create new AVC
            _autoVerificationConfigurationPage = submittedVipLevelForm.Menu.ClickAutoVerificationConfiguration();
            _newAvcForm = _autoVerificationConfigurationPage.OpenNewAutoVerificationForm();
            _newAvcForm.SetAutoVerificationConfigurationFields(_avcData);
            _newAvcForm.SubmitAutoVerificationConfiguration().CloseTab("View Auto Verification Configuration");

        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _autoVerificationConfigurationPage = _dashboardPage.Menu.ClickAutoVerificationConfiguration();
        }


        [Test]
        public void Can_edit_auto_verification_configuration_via_UI()
        {
            //generate auto verification configuration edited form data
            var avcDataEdited = TestDataGenerator.CreateAutoVerificationConfigurationData(
                 DefaultLicensee,
                 DefaultBrand,
                 "RMB",
                 _vipLevelData.Name
               );

            //edit avc
            var editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);
            editAvcForm.EditAutoVerificationConfigurationFields(_avcData, avcDataEdited);
            var viewAvcForm = editAvcForm.Submit();

            Assert.AreEqual("Auto Verification Configurations has been successfully updated.", viewAvcForm.SuccessAlert.Text);
            Assert.AreEqual(avcDataEdited.Licensee, viewAvcForm.Licensee.Text);
            Assert.AreEqual(avcDataEdited.Brand, viewAvcForm.Brand.Text);
            Assert.AreEqual(avcDataEdited.Currency, viewAvcForm.Currency.Text);
            Assert.AreEqual(avcDataEdited.VipLevel, viewAvcForm.GetSelectedVipLevels()[0]);

            viewAvcForm.CloseTab("View Auto Verification Configuration");

            //move to the initial state
            editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(avcDataEdited);
            editAvcForm.EditAutoVerificationConfigurationFields(avcDataEdited, _avcData);
            viewAvcForm = editAvcForm.Submit();

            Assert.AreEqual("Auto Verification Configurations has been successfully updated.", viewAvcForm.SuccessAlert.Text);
        }


        [TestCase(">=", "200.59")]
        public void Can_set_AVC_total_deposit_amount_criteria(string criteria, string amount)
        {
            var editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);
            editAvcForm.SetTotalDepositAmountCheck(criteria, amount);
            var viewAvcForm = editAvcForm.Submit();

            Assert.AreEqual("Auto Verification Configurations has been successfully updated.", viewAvcForm.SuccessAlert.Text);
            Assert.AreEqual(criteria, viewAvcForm.DropdownTotalDepositAmountSelected.Text);
            Assert.AreEqual(amount, viewAvcForm.InputTotalDepositAmount.Text);

            viewAvcForm.CloseTab("View Auto Verification Configuration");
            editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);

            Assert.AreEqual(criteria, editAvcForm.DropdownTotalDepositAmountSelected);
            Assert.AreEqual(amount, editAvcForm.TotalDepositAmountValue);

            //go to the initial state
            editAvcForm.UnchecklDepositAmount();
        }

        [TestCase("<=", "")]
        public void Can_not_set_AVC_total_deposit_amount_criteria(string criteria, string amount)
        {
            var editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);
            editAvcForm.SetTotalDepositAmountCheck(criteria, amount);
            editAvcForm.Submit();

            Assert.AreEqual("This field is required.", editAvcForm.ValidationMessage.Text);

            //go to the initial state
            editAvcForm.CloseTab("Edit Auto Verification Configuration");
        }

        [TestCase(">=", "25")]
        public void Can_set_AVC_account_age_criteria(string criteria, string days)
        {
            var editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);
            editAvcForm.SetAccountAgeCheck(criteria, days);
            var viewAvcForm = editAvcForm.Submit();

            Assert.AreEqual("Auto Verification Configurations has been successfully updated.", viewAvcForm.SuccessAlert.Text);
            Assert.AreEqual(criteria, viewAvcForm.DropdownAccountAgeSelected.Text);
            Assert.AreEqual(days, viewAvcForm.InputDays.Text);

            viewAvcForm.CloseTab("View Auto Verification Configuration");
            editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);

            Assert.AreEqual(criteria, editAvcForm.DropdownAccountAgeSelected);
            Assert.AreEqual(days, editAvcForm.InputDaysValue);

            //go to the initial state
            editAvcForm.UncheckAccountAge();

        }

        [TestCase("<=", "")]
        public void Can_not_set_AVC_account_age_criteria(string criteria, string days)
        {
            var editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);
            editAvcForm.SetAccountAgeCheck(criteria, days);
            editAvcForm.Submit();

            Assert.AreEqual("This field is required.", editAvcForm.ValidationMessage.Text);

            //go to the initial state
            editAvcForm.CloseTab("Edit Auto Verification Configuration");
        }

        [TestCase(">=", "25")]
        public void Can_set_AVC_deposit_count_criteria(string criteria, string count)
        {
            var editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);
            editAvcForm.SetTotalDepositCountCheck(criteria, count);
            var viewAvcForm = editAvcForm.Submit();

            Assert.AreEqual("Auto Verification Configurations has been successfully updated.", viewAvcForm.SuccessAlert.Text);
            Assert.AreEqual(criteria, viewAvcForm.DropdownDepositCountSelected.Text);
            Assert.AreEqual(count, viewAvcForm.InputDepositCount.Text);

            viewAvcForm.CloseTab("View Auto Verification Configuration");
            editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);

            Assert.AreEqual(criteria, editAvcForm.DropdownDepositCountSelected);
            Assert.AreEqual(count, editAvcForm.DepositCountValue);

            //go to the initial state
            editAvcForm.UncheckDepositCount();
        }

        [TestCase("<=", "")]
        public void Can_not_set_AVC_deposit_count_criteria(string criteria, string count)
        {
            var editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);
            editAvcForm.SetTotalDepositCountCheck(criteria, count);
            editAvcForm.Submit();

            Assert.AreEqual("This field is required.", editAvcForm.ValidationMessage.Text);

            //go to the initial state
            editAvcForm.CloseTab("Edit Auto Verification Configuration");
        }

        [TestCase(">=", "999")]
         public void Can_set_AVC_withdrawal_count_criteria(string criteria, string count)
        {
            var editAVCForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);
            editAVCForm.SetWithdrawalCountCheck(criteria, count);
            var viewAVCForm = editAVCForm.Submit();

            Assert.AreEqual("Auto Verification Configurations has been successfully updated.", viewAVCForm.SuccessAlert.Text);
            Assert.AreEqual(criteria, viewAVCForm.DropdownWithdrawalCountSelected.Text);
            Assert.AreEqual(count, viewAVCForm.InputWithdrawalCount.Text);

            viewAVCForm.CloseTab("View Auto Verification Configuration");
            editAVCForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);

            Assert.AreEqual(criteria, editAVCForm.DropDownWithdrawalCountSelected);
            Assert.AreEqual(count, editAVCForm.WithdrawalCountValue);

            //go to the initial state
            editAVCForm.UncheckWithdrawalCount();
        }

        [TestCase("<=", "")]
         public void Can_not_set_AVC_profile_withdrawal_count_criteria(string criteria, string count)
        {
            var editAVCForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);
            editAVCForm.SetWithdrawalCountCheck(criteria, count);
            editAVCForm.Submit();

            Assert.AreEqual("Count must be greater or equals to 1", editAVCForm.ValidationMessage.Text);

            //go to the initial state
            editAVCForm.CloseTab("Edit Auto Verification Configuration");

        }

        [TestCase("<=", "895")]
        public void Can_set_AVC_win_loss_criteria(string criteria, string count)
        {
            var editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);
            editAvcForm.SetWinLossCheck(criteria, count);
            var viewAvcForm = editAvcForm.Submit();

            Assert.AreEqual("Auto Verification Configurations has been successfully updated.", viewAvcForm.SuccessAlert.Text);
            Assert.AreEqual(criteria, viewAvcForm.DropdownWinLossSelected.Text);
            Assert.AreEqual(count, viewAvcForm.InputWinLossAmount.Text);

            viewAvcForm.CloseTab("View Auto Verification Configuration");
            editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);

            Assert.AreEqual(criteria, editAvcForm.DropdownWinLossSelected);
            Assert.AreEqual(count, editAvcForm.WinLossAmountValue);

            //go to the initial state
            editAvcForm.UncheckWinLoss();
        }

        [TestCase(">=", "")]
        public void Can_not_set_AVC_win_loss_criteria(string criteria, string count)
        {
            var editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);
            editAvcForm.SetWinLossCheck(criteria, count);
            editAvcForm.Submit();

            Assert.AreEqual("This field is required.", editAvcForm.ValidationMessage.Text);

            //go to the initial state
            editAvcForm.CloseTab("Edit Auto Verification Configuration");
        }

        [Test]
        public void Can_not_create_duplicate_auto_verification_configuration()
        {
            _newAvcForm = _autoVerificationConfigurationPage.OpenNewAutoVerificationForm();
            _newAvcForm.SetAutoVerificationConfigurationFields(_avcData);
            _newAvcForm.SubmitAutoVerificationConfiguration();

            var failAvc = new AutoVerificationConfigurationFailure(_driver);

            Assert.True(failAvc.ErrorAlert.Displayed);
            Assert.AreEqual(failAvc.ErrorAlert.Text, "You have already set up Auto Verification Check with the selected Brand, " +
                                                     "Currency and Vip level. Please, update the existing one or change your form data.");
            failAvc.CloseTab("Auto Verification Configuration Failure");
        }

        [Test]
        public void Can_activate_deactivate_auto_verification_configuration()
        {
            //Activate AVC
            var _confirmAVCModal = _autoVerificationConfigurationPage.ActivateAutoVerificationConfiguration(_avcData);
            Assert.AreEqual("Auto verification configuration activated successfully",_confirmAVCModal.SuccessAlert.Text);

            //Close modal
            _confirmAVCModal.CloseConfirmationModal();
            Assert.AreEqual("Active", _autoVerificationConfigurationPage.GetAVCStatus(_avcData));

            //Deactivate AVC
            _confirmAVCModal = _autoVerificationConfigurationPage.DeactivateAutoVerificationConfiguration(_avcData);
            Assert.AreEqual("Auto verification configuration deactivated successfully",_confirmAVCModal.SuccessAlert.Text);
            //Close modal
            _confirmAVCModal.CloseConfirmationModal();
            Assert.AreEqual("Inactive", _autoVerificationConfigurationPage.GetAVCStatus(_avcData));
        }

        [Test]
        public void Can_cancel_activation_auto_verification_configuration()
        {
     
            _autoVerificationConfigurationPage.CancelActivationAutoVerificationConfiguration(_avcData);

            Assert.AreEqual("Inactive", _autoVerificationConfigurationPage.GetAVCStatus(_avcData));
        }

        [Test]
        public void Can_set_AVC_payment_level_criteria()
        {
            var editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);
            editAvcForm.SetHasPaymentLevel();
            var viewAvcForm = editAvcForm.Submit();

            Assert.AreEqual("Auto Verification Configurations has been successfully updated.", viewAvcForm.SuccessAlert.Text);

            //go to the initial state
            editAvcForm.CloseTab("View Auto Verification Configuration");
        }

        [Test]
        public void Can_set_no_AVC_payment_level_criteria()
        {
            var editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(_avcData);
            editAvcForm.NotSetHasPaymentLevel();
            var viewAvcForm = editAvcForm.Submit();

            Assert.AreEqual("Auto Verification Configurations has been successfully updated.", viewAvcForm.SuccessAlert.Text);

            //go to the initial state
            editAvcForm.CloseTab("View Auto Verification Configuration");
        }
    }
}
