using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class RegisterPage : FrontendPageBase
    {
        public RegisterPage(IWebDriver driver) : base(driver) { }

        protected override string GetPageUrl()
        {
            return "en-US/Home/Register";
        }

        public virtual void NavigateToMemberWebsite(string referralId)
        {
            var url = GetMemberWebsiteUrl() + string.Format("{0}?referralId={1}", GetPageUrl(), referralId);
            _driver.Navigate().GoToUrl(url);
            Initialize();
        }

        public RegisterPageStep2 Register(RegistrationDataForMemberWebsite data)
        {
            EnterRegistrationData(data);
            ClickRegisterButton();
            _driver.WaitForJavaScript();
            var page = new RegisterPageStep2(_driver);
            page.Initialize();
            return page;
        }

        public void SubmitRegistrationForm(RegistrationDataForMemberWebsite data)
        {
            EnterRegistrationData(data);
            ClickRegisterButton();
        }

        public PlayerProfilePage GoToPlayerProfile()
        {
            _settingsButton.Click();
            _playerProfileUrl.Click();
            var playerProfilePage = new PlayerProfilePage(_driver);
            playerProfilePage.Initialize();
            return playerProfilePage;
        }

        public void RegisterWithInvalidData(RegistrationDataForMemberWebsite data)
        {
            _username.SendKeys(data.Username);
            _firstName.SendKeys(data.FirstName);
            _lastName.SendKeys(data.LastName);
            _email.SendKeys(data.Email);
            _phoneNumber.SendKeys(data.PhoneNumber);
            _password.SendKeys(data.Password);
            _passwordConfirm.SendKeys(data.Password);

            var dayOfBirth = new SelectElement(_dayOfBirth);
            _driver.WaitForJavaScript();
            dayOfBirth.SelectByValue("0");
            var monthOfBirth = new SelectElement(_monthOfBirth);
            monthOfBirth.SelectByValue("0");
            var yearOfBirth = new SelectElement(_yearOfBirth);
            yearOfBirth.SelectByValue("0");
            _address.SendKeys(data.Address);
            _postalCode.SendKeys(data.PostalCode);
            var country = new SelectElement(_country);
            _driver.WaitForJavaScript();

            country.SelectByText("--Please Select--");

            var currency = new SelectElement(_currency);
            _driver.WaitForJavaScript();
            currency.SelectByText("--Please Select--");
            
            var title = new SelectElement(_title);
            title.SelectByText("--Please Select--");

            _city.SendKeys(data.City);


            var contactPreference = new SelectElement(_contactPreference);
            contactPreference.SelectByText("--Please Select--");

            ClickRegisterButton();
        }

        private void EnterRegistrationData(RegistrationDataForMemberWebsite data)
        {
            _driver.Manage().Window.Maximize();
            //_driver.ScrollToElement(_username);
            _username.SendKeys(data.Username);
            _password.SendKeys(data.Password);
            _passwordConfirm.SendKeys(data.Password);

            //_driver.ScrollToElement(_email);
            _email.SendKeys(data.Email);            
            _phoneNumber.SendKeys(data.PhoneNumber);
            var contactPreference = new SelectElement(_contactPreference);
            contactPreference.SelectByText(data.ContactPreference);
            
            //_driver.ScrollToElement(_title);
            var title = new SelectElement(_title);
            title.SelectByValue(data.Title);

            switch (data.Gender)
            {
                case "Male":
                    _driver.ScrollToElement(_radioMale);
                    _radioMale.Click();
                    break;
                case "Female":
                    _driver.ScrollToElement(_radioFemale);
                    _radioFemale.Click();
                    break;
                default:
                    throw  new ApplicationException("Unexpected Gender value");
            }
            Thread.Sleep(5000); //for Debuging in TeamCity

            _driver.ScrollToElement(_firstName);
            _firstName.SendKeys(data.FirstName);
            _lastName.SendKeys(data.LastName);

            _driver.ScrollToElement(_dayOfBirth);
            new SelectElement(_dayOfBirth).SelectByText(data.Day.ToString());
            new SelectElement(_monthOfBirth).SelectByText(data.Month.ToString());
            new SelectElement(_yearOfBirth).SelectByText(data.Year.ToString());

            var questions = new SelectElement(_securityQuestion);
            questions.SelectByValue(data.SecurityQuestion);
            _securityAnswer.SendKeys(data.SecurityAnswer);

            _driver.WaitForJavaScript();                

            _driver.ScrollToElement(_country);
            var country = new SelectElement(_country);
            country.SelectByValue(data.Country);
            _address.SendKeys(data.Address);

            _driver.ScrollToElement(_postalCode);
            _postalCode.SendKeys(data.PostalCode);
            _city.SendKeys(data.City);
            _stateProvince.SendKeys(data.Province);

            _driver.ScrollToElement(_currency);
            var currency = new SelectElement(_currency);
            currency.SelectByValue(data.Currency);

           _driver.WaitForJavaScript();
           // Thread.Sleep(10000);
            _driver.ScrollToElement(_over18);
           // Thread.Sleep(10000);
            _over18.Click();
            _acceptTerms.Click();
        }

        public void ClickRegisterButton()
        {
            if (IsRegisterButtonEnabled())
            {
                _registerButton.Click();
            }
            _driver.WaitForJavaScript();
        }

        public bool IsRegisterButtonEnabled()
        {
            return _registerButton.Enabled;
        }

        public IEnumerable<string> GetErrorMessages()
        {
            return ErrorMessages.FindElements(By.TagName("li")).AsEnumerable().Select(li => li.Text);
        }

        public IEnumerable<string> GetValidationMessages()
        {
            return _driver.FindElements(By.ClassName("msg")).AsEnumerable().Select(p => p.Text);
        }


        public string GetUserNameValidationMessage()
        {
            var xpath = string.Format("//div/label[contains(@data-i18n, 'registration.userNameMandatory')]/following-sibling::div/span");
            return _driver.FindElementValue(By.XPath(xpath));
        }


        public string GetEmailValidationMessage()
        {
            var xpath = string.Format("//div/label[contains(@data-i18n, 'common.emailAddressMandatory')]/following-sibling::div/span");
            return _driver.FindElementValue(By.XPath(xpath));
        }
        

        public PlayerOverviewPage GoToOverviewPage()
        {
            var overviewLink = _driver.FindElementWait(By.XPath("//a[@href='/en-US/Home/Index']")); // same as Overview page 
            overviewLink.Click();

            return new PlayerOverviewPage(_driver);
        }

        public void ClearFields()
        {
            _username.Clear();
            _password.Clear();
            _passwordConfirm.Clear();

            var title = new SelectElement(_title);
            title.SelectByText("--Please Select--");

            _firstName.Clear();
            _lastName.Clear();

            _radioMale.Clear();
            _radioFemale.Clear();

            _email.Clear();
            _phoneNumber.Clear();

            _driver.WaitForJavaScript();

            new SelectElement(_dayOfBirth).SelectByText("--Please Select--");
            new SelectElement(_monthOfBirth).SelectByText("--Please Select--");
            new SelectElement(_yearOfBirth).SelectByText("--Please Select--");

            var country = new SelectElement(_country);
            country.SelectByText("--Please Select--");

            var currency = new SelectElement(_currency);
            currency.SelectByText("--Please Select--");

            _address.Clear();
            _city.Clear();
            _postalCode.Clear();

            _driver.WaitForJavaScript();

            var contactPreference = new SelectElement(_contactPreference);
            contactPreference.SelectByText("--Please Select--");

            var questions = new SelectElement(_securityQuestion);
            questions.SelectByText("--Please Select--");

            _securityAnswer.Clear();

            _over18.Clear();
            _acceptTerms.Clear();
            _stateProvince.Clear();
        }

#pragma warning disable 649
        #region registration data
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

        [FindsBy(How = How.Id, Using = "title")]
        private IWebElement _title;

        [FindsBy(How = How.Id, Using = "mailingAddressCity")]
        private IWebElement _city;

        [FindsBy(How = How.Id, Using = "contactPreference")]
        private IWebElement _contactPreference;

        [FindsBy(How = How.Id, Using = "register-btn")]
        private IWebElement _registerButton;

        [FindsBy(How = How.Id, Using = "securityQuestionId")]
        private IWebElement _securityQuestion;

        [FindsBy(How = How.Id, Using = "securityAnswer")]
        private IWebElement _securityAnswer;

        [FindsBy(How = How.XPath, Using = "//a[@href='/en-US/Home/PlayerProfile']")]
        private IWebElement _playerProfileUrl;

        [FindsBy(How = How.XPath, Using = "//a[@href='#settings']")]
        private IWebElement _settingsButton;        

        [FindsBy(How = How.XPath, Using = "//input[@value='Male']")]
        private IWebElement _radioMale;

        [FindsBy(How = How.XPath, Using = "//input[@value='Female']")]
        private IWebElement _radioFemale;

        [FindsBy(How = How.Id, Using = "over18")]
        private IWebElement _over18;

        [FindsBy(How = How.Id, Using = "acceptTerms")]
        private IWebElement _acceptTerms;

        [FindsBy(How = How.Id, Using = "mailingAddressStateProvince")]
        private IWebElement _stateProvince;

        #endregion

        #region validation messages

        [FindsBy(How = How.XPath, Using = "//div[@id='register-messages']//ul")]
        public IWebElement ErrorMessages { get; private set; }

        //[FindsBy(How = How.XPath, Using = "//div/label[contains(@data-i18n, 'registration.userNameMandatory')]/following-sibling::div/span")]
        //public IWebElement userValidationErrorMessage { get; private set; }

        //[FindsBy(How = How.XPath, Using = "//div/label[contains(@data-i18n, 'common.emailAddressMandatory')]/following-sibling::div/span")]
        //public IWebElement emailValidationErrorMessage { get; private set; }


        #endregion
#pragma warning restore 649
    }

    public class RegistrationDataForMemberWebsite
    {
        public string Username;
        public string Password;
        public string Title;
        public string FirstName;
        public string LastName;
        public string Gender;
        public string Email;
        public string PhoneNumber;
        public int Day;
        public int Month;
        public int Year;
        public string Country;
        public string Currency;
        public string Address;
        public string AddressLine2;
        public string AddressLine3;
        public string AddressLine4;
        public string City;
        public string PostalCode;
        public string ContactPreference;
        public string SecurityQuestion;
        public string SecurityAnswer;
        public string Province;

        public string FullName { get { return FirstName + " " + LastName; } }
    }
}
