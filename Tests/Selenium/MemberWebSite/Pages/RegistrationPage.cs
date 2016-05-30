using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Selenium.MemberWebSite.Pages
{
    class RegistrationPage : BasePage
    {
        public RegistrationPage(IWebDriver driver)
            : base(driver)
        {
        }

        public override String PageTitle()
        {
            return "Welcome to rego";
        }

        #region Locators
#pragma warning disable 649
        [FindsBy(How = How.Id, Using = "username-register")]
        private IWebElement _username;

        [FindsBy(How = How.Id, Using = "first-name")]
        private IWebElement _firstName;

        [FindsBy(How = How.Id, Using = "last-name")]
        private IWebElement _lastName;

        [FindsBy(How = How.Id, Using = "email")]
        private IWebElement _email;

        [FindsBy(How = How.Id, Using = "phone-number")]
        private IWebElement _phoneNumber;

        [FindsBy(How = How.Id, Using = "password-register")]
        private IWebElement _password;

        [FindsBy(How = How.Id, Using = "password-confirm")]
        private IWebElement _passwordConfirm;

        [FindsBy(How = How.Id, Using = "day-of-birth")]
        private IWebElement _dayOfBirth;

        [FindsBy(How = How.Id, Using = "month-of-birth")]
        private IWebElement _monthOfBirth;

        [FindsBy(How = How.Id, Using = "year-of-birth")]
        private IWebElement _yearOfBirth;

        [FindsBy(How = How.Id, Using = "mailingAddressLine1")]
        private IWebElement _address;

        [FindsBy(How = How.Id, Using = "mailingAddressPostalCode")]
        private IWebElement _postalCode;

        [FindsBy(How = How.Id, Using = "country")]
        private IWebElement _country;

        [FindsBy(How = How.Id, Using = "currency")]
        private IWebElement _currency;

        //[FindsBy(How = How.Id, Using = "title")]
        //private IWebElement _title;

        //[FindsBy(How = How.Id, Using = "mailingAddressCity")]
        //private IWebElement _city;

        //[FindsBy(How = How.Id, Using = "contactPreference")]
        //private IWebElement _contactPreference;

        //[FindsBy(How = How.Id, Using = "register-btn")]
        //private IWebElement _registerButton;

        //[FindsBy(How = How.Id, Using = "securityQuestionId")]
        //private IWebElement _securityQuestion;

        //[FindsBy(How = How.Id, Using = "securityAnswer")]
        //private IWebElement _securityAnswer;

        //[FindsBy(How = How.XPath, Using = "//a[@href='/en-US/Home/Index']")]
        //private IWebElement _playerProfileUrl;

        [FindsBy(How = How.XPath, Using = "//input[@value='Male']")]
        private IWebElement _radioMale;

        [FindsBy(How = How.XPath, Using = "//input[@value='Female']")]
        private IWebElement _radioFemale;

        [FindsBy(How = How.Id, Using = "over18")]
        private IWebElement _over18;

        [FindsBy(How = How.Id, Using = "acceptTerms")]
        private IWebElement _acceptTerms;

        [FindsBy(How = How.Id, Using = "mailingAddressStatProvince")]
        private IWebElement _stateProvince;
#pragma warning restore 649
        #endregion

        #region validation messages

        [FindsBy(How = How.XPath, Using = "//div[@id='register-messages']//ul")]
        public IWebElement ErrorMessages { get; private set; }
        #endregion


        public void EnterRegistrationData()
        {
            EnterText(_username, "test");
            EnterText(_password, "test");
            EnterText(_passwordConfirm, "test");
            EnterText(_email, "test@mail.com");
            //_email.SendKeys(data.Email);
            //_phoneNumber.SendKeys(data.PhoneNumber);
            EnterText(_phoneNumber, "0987654321");
            _radioMale.Click();
            _radioFemale.Click();
            _firstName.SendKeys("");
            _lastName.SendKeys("");
            _dayOfBirth.SendKeys("");
            _monthOfBirth.SendKeys("");
            _yearOfBirth.SendKeys("");


            //_securityAnswer.SendKeys("");

            _country.SendKeys("");
            _address.SendKeys("");


            _postalCode.SendKeys("");
            //_city.SendKeys("");
            _stateProvince.SendKeys("");
            _currency.SendKeys("");
            _over18.Click();
            _acceptTerms.Click();
        }
    }
}
