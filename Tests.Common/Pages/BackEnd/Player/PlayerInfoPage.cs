using System;
using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PlayerInfoPage : BackendPageBase
    {
        public PlayerInfoPage(IWebDriver driver) : base(driver) { }

        public string PageXPath =
            "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]/div/div[@data-view='player-manager/info']";

        public static By EditButton = By.XPath("//button[text()='Edit']");
        public static By DeactivateButton = By.XPath("//span[@data-bind='text: activateButtonText']");
        public static By ActivateButton = By.XPath("//button[text()='Activate']");
        public static By SaveButton = By.XPath("//button[text()='Save']");

        public string VipLevel
        {
            get
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
                var vipLevel = _driver.FindElementWait(By.XPath("//p[contains(@data-bind, 'text: vipLevelName')]"));
                wait.Until(x => vipLevel.Displayed);
                return vipLevel.Text;
            }
        }

        public string Username
        {
            get { return _driver.FindElementValue(By.XPath("//p[@data-bind='text: username']")); }
        }

        public string FirstName
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath("//p[contains(@data-bind, 'text: firstName')]"));
            }
        }

        public string LastName
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath("//p[contains(@data-bind, 'text: lastName')]"));
            }
        }

        public void OpenTransactionsSection()
        {
            var transactionsSection = _driver.FindElementWait(By.XPath("//a[contains(@id, 'transactions')]"));
            transactionsSection.Click();
        }

        public void OpenAccountInformationSection()
        {
            var transactionsSection = _driver.FindElementWait(By.XPath("//a[contains(@id, 'account-information')]"));
            transactionsSection.Click();
        }

        public void OpenBalanceInformationSection()
        {
            //Thread.Sleep(5000);
            var balanceSection = _driver.FindElementWait(By.XPath("//a[contains(@id, 'balance-information')]"));
            balanceSection.Click();
        }

        public void OpenWithdrawHistorySection()
        {
            var withdrawHistorySection = _driver.FindElementWait(By.XPath("//a[contains(@id, 'withdraw-history')]"));
            withdrawHistorySection.Click();
        }

        public void OpenAccountRestrictionsSection()
        {
            var accountRestrictionsSection = _driver.FindElementWait(By.XPath("//a[contains(@id, 'account-restrictions')]"));
            accountRestrictionsSection.Click();
        }

        public void OpenBankAccountsSection()
        {
            var bankAccountsSection = _driver.FindElementWait(By.XPath("//a[contains(@id, 'bank-accounts')]"));
            bankAccountsSection.Click();
        }



        public void OpenBonusSection()
        {
            var bonusSection = _driver.FindElementWait(By.XPath("//a[contains(@id, 'bonus')]"));
            bonusSection.Click();
        }

        public void FindAndSelectBonusQualificationRecord(string name, string type)
        {
            var xpath = string.Format("//table//tr[contains(., '{0}') and contains(., '{1}')]",
                name, type);
            var recordInGrid = _driver.FindElementWait(By.XPath(xpath));
            recordInGrid.Click();
        }

        public void FindAndSelectRecord(string name, string type = "")
        {
            var xpath = string.Format("//table//tr[contains(., '{0}') and contains(., '{1}')]",
                name, type);
            var recordInGrid = _driver.FindElementWait(By.XPath(xpath));
            recordInGrid.Click();
        }

        public void OpenBonusToIssueSection()
        {
            var buttonBonusToIssue =
                _driver.FindElementWait(
                        By.XPath("//button[contains(@data-bind, 'enable: bonusToIssue')]"));
                buttonBonusToIssue.Click();
        }

        public string GetBonusQualificationName()
        {
            var xpath = string.Format("//label[@data-bind='text: bonusToIssue().name']");
            return _driver.FindElementValue(By.XPath(xpath));
        }

        public string GetBonusQualificationAmount()
        {
            var xpath = string.Format("//p[contains(@data-bind, 'currentTransaction().bonusAmount')]");
            return _driver.FindElementValue(By.XPath(xpath));
        }

        public void IssuedBonusForQualifiedTransaction()
        {
            var buttonIssueBonus =
                _driver.FindElementWait(
                        By.XPath("//button[contains(@data-bind, 'click: issueBonus')]"));
            buttonIssueBonus.Click();
        }

        public string GetConformationMessage()
        {
            var xpath = string.Format("//h4[contains(@data-bind, 'visible: bonusIssued')]");
            return _driver.FindElementValue(By.XPath(xpath));
        }

        public string GetBonusQualificationMessage()
        {
            var xpath = string.Format("//h3[contains(@data-bind, 'visible: transactions().length')]");
            return _driver.FindElementValue(By.XPath(xpath));
        }



        public decimal GetTransactionMainAmount(string type)
        {
            var xpath = string.Format("//td[text()='{0}']/following-sibling::td[contains(@aria-describedby, 'Amount')]", type);
            var stringValue = _driver.FindElementValue(By.XPath(xpath));
            return decimal.Parse(stringValue);
        }

        public decimal GetTransactionBonusAmount(string type)
        {
            var xpath = string.Format("//td[text()='{0}']/following-sibling::td[contains(@aria-describedby, 'Amount')]", type);
            var stringValue = _driver.FindElementValue(By.XPath(xpath));
            return decimal.Parse(stringValue);
        }

        public decimal[] GetTransactionsMainAmount(string type)
        {
            var xpath = string.Format("//td[text()='{0}']/following-sibling::td[contains(@aria-describedby, 'Amount')]", type);
            var elements = _driver.FindAnyElementsWait(By.XPath(xpath), element => element.Text != string.Empty);
            return elements.Select(webElement => decimal.Parse(webElement.Text)).ToArray();
        }

        public string GetMainBalance()
        {
            var xpath = string.Format("//p[@data-bind='text: balance().mainBalance']");
            return _driver.FindElementValue(By.XPath(xpath));
        }

        public string GetMainBonusBalance()
        {
            var xpath = string.Format("//p[@data-bind='text: balance().bonusBalance']");
            return _driver.FindElementValue(By.XPath(xpath));
        }

        public string GetTotalBonusBalance()
        {
            var xpath = string.Format("//p[@data-bind='text: balance().totalBonus']");
            return _driver.FindElementValue(By.XPath(xpath));
        }

        public string GetWithdrawTransactionAmount(string amount)
        {
            var xpath = string.Format("//table[contains(@id, 'withdraw-list')]//td[contains(@title, '{0}')]", amount);
            return _driver.FindElementValue(By.XPath(xpath));
        }

        public string GetWithdrawTransactionStatus(string status)
        {
            var xpath = string.Format("//table[contains(@id, 'withdraw-list')]//td[contains(@title, '{0}')]", status);
            return _driver.FindElementValue(By.XPath(xpath));
        }

        public void DeactivatePlayer()
        {
            var deactivateButton = _driver.FindElementWait(DeactivateButton);
            deactivateButton.Click();
            _driver.FindElementWait(By.XPath("//button/span[text()='Activate']"));
        }

        public void ActivatePlayer()
        {                                                                     //button/span[text()='Activate'
            var activateButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: setStatus')]"));
            activateButton.Click();                  
            //_driver.FindElementWait(By.XPath("//button/span[text()='Deactivate']"));
        }

        public AccountInformation GetAccountDetails()
        {
            return new AccountInformation
            {
                Username = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'username')]")),
                FirstName = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'firstName')]")),
                LastName = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'lastName')]")),
                Title = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'title')]")),
                Gender = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'gender')]")),
                EmailAddress = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'email')]")),
                MobileNumber = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'phoneNumber')]")),
                AddressLine1 = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'mailingAddressLine1')]")),
                City = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'mailingAddressCity')]")),
                PostalCode = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'mailingAddressPostalCode')]")),
                Country = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'countryName')]")),
                PaymentLevel = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'paymentLevelName')]")),
                PrimaryContact = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'contactPreference')]")),
                AccountAlerts = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'accountAlertsText')]")),
                MarketingAlerts = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: marketingAlertsText')]")),
                VIPLevel = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: vipLevelName')]"))
            };

        }

        public void OpenAccountDetailsInEditMode()
        {
            OpenAccountInformationSection();
            var editButtonButton = _driver.FindElementWait(EditButton);
            editButtonButton.Click();
        }

        public ChangeVipLevelDialog OpenChangeVipLevelDialog()
        {
            var changeVipLevelDialogButton =
                _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: showChangeVipLevelDialog')]"));
            changeVipLevelDialogButton.Click();

            return new ChangeVipLevelDialog(_driver);
        }

        public ChangePaymentLevelDialog OpenChangePaymentLevelDialog()
        {
            var changePaymentLevelDialogButton =
                _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: showChangePaymentLevelDialog')]"));
            changePaymentLevelDialogButton.Click();

            return new ChangePaymentLevelDialog(_driver);
        }

        public string ChangeFreezeStatusOfPlayer()
        {
            var freezeButton = _driver.FindAnyElementWait(By.XPath("//button[contains(@data-bind,'changeFreezeStatus')]"));
            freezeButton.Click();
            var freezeStatus = _driver.FindElementWait(By.XPath("//p[contains(@data-bind,'frozen')]"));
            return freezeStatus.Text;
        }

        public void AssignVipLevel(string vipLevel)
        {
            var xpath = _driver.FindElementWait(By.XPath("//select[contains(@id, 'vip-level')]"));
            var vipLevelList = new SelectElement(xpath);
            vipLevelList.SelectByText(vipLevel);
        }

        public void SaveAccountInfo()
        {
            var saveButton = _driver.FindElementWait(SaveButton);
            saveButton.Click();
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            var editButton = _driver.FindElementWait(EditButton);
            wait.Until(x => editButton.Enabled);
        }

        public void Edit(PlayerRegistrationDataForAdminWebsite editedPlayerData)
        {
            var firstName =
                _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: firstName')]"));
            firstName.Clear();
            firstName.SendKeys(editedPlayerData.FirstName);

            var lastName =
                _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: lastName')]"));
            lastName.Clear();
            lastName.SendKeys(editedPlayerData.LastName);

            var email = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: email')]"));
            email.Clear();
            email.SendKeys(editedPlayerData.Email);

            var mobileNumber = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: phoneNumber')]"));
            mobileNumber.Clear();
            mobileNumber.SendKeys(editedPlayerData.MobileNumber);

            var country = _driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'options: countries')]"));
            country.SendKeys(editedPlayerData.Country);

            SaveAccountInfo();
        }

        public SendNewPlayerPasswordDialog OpenSendNewPasswordDialog()
        {
            var sendNewPasswordButton = _driver.FindElementWait(By.XPath("//button[@data-bind='click: showSendMessageDialog']"));
            sendNewPasswordButton.Click();
            var dialog = new SendNewPlayerPasswordDialog(_driver);
            return dialog;
        }

        public PlayerBankAccountForm OpenNewBankAccountTab()
        {
            var newPlayerBankAccountButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: openAddBankAccountForm')]"));
            newPlayerBankAccountButton.Click();
            return new PlayerBankAccountForm(_driver);
        }

        public SubmittedPlayerBankAccountForm OpenViewBankAccountTab()
        {
            var viewPlayerBankAccountButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'openViewBankAccountForm')]"));
            viewPlayerBankAccountButton.Click();
            return new SubmittedPlayerBankAccountForm(_driver);
        }

        public bool IsEditButtonPresent()
        {
            return IsElementPresent(EditButton);
        }

        public ResponsibleGamblingSection OpenResponsibleGamblingSection()
        {
            _driver.FindElementScroll(By.XPath("//a[@id='responsible-gambling']")).Click();
            return new ResponsibleGamblingSection(_driver);
        }
    }

    public class ResponsibleGamblingSection : PlayerInfoPage
    {
        public By checkBoxSelfExcludeBy = By.XPath("//input[contains(@data-bind,'isSelfExclusionEnabled')]");
        public By checkBoxTimeOutBy = By.XPath("//input[contains(@data-bind,'isTimeOutEnabled')]");

        public ResponsibleGamblingSection(IWebDriver driver) : base(driver) { }

        //Self Exclude
        public string SetSelfExcludeData(string data)
        {
            var checkBoxSelfExclude = _driver.FindElementScroll(checkBoxSelfExcludeBy);
            checkBoxSelfExclude.Click();
            var selectSelfExclude = _driver.FindElementWait(By.XPath("//select[contains(@data-bind,'selfExclusions')]"));
            new SelectElement(selectSelfExclude).SelectByText(data);
            var update = _driver.FindElementWait(By.XPath("//button[contains(@data-bind,'update')]"));
            update.Click();
            var yesButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind,'yesClick')]"));
            yesButton.Click();
            var selfExclusionEndDate = _driver.FindElementWait(By.XPath("//span[contains(@data-bind,'selfExclusionEndDate')]")).Text;
            return selfExclusionEndDate;
        }

        public bool IsCheckBoxSelfExcludeSelected()
        {
            var checkBoxSelfExclude = _driver.FindElementScroll(checkBoxSelfExcludeBy);
            return checkBoxSelfExclude.Selected;

        }

        public void UnSelectedCheckBoxSelfExclude()
        {
            var checkBoxSelfExclude = _driver.FindElementScroll(checkBoxSelfExcludeBy);
            checkBoxSelfExclude.Click();
            var update = _driver.FindElementWait(By.XPath("//button[contains(@data-bind,'update')]"));
            update.Click();
            var yesButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind,'yesClick')]"));
            yesButton.Click();
        }

        //Time Out
        public string SetTimeOutData(string data)
        {
            var checkBoxTimeOut = _driver.FindElementScroll(checkBoxTimeOutBy);
            checkBoxTimeOut.Click();
            var selectTimeOut = _driver.FindElementWait(By.XPath("//select[contains(@data-bind,'isTimeOutEnabled')]"));
            new SelectElement(selectTimeOut).SelectByText(data);
            var update = _driver.FindElementWait(By.XPath("//button[contains(@data-bind,'update')]"));
            update.Click();
            var yesButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind,'yesClick')]"));
            yesButton.Click();
            var selfTimeOutEndDate = _driver.FindElementWait(By.XPath("//span[contains(@data-bind,'timeOutEndDate')]")).Text;
            return selfTimeOutEndDate;
        }        

        public bool IsCheckBoxTimeOutSelected()
        {
            var checkBoxTimeOut = _driver.FindElementScroll(checkBoxTimeOutBy);
            return checkBoxTimeOut.Selected;

        }

        public void UnSelectedCheckBoxTimeOut()
        {
            var checkBoxTimeOut = _driver.FindElementScroll(checkBoxTimeOutBy);
            checkBoxTimeOut.Click();
            var update = _driver.FindElementWait(By.XPath("//button[contains(@data-bind,'update')]"));
            update.Click();
            var yesButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind,'yesClick')]"));
            yesButton.Click();
        }

    }

    public class SendNewPlayerPasswordDialog : BackendPageBase
    {
        public SendNewPlayerPasswordDialog(IWebDriver driver) : base(driver) { }

        public string ConfirmationMessage
        {
            get
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(3));
                var confirmationMsg =
                    _driver.FindElementWait(By.XPath("//div[text()='New password has been successfully sent']"));
                wait.Until(x => confirmationMsg.Displayed);
                return confirmationMsg.Text;
            }
        }

        public void SpecifyNewPassword(string newPassword)
        {
            var unselectGenerateNewPasswordCheckbox =
                _driver.FindLastElementWait(By.XPath("//input[contains(@data-bind, 'checked: generateNewPassword')]"));
            unselectGenerateNewPasswordCheckbox.Click();
            var newPasswordField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: newPassword')]"));
            newPasswordField.SendKeys(newPassword);
        }

        public void Send()
        {
            var sendButton = _driver.FindElementWait(By.XPath("//button[text()='Send']"));
            sendButton.Click();
        }
    }

    public class AccountInformation
    {
        public string Username;
        public string FirstName;
        public string LastName;
        public string Title;
        public string Gender;
        public string EmailAddress;
        public string MobileNumber;
        public string AddressLine1;
        public string City;
        public string PostalCode;
        public string Country;
        public string PaymentLevel;
        public string PrimaryContact;
        public string AccountAlerts;
        public string MarketingAlerts;
        public string VIPLevel;        
    }
}