using System;
using System.Threading;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class PlayerProfilePage : FrontendPageBase
    {
        public PlayerProfilePage(IWebDriver driver) : base(driver) {}

        protected override string GetPageUrl()
        {
            return "en-US/Home/PlayerProfile";  //MyAccount page
        }

        public MemberWebsiteLoginPage Logout()
        {
            _driver.WaitForJavaScript();
            _settingsDropdown.Click();
            _logoutUrl.Click();
            return new MemberWebsiteLoginPage(_driver);
        }

        public string GetUserName()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            wait.Until(d =>
            {
                try
                {
                    Initialize();
                    return Username.Displayed;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
            return Username.Text;
        }

        public void ChangePassword(string oldPassword)
        {
            ChangePassword(oldPassword, "new-password");
        }


        public void EnterOnlyNewPassword()
        {
            ChangePassword(null, "new-password");
        }

        public void EnterNewPassword(String currentPassword, string password)
        {
            ChangePassword(currentPassword, password);
        }

        public void EnterNewPasswordWithoutConfirmPassword(String currentPassword)
        {
            ChangePassword(currentPassword, "new-password", string.Empty);
        }

        public void EnterNewPasswordWithIncorrectConfirmPassword(String currentPassword)
        {
            ChangePassword(currentPassword, "new-password", "another-password");
        }

        private void ChangePassword(string oldPassword, string newPassword, string confirmPassword = null)
        {
            _driver.ScrollToElement(_changePasswordBtn);
            _driver.WaitAndClickElement(_changePasswordBtn);            

            if (oldPassword != null)
            {
                _driver.ScrollToElement(_oldPassword);
                _oldPassword.SendKeys(oldPassword);
            }
            if (newPassword != null)
            {
                _newPassword.SendKeys(newPassword);
                _confirmPassword.SendKeys(confirmPassword ?? newPassword);
            }
            _savePasswordBtn.Click();
        }

        public string GetPasswordChangedMsg()
        {
            Console.WriteLine(_passwordChangeSuccessMsg.Text);
            return _passwordChangeSuccessMsg.Text;
        }

        public string GetPasswordChangedErrorMsg()
        {
            return _passwordChangeErrorMsg.Text;
        }

        public string GetCurrentPasswordChangedErrorMsg()
        {
            return _currentPasswordChangeErrorMsg.Text;
        }

        public string GetNewPasswordChangedErrorMsg()
        {
            return _newPasswordChangeErrorMsg.Text;
        }

        public string GetConfirmPasswordChangedErrorMsg()
        {
            return _confirmpasswordChangeErrorMsg.Text;
        }

        public void RequestMobileVerificationCode()
        {
            _editContactInformationBtn.Click();
            _requestMobileVerificationCodeBtn.Click();
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(driver => _verifyMobileSucessLabel.Text == "Verification code has been sent.");
            _saveContactInformationBtn.Click();
        }

        public void VerifyMobileNumber(int mobileVerificationCode)
        {
            _editContactInformationBtn.Click();
            _mobileVerificationCodeField.SendKeys(mobileVerificationCode.ToString("D4"));
            _verifyMobileNumberBtn.Click();
            _saveContactInformationBtn.Click();
        }

        #region player profile
#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//div/a[contains(@class, 'settings drop')]")]
        private IWebElement _settingsDropdown;

        //[FindsBy(How = How.XPath, Using = "//a[contains(., 'my detail')]")]
        //private IWebElement _myDetailsButton;

        // "//button[@data-bind='click: logout']")]
        [FindsBy(How = How.XPath, Using = "//li/a[contains(@data-bind, 'click: logout')]")]
        private IWebElement _logoutUrl;
                                           //"//span[@data-bind='text: account.username']"
        [FindsBy(How = How.XPath, Using = "//div[@class='col-sm-9 white-content my-detail']//following-sibling::p[text()='Welcome ']/b")]
        public IWebElement Username { get; private set; }

        [FindsBy(How = How.XPath, Using = "//p[@data-bind=\"text: personal.firstName() + ' ' + personal.lastName()\"]")]
        public IWebElement FullName { get; private set; }
#region not available any more
        [FindsBy(How = How.XPath, Using = "//span[contains(@data-bind, 'text: personal.firstName')]")]
        public IWebElement FirstName { get; private set; }

        [FindsBy(How = How.XPath, Using = "//span[contains(@data-bind, 'text: personal.lastName')]")]
        public IWebElement LastName { get; private set; }

        [FindsBy(How = How.XPath, Using = "//span[contains(@data-bind, 'text: personal.dateOfBirth')]")]
        public IWebElement DateOfBirth { get; private set; }
#endregion
        [FindsBy(How = How.XPath, Using = "//*[contains(@data-bind, 'text: contacts.phoneNumber')]")]
        public IWebElement PhoneNumber { get; private set; }

        [FindsBy(How = How.XPath, Using = "//*[contains(@data-bind, 'text: personal.email')]")]
        public IWebElement Email { get; private set; }

        [FindsBy(How = How.XPath, Using = "//*[contains(@data-bind, 'text: contacts.address')]")]
        public IWebElement Address { get; private set; }

        [FindsBy(How = How.XPath, Using = "//*[contains(@data-bind, 'text: contacts.postalCode')]")]
        public IWebElement ZipCode { get; private set; }

        [FindsBy(How = How.XPath, Using = "//*[contains(@data-bind, 'text: contacts.countryCode')]")]
        public IWebElement CountryCode { get; private set; }

        [FindsBy(How = How.XPath, Using = "//*[contains(@data-bind, 'text: personal.currencyCode')]")]
        public IWebElement CurrencyCode { get; private set; }

        [FindsBy(How = How.Id, Using = "account-current-password")]
        private IWebElement _oldPassword;

        [FindsBy(How = How.Id, Using = "account-newPassword")]
        private IWebElement _newPassword;

        [FindsBy(How = How.Id, Using = "account-confirmPassword")]
        private IWebElement _confirmPassword;

        [FindsBy(How = How.Id, Using = "changePasswordBtn")]
        private IWebElement _savePasswordBtn;

        [FindsBy(How = How.XPath, Using = "//label[@class=\"ky-notification notification-success\"]")]
        public IWebElement _passwordChangeSuccessMsg { get; private set; }

        //[FindsBy(How = How.XPath, Using = "//label[contains(@data-bind, 'text: account.errorMessage')]")]
        [FindsBy(How = How.XPath, Using = "//p[@class='msg']")]
        private IWebElement _passwordChangeErrorMsg;

        [FindsBy(How = How.XPath, Using = "//p[@data-bind='validationMessage: account.currentPassword']")]
        private IWebElement _currentPasswordChangeErrorMsg;

        [FindsBy(How = How.XPath, Using = "//p[@data-bind='validationMessage: account.newPassword']")]
        private IWebElement _newPasswordChangeErrorMsg;

        [FindsBy(How = How.XPath, Using = "//p[@data-bind='validationMessage: account.confirmPassword']")]
        private IWebElement _confirmpasswordChangeErrorMsg;

        [FindsBy(How = How.XPath, Using = "//button[contains(@data-bind, 'click: verification.requestCode')]")]
        private IWebElement _requestMobileVerificationCodeBtn;

        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: verification.code')]")]
        private IWebElement _mobileVerificationCodeField;

        [FindsBy(How = How.XPath, Using = "//button[contains(@data-bind, 'click: verification.verifyPhoneNumber')]")]
        private IWebElement _verifyMobileNumberBtn;

        [FindsBy(How = How.XPath, Using = "//label[contains(@data-bind, 'text: verification.successMessage')]")]
        private IWebElement _verifyMobileSucessLabel;

        [FindsBy(How = How.XPath, Using = "//button[contains(@data-bind, 'click: contacts.edit')]")]
        private IWebElement _editContactInformationBtn;

        [FindsBy(How = How.XPath, Using = "//button[contains(@data-bind, 'click: contacts.save')]")]
        private IWebElement _saveContactInformationBtn;

        [FindsBy(How = How.XPath, Using = "//button[contains(., 'Change Password')]")] 
        private IWebElement _changePasswordBtn;


#pragma warning restore 649

        #endregion
    }
}