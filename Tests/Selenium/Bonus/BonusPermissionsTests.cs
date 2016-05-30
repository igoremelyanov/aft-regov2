using System.Threading;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    [Ignore("Failing unstable on RC-1.0 - Igor, 27-Aiprl-2016")]
    class BonusPermissionsTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private string _bonusTemplateName;

        public override async void BeforeAll()
        {
            base.BeforeAll();
            var bonusHelper = _container.Resolve<BonusTestHelper>();
            var template = await bonusHelper.CreateFirstDepositTemplateAsync(DefaultBrand);
            _bonusTemplateName = template.Info.Name;
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
        }

        [Test]
        public void Can_create_and_view_bonus_with_MarketingOfficer_permissions()
        {
            // create a user with MarketingOfficer permissions
            var bonusName = "Bonus" + TestDataGenerator.GetRandomString(5);
            var bonusCode = TestDataGenerator.GetRandomString(5);
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
               role: "MarketingOfficer", status: "Active", licensee: DefaultLicensee, brand: DefaultBrand, currency: "EUR");
            _driver.CreateUserBasedOnPredefinedRole(userData);
            Thread.Sleep(5000); //wait for new User event proceeds.

            // log in as the user and view bonus
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var bonusManagerPage = _dashboardPage.Menu.ClickBonusMenuItem();
            var newBonusForm = bonusManagerPage.OpenNewBonusForm();
            var submittedForm = newBonusForm.Submit(bonusName, bonusCode, _bonusTemplateName, numberOfdaysToClaimBonus: 20);

            Assert.AreEqual("Bonus has been successfully created.", submittedForm.ConfirmationMessageAfterBonusSaving);
            Assert.AreEqual("View bonus", submittedForm.TabName);
        }

        [Test]
        public void Can_manage_bonus_with_Licensee_permissions()
        {
            // create a user with Licensee permissions
            var bonusName = "Bonus" + TestDataGenerator.GetRandomString(5);
            var bonusCode = TestDataGenerator.GetRandomString(5);
            var editedBonusName = bonusName + "edited";
            var editedBonusCode = bonusCode + "edited";
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
               role: "Licensee", status: "Active", licensee: DefaultLicensee, brand: DefaultBrand, currency: "EUR");
            _driver.CreateUserBasedOnPredefinedRole(userData);
            Thread.Sleep(5000); //wait for new User event proceeds.

            // log in as the user and create a bonus
            _dashboardPage = _driver.LoginToAdminWebsiteAs(userData.UserName, userData.Password);
            var bonusManagerPage = _dashboardPage.Menu.ClickBonusMenuItem();
            var newBonusForm = bonusManagerPage.OpenNewBonusForm();
            var submittedBonusForm = newBonusForm.Submit(bonusName, bonusCode, _bonusTemplateName, numberOfdaysToClaimBonus:0);

            Assert.AreEqual("Bonus has been successfully created.", submittedBonusForm.ConfirmationMessageAfterBonusSaving);
            
            //view the bonus
            Assert.AreEqual(bonusName, submittedBonusForm.BonusName);
            Assert.AreEqual(bonusCode, submittedBonusForm.BonusCode);
            Assert.AreEqual(_bonusTemplateName, submittedBonusForm.BonusTemplate);
            
            // edit the bonus
            submittedBonusForm.CloseTab();
            var editBonusForm = bonusManagerPage.OpenEditBonusForm(bonusName);
            editBonusForm.SetNameAndCode(editedBonusName, editedBonusCode);
            var submittedForm = editBonusForm.Submit();

            Assert.AreEqual("Bonus has been successfully edited.", submittedForm.ConfirmationMessageAfterBonusEditing);
        }
    }
}