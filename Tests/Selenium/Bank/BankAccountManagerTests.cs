using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    [Ignore("Failing unstable on RC-1.0 - Igor, 27-Aiprl-2016")]
    class BankAccountManagerTests : SeleniumBaseForAdminWebsite
    {
        private BankAccountManagerPage _bankAccountManagerPage;

        [Test, Ignore("AFTREGO-4518 Rostyslav 04/06/2016")]
        public void Can_create_and_view_bank_account()
        {
            var bankAccountId = "bankaccpontID_" + TestDataGenerator.GetRandomString(5);
            var bankAccountNumber = TestDataGenerator.GetRandomBankAccountNumber(8);
            var bankAccountName = "BankAccountName-" + TestDataGenerator.GetRandomString(5);
            var province = "province-" + TestDataGenerator.GetRandomString(5);
            var branch = "branch-" + TestDataGenerator.GetRandomString(5);

            var supplierName = "SupplierName" + TestDataGenerator.GetRandomAlphabeticString(8);
            var contactNumber = TestDataGenerator.GetRandomNumber(12222222, 10000000) + "";
            var usbCode = "USBcode" + TestDataGenerator.GetRandomString(4);

            _driver.Logout();
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _bankAccountManagerPage = dashboardPage.Menu.ClickBankAccountsItem();
            var editedData = TestDataGenerator.EditBankAccountData();

            // create a bank account
            var newBankAccountForm = _bankAccountManagerPage.OpenNewBankAccountForm();
            var submittedNewBankAccountForm = newBankAccountForm.SubmitWithLicensee("Flycow",
                "138", "CAD", bankAccountId, bankAccountName, bankAccountNumber, province, branch, "Affiliate", supplierName, contactNumber, usbCode);

            Assert.AreEqual("The bank account has been successfully created", submittedNewBankAccountForm.ConfirmationMessage);
            Assert.AreEqual(bankAccountId, submittedNewBankAccountForm.BankAccountIdValue);
            Assert.AreEqual(bankAccountNumber, submittedNewBankAccountForm.BankAccountNumberValue);
            Assert.AreEqual(bankAccountName, submittedNewBankAccountForm.BankAccountNameValue);
            Assert.AreEqual(province, submittedNewBankAccountForm.ProvinceValue);
            Assert.AreEqual(branch, submittedNewBankAccountForm.BranchValue);
            Assert.AreEqual("Affiliate", submittedNewBankAccountForm.BankAccountTypeValue);
            Assert.AreEqual(supplierName, submittedNewBankAccountForm.BankAccountSupplierName);
            Assert.AreEqual(contactNumber, submittedNewBankAccountForm.BankAccountContactNumber);
            Assert.AreEqual(usbCode, submittedNewBankAccountForm.BankAccountUsbCobeValue);
            submittedNewBankAccountForm.CloseTab("View Bank Account");

            // view a bank account
            var _viewBankAccountForm = _bankAccountManagerPage.OpenViewBankAccountForm(bankAccountName);
            Assert.AreEqual("Flycow", _viewBankAccountForm.LicenseeValue);
            Assert.AreEqual("138", _viewBankAccountForm.BrandNameValue);
            Assert.AreEqual("CAD", _viewBankAccountForm.CurrencyValue);
            Assert.AreEqual(bankAccountId, _viewBankAccountForm.BankAccountIdValue);
            Assert.AreEqual(bankAccountNumber, _viewBankAccountForm.BankAccountNumberValue);
            Assert.AreEqual(bankAccountName, _viewBankAccountForm.BankAccountNameValue);
            Assert.AreEqual(province, _viewBankAccountForm.ProvinceValue);
            Assert.AreEqual(branch, _viewBankAccountForm.BranchValue);
            Assert.AreEqual("Affiliate", _viewBankAccountForm.BankAccountTypeValue);
            Assert.AreEqual(supplierName, _viewBankAccountForm.BankAccountSupplierName);
            Assert.AreEqual(contactNumber, _viewBankAccountForm.BankAccountContactNumber);
            Assert.AreEqual(usbCode, _viewBankAccountForm.BankAccountUsbCobeValue);
            Assert.AreEqual("2015/10/24", _viewBankAccountForm.PurchasedDate);
            Assert.AreEqual("2016/11/24", _viewBankAccountForm.UtilizationDate);
            Assert.AreEqual("2016/12/24", _viewBankAccountForm.ExpirationDate);
            Assert.AreEqual("new created Bank Account by Selenium Test", _viewBankAccountForm.RemarksValue);

            _driver.Logout();
        }

        [Test, Ignore("AFTREGO-4518 Rostyslav 04/06/2016")]
        public void Can_edit_pending_bank_account()
        {
            var bankAccountId = "bankaccpontID_" + TestDataGenerator.GetRandomString(5);
            var bankAccountNumber = TestDataGenerator.GetRandomBankAccountNumber(8);
            var bankAccountName = "BankAccountName-" + TestDataGenerator.GetRandomString(5);
            var province = "province-" + TestDataGenerator.GetRandomString(5);
            var branch = "branch-" + TestDataGenerator.GetRandomString(5);

            var supplierName = "SupplierName" + TestDataGenerator.GetRandomAlphabeticString(8);
            var contactNumber = TestDataGenerator.GetRandomNumber(12222222, 10000000) + "";
            var usbCode = "USBcode" + TestDataGenerator.GetRandomString(4);

            _driver.Logout();
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _bankAccountManagerPage = dashboardPage.Menu.ClickBankAccountsItem();
            var editedData = TestDataGenerator.EditBankAccountData();

            // create a bank account
            var newBankAccountForm = _bankAccountManagerPage.OpenNewBankAccountForm();
            var submittedNewBankAccountForm = newBankAccountForm.SubmitWithLicensee("Flycow",
                "138", "CAD", bankAccountId, bankAccountName, bankAccountNumber, province, branch, "Affiliate", supplierName, contactNumber, usbCode);
            Assert.AreEqual("The bank account has been successfully created", submittedNewBankAccountForm.ConfirmationMessage);
            Assert.AreEqual(bankAccountId, submittedNewBankAccountForm.BankAccountIdValue);
            Assert.AreEqual(bankAccountNumber, submittedNewBankAccountForm.BankAccountNumberValue);
            Assert.AreEqual(bankAccountName, submittedNewBankAccountForm.BankAccountNameValue);
            Assert.AreEqual(province, submittedNewBankAccountForm.ProvinceValue);
            Assert.AreEqual(branch, submittedNewBankAccountForm.BranchValue);
            Assert.AreEqual("Affiliate", submittedNewBankAccountForm.BankAccountTypeValue);
            Assert.AreEqual(supplierName, submittedNewBankAccountForm.BankAccountSupplierName);
            Assert.AreEqual(contactNumber, submittedNewBankAccountForm.BankAccountContactNumber);
            Assert.AreEqual(usbCode, submittedNewBankAccountForm.BankAccountUsbCobeValue);
            submittedNewBankAccountForm.CloseTab("View Bank Account");
           
            // edit bank account details
            var editForm = _bankAccountManagerPage.OpenEditForm(bankAccountName);
            var submittedForm = editForm.SubmitForPendingAccount(data: editedData, currency: "RMB", bank: "Bank of Canada");
            Assert.AreEqual("The bank account has been successfully updated", submittedForm.ConfirmationMessage);
            Assert.AreEqual(editedData.Type, submittedForm.BankAccountTypeValue);
            Assert.AreEqual(editedData.SupplierName, submittedForm.BankAccountSupplierName);
            Assert.AreEqual(editedData.ContactNumber.ToString(), submittedForm.BankAccountContactNumber);
            Assert.AreEqual(editedData.UsbCode, submittedForm.BankAccountUsbCobeValue);
            Assert.AreEqual("2016/11/25", submittedForm.UtilizationDate);
            Assert.AreEqual("2016/12/25", submittedForm.ExpirationDate);

            _driver.Logout();
        }

        [Test, Ignore("AFTREGO-4518 Rostyslav 04/06/2016")]
        public void Can_edit_activated_bank_account()
        {
            var bankAccountId = "bankaccpontID_" + TestDataGenerator.GetRandomString(5);
            var bankAccountNumber = TestDataGenerator.GetRandomBankAccountNumber(8);
            var bankAccountName = "BankAccountName-" + TestDataGenerator.GetRandomString(5);
            var province = "province-" + TestDataGenerator.GetRandomString(5);
            var branch = "branch-" + TestDataGenerator.GetRandomString(5);

            var supplierName = "SupplierName" + TestDataGenerator.GetRandomAlphabeticString(8);
            var contactNumber = TestDataGenerator.GetRandomNumber(12222222, 10000000) + "";
            var usbCode = "USBcode" + TestDataGenerator.GetRandomString(4);

            _driver.Logout();
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _bankAccountManagerPage = dashboardPage.Menu.ClickBankAccountsItem();
            var editedData = TestDataGenerator.EditBankAccountData();

            // create a bank account
            var newBankAccountForm = _bankAccountManagerPage.OpenNewBankAccountForm();
            var submittedNewBankAccountForm = newBankAccountForm.SubmitWithLicensee("Flycow",
                "138", "CAD", bankAccountId, bankAccountName, bankAccountNumber, province, branch, "Affiliate", supplierName, contactNumber, usbCode);
            Assert.AreEqual("The bank account has been successfully created", submittedNewBankAccountForm.ConfirmationMessage);
            Assert.AreEqual(bankAccountId, submittedNewBankAccountForm.BankAccountIdValue);
            Assert.AreEqual(bankAccountNumber, submittedNewBankAccountForm.BankAccountNumberValue);
            Assert.AreEqual(bankAccountName, submittedNewBankAccountForm.BankAccountNameValue);
            Assert.AreEqual(province, submittedNewBankAccountForm.ProvinceValue);
            Assert.AreEqual(branch, submittedNewBankAccountForm.BranchValue);
            Assert.AreEqual("Affiliate", submittedNewBankAccountForm.BankAccountTypeValue);
            Assert.AreEqual(supplierName, submittedNewBankAccountForm.BankAccountSupplierName);
            Assert.AreEqual(contactNumber, submittedNewBankAccountForm.BankAccountContactNumber);
            Assert.AreEqual(usbCode, submittedNewBankAccountForm.BankAccountUsbCobeValue);
            submittedNewBankAccountForm.CloseTab("View Bank Account");

            //activate bank account
            var _activateBankAccountDialog = _bankAccountManagerPage.OpenActivateBankAccountDialog(bankAccountName);
            var _submittedActivateBankAccountDialog = _activateBankAccountDialog.ActivateBankAccount("Bank account activated");
            Assert.AreEqual("The Bank Account has been successfully activated", _submittedActivateBankAccountDialog.ConfirmationMessage);
            _submittedActivateBankAccountDialog.Close();

            // edit bank account details
            var editForm = _bankAccountManagerPage.OpenEditForm(bankAccountName);
            var submittedForm = editForm.SubmitForActivatedAccount(data: editedData);
            Assert.AreEqual("The bank account has been successfully updated", submittedForm.ConfirmationMessage);
            Assert.AreEqual(editedData.Type, submittedForm.BankAccountTypeValue);
            Assert.AreEqual(editedData.SupplierName, submittedForm.BankAccountSupplierName);
            Assert.AreEqual(editedData.ContactNumber.ToString(), submittedForm.BankAccountContactNumber);
            Assert.AreEqual(editedData.UsbCode, submittedForm.BankAccountUsbCobeValue);
            Assert.AreEqual("2016/11/25", submittedForm.UtilizationDate);
            Assert.AreEqual("2016/12/25", submittedForm.ExpirationDate);

            _driver.Logout();
        }

        [Test]
        public void Can_activate_and_deactivate_bank_account()
        {
            var bankAccountId = "bankaccpontID_" + TestDataGenerator.GetRandomString(5);
            var bankAccountNumber = TestDataGenerator.GetRandomBankAccountNumber(8);
            var bankAccountName = "BankAccountName-" + TestDataGenerator.GetRandomString(5);
            var province = "province-" + TestDataGenerator.GetRandomString(5);
            var branch = "branch-" + TestDataGenerator.GetRandomString(5);

            var supplierName = "SupplierName" + TestDataGenerator.GetRandomAlphabeticString(8);
            var contactNumber = TestDataGenerator.GetRandomNumber(12222222, 10000000) + "";
            var usbCode = "USBcode" + TestDataGenerator.GetRandomString(4);

            _driver.Logout();
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _bankAccountManagerPage = dashboardPage.Menu.ClickBankAccountsItem();
            var editedData = TestDataGenerator.EditBankAccountData();

            // create a bank account
            var newBankAccountForm = _bankAccountManagerPage.OpenNewBankAccountForm();
            var submittedNewBankAccountForm = newBankAccountForm.SubmitWithLicensee("Flycow",
                "138", "CAD", bankAccountId, bankAccountName, bankAccountNumber, province, branch, "Affiliate", supplierName, contactNumber, usbCode);
            Assert.AreEqual("The bank account has been successfully created", submittedNewBankAccountForm.ConfirmationMessage);
            Assert.AreEqual(bankAccountId, submittedNewBankAccountForm.BankAccountIdValue);
            Assert.AreEqual(bankAccountNumber, submittedNewBankAccountForm.BankAccountNumberValue);
            Assert.AreEqual(bankAccountName, submittedNewBankAccountForm.BankAccountNameValue);
            Assert.AreEqual(province, submittedNewBankAccountForm.ProvinceValue);
            Assert.AreEqual(branch, submittedNewBankAccountForm.BranchValue);
            Assert.AreEqual("Affiliate", submittedNewBankAccountForm.BankAccountTypeValue);
            Assert.AreEqual(supplierName, submittedNewBankAccountForm.BankAccountSupplierName);
            Assert.AreEqual(contactNumber, submittedNewBankAccountForm.BankAccountContactNumber);
            Assert.AreEqual(usbCode, submittedNewBankAccountForm.BankAccountUsbCobeValue);
            submittedNewBankAccountForm.CloseTab("View Bank Account");

            //activate bank account
            var _activateBankAccountDialog = _bankAccountManagerPage.OpenActivateBankAccountDialog(bankAccountName);
            var _submittedActivateBankAccountDialog = _activateBankAccountDialog.ActivateBankAccount("Bank account activated");
            Assert.AreEqual("The Bank Account has been successfully activated", _submittedActivateBankAccountDialog.ConfirmationMessage);
            _submittedActivateBankAccountDialog.Close();
            Assert.AreEqual("Active", _bankAccountManagerPage.Status);

            //deactivate bank account
            var _deactivateBankAccountDialog = _bankAccountManagerPage.OpenDeactivateBankAccountDialog(bankAccountName);
            var _submittedDeactivateBankAccountDialog = _deactivateBankAccountDialog.DeactivateBankAccount("Bank account deactivated");
            Assert.AreEqual("The Bank Account has been successfully deactivated", _submittedDeactivateBankAccountDialog.ConfirmationMessage);
            _submittedDeactivateBankAccountDialog.Close();
            Assert.AreEqual("Pending", _bankAccountManagerPage.Status);

            _driver.Logout();
        }
   }
}
