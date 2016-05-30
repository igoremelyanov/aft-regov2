using System.Globalization;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class PaymentGatewaySettingsTests : SeleniumBaseForAdminWebsite
    {
        private PaymentGatewaySettingsPage _paymentGatewaySettingsPage;
        private DashboardPage _dashboardPage;
        
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138"; 

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();

            _paymentGatewaySettingsPage = _dashboardPage.Menu.ClickPaymentGatewaySettingsMenuItem();
        }

        [Test]
        public void Can_add_payment_gateway_settings()
        {
            var addForm = _paymentGatewaySettingsPage.OpenNewPaymentGatewaySettingsForm();
            var settingsData = TestDataGenerator.CreateValidPaymentGatewaySettingsData(
                 DefaultLicensee,
                 DefaultBrand,
                 onlinePaymentMethodName: TestDataGenerator.GetRandomString(30),
                 paymentGatewayName: "XPAY",
                 channel: TestDataGenerator.GetRandomNumber(99999, 1).ToString(CultureInfo.InvariantCulture),
                 entryPoint:"http://shop.domain.com/payment/issue",
                 remarks: "Add new setting-Can_add_payment_gateway_settings"
            );
            var submittedPage = addForm.Submit(settingsData);

            Assert.AreEqual("Payment gateway settings have been successfully created.", submittedPage.ConfirmationMessage);
            Assert.AreEqual(DefaultLicensee, submittedPage.Licensee);
            Assert.AreEqual(DefaultBrand, submittedPage.Brand);
            Assert.AreEqual(settingsData.OnlinePaymentMethodName, submittedPage.OnlinePaymentMethodName);
            Assert.AreEqual(settingsData.PaymentGatewayName, submittedPage.PaymentGatewayName);
            Assert.AreEqual(settingsData.Channel, submittedPage.Channel);
            Assert.AreEqual(settingsData.EntryPoint, submittedPage.EntryPoint);
            Assert.AreEqual(settingsData.Remarks, submittedPage.Remarks);
        }

        [Test]
        public void Can_edit_payment_gateway_settings()
        {
            var addForm = _paymentGatewaySettingsPage.OpenNewPaymentGatewaySettingsForm();
            var settingsData = TestDataGenerator.CreateValidPaymentGatewaySettingsData(
                 DefaultLicensee,
                 DefaultBrand,
                 onlinePaymentMethodName: TestDataGenerator.GetRandomString(30),
                 paymentGatewayName: "XPAY",
                 channel: TestDataGenerator.GetRandomNumber(99999, 1).ToString(CultureInfo.InvariantCulture),
                 entryPoint: "http://shop.domain.com/payment/issue",
                 remarks: "Add setting-Can_edit_payment_gateway_settings"
            );
            var submittedPage = addForm.Submit(settingsData);

            Assert.AreEqual("Payment gateway settings have been successfully created.", submittedPage.ConfirmationMessage);

            submittedPage.CloseTab("View Payment Gateway Settings");

            var editForm = _paymentGatewaySettingsPage.OpenEditPaymentGatewaySettingsForm(DefaultBrand, settingsData.OnlinePaymentMethodName);
            //modify data
            settingsData.EntryPoint="http://modifiedshop.domain.com/payment/issue";
            settingsData.Remarks = "Edit setting-Can_edit_payment_gateway_settings, name=" + settingsData.OnlinePaymentMethodName + ",channele=" + settingsData.Channel;

            var submittedEditPaymentSettingsForm = editForm.Submit(settingsData);

            Assert.AreEqual("Payment gateway settings have been successfully updated.", submittedEditPaymentSettingsForm.ConfirmationMessage);
            Assert.AreEqual(DefaultBrand, submittedEditPaymentSettingsForm.Brand);
            Assert.AreEqual(DefaultLicensee, submittedEditPaymentSettingsForm.Licensee);            
            Assert.AreEqual(settingsData.OnlinePaymentMethodName, submittedPage.OnlinePaymentMethodName);
            Assert.AreEqual(settingsData.PaymentGatewayName, submittedPage.PaymentGatewayName);
            Assert.AreEqual(settingsData.Channel, submittedPage.Channel);
            Assert.AreEqual(settingsData.EntryPoint, submittedPage.EntryPoint);
            Assert.AreEqual(settingsData.Remarks, submittedPage.Remarks);
        }

        [Test]
        public void Can_activate_deactive_payment_gateway_settings()
        {
            var addForm = _paymentGatewaySettingsPage.OpenNewPaymentGatewaySettingsForm();
            var settingsData = TestDataGenerator.CreateValidPaymentGatewaySettingsData(
                 DefaultLicensee,
                 DefaultBrand,
                 onlinePaymentMethodName: TestDataGenerator.GetRandomString(30),
                 paymentGatewayName: "XPAY",
                 channel: TestDataGenerator.GetRandomNumber(99999, 1).ToString(CultureInfo.InvariantCulture),
                 entryPoint: "http://shop.domain.com/payment/issue",
                 remarks: "Add setting-Can_activate_deactive_payment_gateway_settings"
            );
            var submittedPage = addForm.Submit(settingsData);

            Assert.AreEqual("Payment gateway settings have been successfully created.", submittedPage.ConfirmationMessage);
            submittedPage.CloseTab("View Payment Gateway Settings");
            
            _paymentGatewaySettingsPage.Activate(DefaultBrand, settingsData.OnlinePaymentMethodName);

            var status = _paymentGatewaySettingsPage.GetStatus(DefaultBrand, settingsData.OnlinePaymentMethodName);
            Assert.AreEqual("Active", status);

            _paymentGatewaySettingsPage.Deactivate(DefaultBrand, settingsData.OnlinePaymentMethodName);
            status = _paymentGatewaySettingsPage.GetStatus(DefaultBrand, settingsData.OnlinePaymentMethodName);
            Assert.AreEqual("Inactive", status);
        }
    }
}
