using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Selenium
{
    [Ignore("Failing unstable on RC-1.0 - Igor, 27-Aiprl-2016")]
    class LanguageManagerTests : SeleniumBaseForAdminWebsite
    {
        private LanguageManagerPage _languages;
        string validationMessageCode;
        string validationMessageName;
        string validationMessageNativeName;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _languages = dashboardPage.Menu.ClickLanguageManagerMenuItem();
        }

        [Test]
        public void Can_create_a_language()
        {
            var newLanguageForm = _languages.OpenNewLanguageForm();
            var code = TestDataGenerator.GetRandomAlphabeticString(3);
            var name = "Name" + code;
            var nativeName = "NativeName" + code;
            var submittedLanguageForm = newLanguageForm.Submit(code, name, nativeName);

            Assert.AreEqual("The language has been successfully created", submittedLanguageForm.ConfirmationMessage);
        }

        [Test]
        public void Cannot_create_language_without_required_data()
        {
            var newLanguageForm = _languages.OpenNewLanguageForm();            
            newLanguageForm.Submit();
            
            validationMessageCode = "This field is required.";
            validationMessageName = "This field is required.";
            validationMessageNativeName = "This field is required.";
            AssertValidationMessages(validationMessageCode, validationMessageName, validationMessageNativeName);
        }

        [Test]
        public void Cannot_create_language_with_not_valid_data()
        {
            var newLanguageForm = _languages.OpenNewLanguageForm();
            var code = "!@#$%^&*12";
            var name = "Name" + code;
            var nativeName = "NativeName" + code;
            newLanguageForm.FillFields(code, name, nativeName);
            newLanguageForm.Submit();       

            validationMessageCode = "Code can only contain alphabetic characters or dashes(-).";
            validationMessageName = "Name can only contain alphabetic characters, dashes(-), or spaces.";            
            AssertValidationMessages(validationMessageCode, validationMessageName);
        }

        [Test]
        public void Cannot_create_language_no_more_then_10_and_50_characters()
        {
            var newLanguageForm = _languages.OpenNewLanguageForm();
            var code = "12345678910";
            var name = "1234567891012345678910123456789101234567891012345678910";
            var nativeName = "1234567891012345678910123456789101234567891012345678910";
            newLanguageForm.FillFields(code, name, nativeName);
            newLanguageForm.Submit();

            validationMessageCode = "Please enter no more than 10 characters.";
            validationMessageName = "Please enter no more than 50 characters.";
            validationMessageNativeName = "Please enter no more than 50 characters.";
            AssertValidationMessages(validationMessageCode, validationMessageName, validationMessageNativeName);            
        }

        [Test]
        public void Cannot_create_duplicate_language()
        {
            var newLanguageForm = _languages.OpenNewLanguageForm();
            var code = TestDataGenerator.GetRandomAlphabeticString(3);
            var name = "Name" + code;
            var nativeName = "NativeName" + code;
            var submittedLanguageFormFirst = newLanguageForm.Submit(code, name, nativeName);
            Assert.AreEqual("The language has been successfully created", submittedLanguageFormFirst.ConfirmationMessage);
            submittedLanguageFormFirst.CloseTab("View Language");

            _languages.OpenNewLanguageForm();
            newLanguageForm.FillFields(code, name, nativeName);
            newLanguageForm.Submit();
            var confirmationMessage = _driver.FindElementValue(By.XPath("//div[contains(@class,'alert alert-')]"));
            Assert.AreEqual("This code has been used.", confirmationMessage);            
        }

        [Test]
        public void Can_edit_a_language()
        {
            var newLanguage = CreateLanguage();
            newLanguage.CloseTab("View Language");
                        
            _driver.SelectRecordInGridExtendFilter("Name", "is", newLanguage.Name);
            var editForm = _languages.OpenEditLanguageForm();
            var newSuffix = TestDataGenerator.GetRandomAlphabeticString(3);
            var newName = "Name" + newSuffix;
            var newNativeName = "NativeName" + newSuffix;
            var submittedEditForm = editForm.Submit(newName, newNativeName);

            Assert.AreEqual("The language has been successfully updated", submittedEditForm.ConfirmationMessage);
        }

        [Test]
        public void Can_view_a_language()
        {
            var newLanguage = CreateLanguage();
            newLanguage.CloseTab("View Language");
            _driver.SelectRecordInGridExtendFilter("Name", "is", newLanguage.Name);
            var viewPage = _languages.OpenViewLanguagePage();

            Assert.AreEqual("View Language", viewPage.TabName);
        }

        [Test]
        [Ignore("Until VladS fixes - AFREGO-4285, 22-Feb-2016, Igor")]
        public void Can_deactivate_language()
        {
            var newLanguage = CreateLanguage();
            newLanguage.CloseTab("View Language");

            _driver.SelectRecordInGridExtendFilter("Name", "is", newLanguage.Name);
            var dialog = _languages.ShowDeactivateDialog();
            dialog.Deactivate();

            Assert.AreEqual("The language has been successfully deactivated.", dialog.ResponseMessage);
        }

        [Test]
        [Ignore("Until VladS fixes - AFREGO-4285, 22-Feb-2016, Igor")]
        public void Can_activate_language()
        {
            var newLanguage = CreateLanguage();
            newLanguage.CloseTab("View Language");

            _driver.SelectRecordInGridExtendFilter("Name", "is", newLanguage.Name);
            var deactivateDialog = _languages.ShowDeactivateDialog();
            deactivateDialog.Deactivate();
            deactivateDialog.CloseDialog();

            _driver.SelectRecordInGridExtendFilter("Name", "is", newLanguage.Name);
            var acctivateDialog = _languages.ShowActivateDialog();
            acctivateDialog.Activate();
            acctivateDialog.CloseDialog();

            Assert.AreEqual("The language has been successfully activated.", acctivateDialog.ResponseMessage);
        }

        public SubmittedLanguageForm CreateLanguage(string code = null, string name = null, string nativeName = null)
        {
            var newLanguageForm = _languages.OpenNewLanguageForm();
            var newCode = code ?? TestDataGenerator.GetRandomAlphabeticString(3);
            var newName = name ?? "Name" + newCode;
            var newNativeName = nativeName ?? "NativeName" + newCode;
            return newLanguageForm.Submit(newCode, newName, newNativeName);
        }

        public void AssertValidationMessages(string code, string name, string nativeName)
        {
            var validationMessageCode = _driver.FindElementWait(By.XPath("//span[@data-bind ='validationMessage: fields.code']"));
            Assert.AreEqual(code, validationMessageCode.Text);
            var validationMessageName = _driver.FindElementWait(By.XPath("//span[@data-bind ='validationMessage: fields.name']"));
            Assert.AreEqual(name, validationMessageName.Text);
            var validationMessageNativeName = _driver.FindElementWait(By.XPath("//span[@data-bind ='validationMessage: fields.nativeName']"));
            Assert.AreEqual(nativeName, validationMessageName.Text);
        }

        public void AssertValidationMessages(string code, string name)
        {
            var validationMessageCode = _driver.FindElementWait(By.XPath("//span[@data-bind ='validationMessage: fields.code']"));
            Assert.AreEqual(code, validationMessageCode.Text);
            var validationMessageName = _driver.FindElementWait(By.XPath("//span[@data-bind ='validationMessage: fields.name']"));
            Assert.AreEqual(name, validationMessageName.Text);            
        }
    }
}
