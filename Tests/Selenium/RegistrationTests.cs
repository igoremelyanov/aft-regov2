using System.Linq;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    internal class PlayerRegistrationTests : SeleniumBaseForMemberWebsite
    {
        private RegisterPage _registerPage;
        private RegistrationDataForMemberWebsite _data;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _registerPage = new RegisterPage(_driver);
            _registerPage.NavigateToMemberWebsite();
            _data = TestDataGenerator.CreateValidPlayerDataForMemberWebsite("CAD", "en-US", "CA");
        }

        public void CanOpenRegisterPage()
        {
            Assert.That(_registerPage.Url.ToString(), Is.StringContaining("Register"));
        }

        [Test]
        public void Can_register_on_member_website()
        {
            var registrationSuccess = _registerPage.Register(_data);
            var playerProfilePage = _registerPage.GoToPlayerProfile();
            Assert.That(playerProfilePage.Url.ToString(), Is.StringContaining("PlayerProfile"));
        }

        [Test]
        public void Cannot_register_with_duplicate_data()
        {
            _registerPage.Register(_data);
            var playerProfilePage = _registerPage.GoToPlayerProfile();
            playerProfilePage.Logout();

            _registerPage = new RegisterPage(_driver);
            _driver.WaitForJavaScript();
            _registerPage.NavigateToMemberWebsite();
            _registerPage.SubmitRegistrationForm(_data);

            Assert.AreEqual("Username already exists", _registerPage.GetUserNameValidationMessage());
            Assert.AreEqual("The email already exists", _registerPage.GetEmailValidationMessage());
        }

        [Test]
        public void Cannot_register_using_only_spaces()
        {
            var data = TestDataGenerator.CreateRegistrationDataWithSpacesOnly();
            _registerPage.RegisterWithInvalidData(data);

            Assert.False(_registerPage.IsRegisterButtonEnabled());
        }

        [Test]
        public void Cannot_register_with_data_exceeding_max_limit()
        {
           var data = TestDataGenerator.RegistrationDataExceedsMaxLimit();
            _registerPage.RegisterWithInvalidData(data);

            Assert.False(_registerPage.IsRegisterButtonEnabled());
        }
    }
}
