using System;
using System.Threading;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    internal class PlayerProfileOnMemberWebsiteTests : SeleniumBaseForMemberWebsite
    {
        private RegisterPage _registerPage;
        private MemberWebsiteLoginPage _loginPage;
        private PlayerProfilePage _playerProfilePage;
        private static string _correctUsername;
        private static string _correctPassword;
        private RegistrationDataForMemberWebsite _data;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _data = TestDataGenerator.CreateValidPlayerDataForMemberWebsite();
            _correctUsername = _data.Username;
            _correctPassword = _data.Password;
            _registerPage = new RegisterPage(_driver);
            _registerPage.NavigateToMemberWebsite();
            _registerPage.Register(_data);
            
            _loginPage = new MemberWebsiteLoginPage(_driver);
            _loginPage.NavigateToMemberWebsite();
            _driver.WaitForJavaScript();
            _playerProfilePage = _loginPage.Login(_correctUsername, _correctPassword);
            _driver.WaitForJavaScript();
        }

        [Test]
        public void Can_view_player_info()
        {
            Assert.AreEqual(_data.Username+" !", _playerProfilePage.Username.Text);
            Assert.AreEqual(_data.FirstName+" "+_data.LastName, _playerProfilePage.FullName.Text);            
            Assert.AreEqual(_data.Email, _playerProfilePage.Email.Text);
            Assert.AreEqual(_data.Address, _playerProfilePage.Address.Text);
            Assert.AreEqual(_data.PostalCode, _playerProfilePage.ZipCode.Text);
        }

        [Test]
        public void Can_change_password()
        {
            _playerProfilePage.ChangePassword(_correctPassword);
            _driver.WaitForJavaScript();
            Assert.AreEqual(_playerProfilePage.GetPasswordChangedMsg(), "Password has been changed successfully.");

            _driver.Manage().Cookies.DeleteAllCookies();
            _loginPage = new MemberWebsiteLoginPage(_driver);
            _loginPage.NavigateToMemberWebsite();
            _driver.WaitForJavaScript();
            var currentPage = _playerProfilePage = _loginPage.Login(_correctUsername, "new-password");
            Assert.That(currentPage.GetUserName(), Is.StringContaining(_correctUsername));
        }

        [Test]
        public void Cannot_change_password_to_short_one()
        {
            var password = TestDataGenerator.GetRandomStringWithSpecialSymbols(5, ".-'");
            _playerProfilePage.EnterNewPassword(_correctPassword, password);
            _driver.WaitForJavaScript();
            Assert.That(_playerProfilePage.GetNewPasswordChangedErrorMsg(), Is.StringContaining("Password must contain at least 6 characters and not more then 12"));
        }

        [Test]
        [Ignore("Till Volodimir's investigations, 21-April-2016")]
        public void Cannot_change_password_to_long_one()
        {
            var password = TestDataGenerator.GetRandomStringWithSpecialSymbols(13, ".-'");
            _playerProfilePage.EnterNewPassword(_correctPassword, password);
            _driver.WaitForJavaScript();
            Assert.That(_playerProfilePage.GetNewPasswordChangedErrorMsg(), Is.StringContaining("Password must contain at least 6 characters and not more then 12"));
        }

        [Test]
        public void Cannot_change_password_without_old_one()
        {
            _playerProfilePage.EnterOnlyNewPassword();
            _driver.WaitForJavaScript();
            Assert.AreEqual("Field is required", _playerProfilePage.GetPasswordChangedErrorMsg());
        }

        [Test]
        public void Cannot_change_password_without_confirming_password()
        {
            _playerProfilePage.EnterNewPasswordWithoutConfirmPassword(_correctPassword);
            _driver.WaitForJavaScript();
            Assert.AreEqual("Field is required", _playerProfilePage.GetConfirmPasswordChangedErrorMsg());
        }

        [Test]
        public void Cannot_change_password_with_incorrect_confirm_password()
        {
            _playerProfilePage.EnterNewPasswordWithIncorrectConfirmPassword(_correctPassword);
            _driver.WaitForJavaScript();
            Assert.AreEqual("New password should match confirm password.", _playerProfilePage.GetConfirmPasswordChangedErrorMsg());
        }
    }
}
