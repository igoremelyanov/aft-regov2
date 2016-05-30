using System;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    [Ignore("Unsatble tests  27-April-2016, Igor.")]
    class BrandManagerTests : SeleniumBaseForAdminWebsite
    {
        private BrandManagerPage _brandManagerPage;
        private DashboardPage _dashboardPage;
        
        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();
        }

        [Test]  //Only create Brand, assigne Country and create Bank for now 25-Jan-2016
        public void Can_add_and_activate_brand()
        {            
            // create a brand
            const string licensee = "Flycow";
            var randomString = TestDataGenerator.GetRandomString(4);
            var brandName = "brand-" + randomString;
            var brandCode = randomString;
            var playerPrefix = TestDataGenerator.GetRandomAlphabeticString(3);
            //TODO Support Currency for Brand before
            //const string bankAccountType = "Affiliate";
            const string brandType = "Credit";
            var country = TestDataGenerator.CountryNames;
            //TODO Support Currency
            //const string currency = "CAD";
            //TODO Support Language Manager
            //const string languageCode = "en-GB";
            TimeSpan _ts = DateTime.Now.TimeOfDay;

            var newBrandForm = _brandManagerPage.OpenNewBrandForm();
            var submittedBrandForm = newBrandForm.Submit(brandName, brandCode, playerPrefix, brandType);
            Assert.AreEqual("The brand has been successfully created.", submittedBrandForm.ConfirmationMessage);
            Assert.AreEqual(licensee, submittedBrandForm.LicenseeValue);
            Assert.AreEqual(brandType, submittedBrandForm.BrandTypeValue);
            Assert.AreEqual(brandName, submittedBrandForm.BrandNameValue);
            Assert.AreEqual(brandCode, submittedBrandForm.BrandCodeValue);
            Assert.AreEqual(playerPrefix, submittedBrandForm.PlayerPrefix);
            submittedBrandForm.CloseTab("View Brand");

            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            //_dashboardPage.BrandFilter.ClearAll();
            //_dashboardPage.BrandFilter.SelectLicense();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();

            //TODO AFTREGO-4128 - VladK.
            //create wallet for brand
            //var walletTemplateListPage = _dashboardPage.Menu.ClickWalletManagerMenuItem();
            //var addWalletTemplateForm = walletTemplateListPage.OpenNewWalletForm();
            //var submittedAddWalletTemplateForm = addWalletTemplateForm.Submit(licensee, brandName);
            //Assert.AreEqual("The wallet has been successfully created", submittedAddWalletTemplateForm.ConfirmationMessage);
            //submittedAddWalletTemplateForm.CloseTab("View Wallet");

            // assign a country to the brand
            var supportedCountriesPage = _brandManagerPage.Menu.ClickSupportedCountriesMenuItem();
            var assignCountryForm = supportedCountriesPage.OpenAssignCountriesForm();
            var submittedAssignCountryForm = assignCountryForm.AssignCountries(licensee, brandName, country);
            Assert.AreEqual("The countries have been successfully assigned", submittedAssignCountryForm.ConfirmationMessage);
            submittedAssignCountryForm.CloseTab("View Assigned Countries");

            //TODO AFTREGO-4096 - VladS.
            // assign a currency to the brand
            //var supportedCurrenciesPage = submittedAssignCurrencyForm.Menu.ClickSupportedCurrenciesMenuItem();
            //var assignCurrencyForm = supportedCurrenciesPage.OpenAssignCurrencyForm();
            //var submittedAssignedCurrencyForm = assignCurrencyForm.Submit(licensee, brandName, currency);
            //Assert.AreEqual("The currencies have been successfully assigned", submittedAssignedCurrencyForm.ConfirmationMessage);
            //submittedAssignedCurrencyForm.CloseTab("View Assigned Currencies");

            //TODO AFTREGO-4130 - VladS.
            //// assign a language to the brand
            //var supportedLanguagesPage = submittedAssignedCurrencyForm.Menu.ClickSupportedLanguagesMenuItem();
            //var assignLanguageForm = supportedLanguagesPage.OpenAssignLanguageForm();

            //var submittedAssignedLanguageForm = assignLanguageForm.Submit(licensee, brandName, languageCode);

            //Assert.AreEqual("The languages have been successfully assigned", submittedAssignedLanguageForm.ConfirmationMessage);

            //TODO AFTREGO-4127 - VladK.
            // assign a product
            //var supportedProductsPage = submittedAssignedLanguageForm.Menu.ClickSupportedProductsMenuItem();
            //var assignProductForm = supportedProductsPage.OpenManageProductsPage();
            //var submittedAssignProductsForm = assignProductForm.AssignProducts(licensee, brandName, new[] {"Mock Casino"});
            //Assert.AreEqual("The products have been successfully assigned", submittedAssignProductsForm.Confirmation);
            //submittedAssignProductsForm.CloseTab("View Assigned Products");

            // create a bank for the brand
            var bankId = randomString;
            var bankName = "bankName" + randomString;
                                   //submittedAssignProductsForm.Menu.ClickBanksItem();
            var banksManagerPage = _dashboardPage.Menu.ClickBanksItem();
            var newBankForm = banksManagerPage.OpenNewBankForm();
            var submittedBankForm = newBankForm.SubmitWithLicensee(licensee, brandName, bankId, bankName, country[1], remarks:"new bank");
            submittedBankForm.CloseTab("View");

            // create a bank account for the brand
            //var bankAccountId = "bankaccountID_" + randomString;
            //var bankAccountNumber = "bankAccountNumber";
            //var bankAccountName = "bankAccountName_" + randomString;
            //var province = "province-" + randomString;
            //var branch = "branch-" + randomString;
            
            //var supplierName = "Supplier Name " + TestDataGenerator.GetRandomAlphabeticString(8);
            //var contactNumber = TestDataGenerator.GetRandomNumber(12222222, 10000000);
            //var usbCode = "USBcode_-" + randomString;

            //var bankAccountsManagerPage = banksManagerPage.Menu.ClickBankAccountsItem();
            //var newBankAccountForm = bankAccountsManagerPage.OpenNewBankAccountForm();
            //var submittedBankAccountForm = newBankAccountForm.SubmitWithLicensee(licensee, brandName, currency, bankAccountId, bankAccountName, bankAccountNumber,
            //     province, branch, bankAccountType, supplierName, contactNumber, usbCode);
            //submittedBankAccountForm.CloseTab("View Bank Account");
            
            //bankAccountsManagerPage = _dashboardPage.Menu.ClickBankAccountsItem();
            //var activateDialog = bankAccountsManagerPage.OpenActivateBankAccountDialog(bankAccountName);
            //var confirmDialog = activateDialog.ActivateBankAccount(remark:"activated");
            //bankAccountsManagerPage = confirmDialog.Close();
            
            // create Default payment level for the brand
            //var paymentLevelCode = "pl" + randomString;
            //var paymentLevelName = "payment-level" + randomString;
            //var paymentLevelsPage = bankAccountsManagerPage.Menu.ClickPaymentLevelsMenuItem();
            //var newPaymentLevelForm = paymentLevelsPage.OpenNewPaymentLevelForm();
            //var submittedPaymentLevelForm = newPaymentLevelForm.Submit(brandName, paymentLevelCode, paymentLevelName, bankAccountId);
            //Assert.AreEqual("The payment level has been created.", submittedPaymentLevelForm.ConfirmationMessage);
            //submittedPaymentLevelForm.CloseTab("View Payment Level");
            
            // create Default vip level for the brand
            //var vipLevelData = TestDataGenerator.CreateValidVipLevelData(licensee, brandName, defaultForNewPlayers:true);

            //var vipLevelManagerPage = paymentLevelsPage.Menu.ClickVipLevelManagerMenuItem();
            //var newVipLevelForm = vipLevelManagerPage.OpenNewVipLevelForm();
            //newVipLevelForm.EnterVipLevelDetails(vipLevelData);
            //var submittedVipLevelForm = newVipLevelForm.Submit();

            //Assert.AreEqual("VIP Level has been created successfully.", submittedVipLevelForm.ConfirmationMessage);
            
            //create a risk level for the brand
            //var fraudManagerPage = vipLevelManagerPage.Menu.OpenFraudManager();

            //var Code = (_ts.Milliseconds + 1000 * (_ts.Seconds + 60 * (_ts.Minutes + 60 * _ts.Hours))).ToString();
            //var Name = "Name_" + TestDataGenerator.GetRandomAlphabeticString(5);
            //var Remarks = "remarks_new FRL";
            
            //generate auto verification configuration form data
            //FraudRiskLevelData data = TestDataGenerator.CreateFraudRiskLevelData(
            //    licensee,
            //    brandName,
            //    Code,
            //    Name,
            //    Remarks
            //    );

            //fraudManagerPage = _dashboardPage.Menu.OpenFraudManager();
            //var _newFRLform = fraudManagerPage.OpenNewFraudRiskLevelForm();
            //_newFRLform.SetFraudRiskLevelFields(data);
            //var viewFRLForm = _newFRLform.SubmitFraudRiskLevel();
            //Assert.AreEqual("The Fraud Risk Level has been successfully created", viewFRLForm.SuccessAlert.Text);
            //viewFRLForm.CloseTab("View Fraud Risk Level");
            //viewFRLForm.CloseTab("Fraud Manager");

            //TODO AFTREGO-4127, 4128 - VladK.
            //activate the Brand
            //_brandManagerPage = fraudManagerPage.Menu.ClickBrandManagerItem();
            //var brandActivateDialog = _brandManagerPage.OpenBrandActivateDialog(brandName);
            //var brandActivatedConfirmDialog = brandActivateDialog.Activate("activated");
            //Assert.AreEqual("This brand has been successfully activated.", brandActivatedConfirmDialog.ConfirmationMessage);
            //brandActivatedConfirmDialog.Close();

            //check the Brand satus
            //Assert.IsTrue(_brandManagerPage.HasActiveStatus(brandName));
        }

        [Test]
        [Ignore("Till Rostislav's investigations, 21-April-2016")]
        public void Can_edit_brand()
        {
            var brandName = "brand-" + TestDataGenerator.GetRandomAlphabeticString(6);
            var brandCode = TestDataGenerator.GetRandomString(size: 4, charsToUse: TestDataGenerator.NumericChars);
            var playerPrefix = TestDataGenerator.GetRandomAlphabeticString(3);
            _dashboardPage.BrandFilter.SelectAll();
            var newBrandForm = _brandManagerPage.OpenNewBrandForm();
            var submittedForm = newBrandForm.Submit(brandName, brandCode, playerPrefix);
            submittedForm.CloseTab("View Brand");

            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();

            var editBrandName = "brand-" + TestDataGenerator.GetRandomAlphabeticString(6);
            var editBrandCode = TestDataGenerator.GetRandomString(size: 4, charsToUse: TestDataGenerator.NumericChars);
            var editBrandForm = _brandManagerPage.OpenEditBrandForm(brandName);
            var submittedEditForm = editBrandForm.EditOnlyRequiredData("Deposit", editBrandName, editBrandCode);

            Assert.AreEqual(editBrandCode, submittedEditForm.BrandCodeValue);
            Assert.AreEqual(editBrandName, submittedEditForm.BrandNameValue);
        }

        [Test]
        public void Cannot_activate_brand_without_country_currency_language_vip_level()
        {
            var brand = "brand-" + TestDataGenerator.GetRandomAlphabeticString(6);
            var brandCode = TestDataGenerator.GetRandomString(size:4, charsToUse:TestDataGenerator.NumericChars);
            var playerPrefix = TestDataGenerator.GetRandomAlphabeticString(3);

            _dashboardPage.BrandFilter.SelectAll();
            var newBrandForm = _brandManagerPage.OpenNewBrandForm();
            var submittedForm = newBrandForm.Submit(brand, brandCode, playerPrefix);
            submittedForm.CloseTab("View Brand");

            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();
            
            var activateDialog = _brandManagerPage.OpenBrandActivateDialog(brand);
            activateDialog.TryToActivate("approved");
            var validationMessages = activateDialog.GetErrorMessages().ToArray();
            
            Assert.That(validationMessages.Length, Is.EqualTo(8));
            Assert.That(validationMessages[0].Text, Is.EqualTo("A wallet must be assigned prior to activation."));
            Assert.That(validationMessages[1].Text, Is.EqualTo("A country must be assigned prior to activation."));
            Assert.That(validationMessages[2].Text, Is.EqualTo("A currency must be assigned prior to activation."));
            Assert.That(validationMessages[3].Text, Is.EqualTo("A language must be assigned prior to activation."));
            Assert.That(validationMessages[4].Text, Is.EqualTo("A default VIP level is required prior to activation."));
            Assert.That(validationMessages[5].Text, Is.EqualTo("A product must be assigned prior to activation."));
            Assert.That(validationMessages[6].Text, Is.EqualTo("A risk level is required prior to activation."));
            Assert.That(validationMessages[7].Text, Is.EqualTo("A default payment level is required for each currency prior to activation."));
        }

        [Test]
        public void Can_view_brand()
        {
            const string defaultLicensee = "Flycow";
            var brandQueries = _container.Resolve<BrandQueries>();
            var licensee = brandQueries.GetLicensees().First(x => x.Name == defaultLicensee);
           
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var brand = brandTestHelper.CreateBrand(licensee);

            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();

            var viewBrandForm = _brandManagerPage.OpenViewBrandForm(brand.Name);
            
            Assert.AreEqual(licensee.Name, viewBrandForm.Licensee);
            Assert.AreEqual(brand.Name, viewBrandForm.BrandName);
            Assert.AreEqual(brand.Type.ToString(), viewBrandForm.BrandType);
            Assert.AreEqual(brand.Code, viewBrandForm.BrandCode);
            Assert.AreEqual(brand.Status.ToString(), viewBrandForm.Status);
            Assert.AreEqual(brand.PlayerPrefix, viewBrandForm.PlayerPrefix);
        }

        [Test]
        public void Can_deactivate_brand()
        {
            const string defaultLicensee = "Flycow";
            var brandQueries = _container.Resolve<BrandQueries>();
            var licensee = brandQueries.GetLicensees().First(x => x.Name == defaultLicensee);
            //creatr brand
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var brand = brandTestHelper.CreateBrand(licensee);
            //check brand            
            _dashboardPage.BrandFilter.SelectAll();            
            var viewBrandForm = _brandManagerPage.OpenViewBrandForm(brand.Name);
            Assert.AreEqual(licensee.Name, viewBrandForm.Licensee);
            Assert.AreEqual(brand.Name, viewBrandForm.BrandName);
            Assert.AreEqual(brand.Type.ToString(), viewBrandForm.BrandType);
            Assert.AreEqual(brand.Code, viewBrandForm.BrandCode);
            Assert.AreEqual(brand.Status.ToString(), viewBrandForm.Status);
            Assert.AreEqual(brand.PlayerPrefix, viewBrandForm.PlayerPrefix);
            viewBrandForm.CloseTab("View Brand");
            //activate brand
            var activateDialog =_brandManagerPage.OpenBrandActivateDialog(brand.Name);
            var confirmActivateDialog = activateDialog.Activate("approved activate");
            Assert.AreEqual("This brand has been successfully activated.", confirmActivateDialog.ConfirmationMessage);
            confirmActivateDialog.Close();
            //deactivate brand
            var deactivateDialog = _brandManagerPage.OpenBrandDeactivateDialog(brand.Name);
            var confirmDeactivateDialog = deactivateDialog.Deactivate("approved deactivate");
            Assert.AreEqual("This brand has been successfully deactivated", confirmActivateDialog.ConfirmationMessage);
            confirmActivateDialog.Close();
            //check brand's deactivate
            Assert.IsTrue(_brandManagerPage.CheckDeactivatedBrandStatus(brand.Name));
        }

        [Test]
        [Ignore("Supported Product Manage buttom disabled for now - 17-March-2016, Igor")]
        public void Can_assign_products_to_brand()
        {
            //create brand
            const string licensee = "Flycow";
            var brandName = "brand-" + TestDataGenerator.GetRandomAlphabeticString(6);
            var brandCode = TestDataGenerator.GetRandomString(size: 4, charsToUse: TestDataGenerator.NumericChars);
            var playerPrefix = TestDataGenerator.GetRandomAlphabeticString(3);
            _dashboardPage.BrandFilter.SelectAll();
            var newBrandForm = _brandManagerPage.OpenNewBrandForm();
            var submittedBrandForm = newBrandForm.Submit(brandName, brandCode, playerPrefix);
            Assert.AreEqual("The brand has been successfully created.", submittedBrandForm.ConfirmationMessage);
            submittedBrandForm.CloseTab("View Brand");
            //check brand
            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();
            var viewBrandForm = _brandManagerPage.OpenViewBrandForm(brandName);
            Assert.AreEqual(licensee, viewBrandForm.Licensee);
            Assert.AreEqual(brandName, viewBrandForm.BrandName);
            Assert.AreEqual("Deposit", viewBrandForm.BrandType);
            Assert.AreEqual(brandCode, viewBrandForm.BrandCode);
            Assert.AreEqual("Inactive", viewBrandForm.Status);
            Assert.AreEqual(playerPrefix, viewBrandForm.PlayerPrefix);
            viewBrandForm.CloseTab("View Brand");
            //assign product to brand
            var supportedProductsPage = _brandManagerPage.Menu.ClickSupportedProductsMenuItem();
            var assignProductForm = supportedProductsPage.OpenManageProductsPage();            
            var submittedAssignProductsForm = assignProductForm.AssignProducts(licensee, brandName, new[] { "Mock Sport Bets", "Mock Casino" });
            Assert.AreEqual("The products have been successfully assigned", submittedAssignProductsForm.Confirmation);
            Assert.AreEqual(licensee, submittedAssignProductsForm.Licensee);
            Assert.AreEqual(brandName, submittedAssignProductsForm.Brand);
            Assert.IsTrue(submittedAssignProductsForm.IsProductDisplayed("Mock Sport Bets"));
            Assert.IsTrue(submittedAssignProductsForm.IsProductDisplayed("Mock Casino"));
            submittedAssignProductsForm.CloseTab("View Assigned Products");
        }

        [Test, Ignore("AFTREGO-4378 Rostyslav 03/28/2016")]
        public void Can_unassign_products_to_brand()
        {
            //create brand
            const string licensee = "Flycow";
            var brandName = "brand-" + TestDataGenerator.GetRandomAlphabeticString(6);
            var brandCode = TestDataGenerator.GetRandomString(size: 4, charsToUse: TestDataGenerator.NumericChars);
            var playerPrefix = TestDataGenerator.GetRandomAlphabeticString(3);
            _dashboardPage.BrandFilter.SelectAll();
            var newBrandForm = _brandManagerPage.OpenNewBrandForm();
            var submittedBrandForm = newBrandForm.Submit(brandName, brandCode, playerPrefix);
            Assert.AreEqual("The brand has been successfully created.", submittedBrandForm.ConfirmationMessage);
            submittedBrandForm.CloseTab("View Brand");
            //check brand
            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();
            var viewBrandForm = _brandManagerPage.OpenViewBrandForm(brandName);
            Assert.AreEqual(licensee, viewBrandForm.Licensee);
            Assert.AreEqual(brandName, viewBrandForm.BrandName);
            Assert.AreEqual("Deposit", viewBrandForm.BrandType);
            Assert.AreEqual(brandCode, viewBrandForm.BrandCode);
            Assert.AreEqual("Inactive", viewBrandForm.Status);
            Assert.AreEqual(playerPrefix, viewBrandForm.PlayerPrefix);
            viewBrandForm.CloseTab("View Brand");
            //assign product to brand
            var supportedProductsPage = _brandManagerPage.Menu.ClickSupportedProductsMenuItem();
            var assignProductForm = supportedProductsPage.OpenManageProductsPage();
            var submittedAssignProductsForm = assignProductForm.AssignProducts(licensee, brandName, new[] { "Mock Sport Bets", "Mock Casino" });
            Assert.AreEqual("The products have been successfully assigned", submittedAssignProductsForm.Confirmation);
            Assert.AreEqual(licensee, submittedAssignProductsForm.Licensee);
            Assert.AreEqual(brandName, submittedAssignProductsForm.Brand);
            Assert.IsTrue(submittedAssignProductsForm.IsProductDisplayed("Mock Sport Bets"));
            Assert.IsTrue(submittedAssignProductsForm.IsProductDisplayed("Mock Casino"));
            submittedAssignProductsForm.CloseTab("View Assigned Products");
            //unassign product to brand
            var unassignProductForm = supportedProductsPage.OpenManageProductsPage();
            var submittedUnassignProductsForm = unassignProductForm.UnassignProducts(licensee, brandName, new[] { "Mock Casino" });
            Assert.AreEqual("The products have been successfully assigned", submittedUnassignProductsForm.Confirmation);
            Assert.AreEqual(licensee, submittedUnassignProductsForm.Licensee);
            Assert.AreEqual(brandName, submittedUnassignProductsForm.Brand);
            Assert.IsTrue(submittedUnassignProductsForm.IsProductDisplayed("Mock Sport Bets"));            
        }

        [Test, Ignore("AFTREGO-4379 Rostyslav 03/28/2016")]
        public void Can_assign_countries_to_brand()
        {
            //create brand
            const string licensee = "Flycow";
            var brandName = "brand-" + TestDataGenerator.GetRandomAlphabeticString(6);
            var brandCode = TestDataGenerator.GetRandomString(size: 4, charsToUse: TestDataGenerator.NumericChars);
            var playerPrefix = TestDataGenerator.GetRandomAlphabeticString(3);
            var country = TestDataGenerator.CountryNames;
            _dashboardPage.BrandFilter.SelectAll();
            var newBrandForm = _brandManagerPage.OpenNewBrandForm();
            var submittedBrandForm = newBrandForm.Submit(brandName, brandCode, playerPrefix);
            Assert.AreEqual("The brand has been successfully created.", submittedBrandForm.ConfirmationMessage);
            submittedBrandForm.CloseTab("View Brand");
            //check brand
            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();
            var viewBrandForm = _brandManagerPage.OpenViewBrandForm(brandName);
            Assert.AreEqual(licensee, viewBrandForm.Licensee);
            Assert.AreEqual(brandName, viewBrandForm.BrandName);
            Assert.AreEqual("Deposit", viewBrandForm.BrandType);
            Assert.AreEqual(brandCode, viewBrandForm.BrandCode);
            Assert.AreEqual("Inactive", viewBrandForm.Status);
            Assert.AreEqual(playerPrefix, viewBrandForm.PlayerPrefix);
            viewBrandForm.CloseTab("View Brand");
            //assign countries to brand
            var supportedCountriesPage = _brandManagerPage.Menu.ClickSupportedCountriesMenuItem();
            var assignCountriesForm = supportedCountriesPage.OpenAssignCountriesForm();
            var submittedAssignCountriesForm = assignCountriesForm.AssignCountries(licensee, brandName, country);
            Assert.AreEqual("The countries have been successfully assigned", submittedAssignCountriesForm.ConfirmationMessage);
            Assert.AreEqual(licensee, submittedAssignCountriesForm.Licensee);
            Assert.AreEqual(brandName, submittedAssignCountriesForm.Brand);
            Assert.IsTrue(submittedAssignCountriesForm.IsCountryDisplayed(country[1]));
            Assert.IsTrue(submittedAssignCountriesForm.IsCountryDisplayed(country[2]));
        }

        [Test, Ignore("AFTREGO-4379 Rostyslav 04/05/2016")]
        public void Can_unassign_countries_to_brand()
        {
            //create brand
            const string licensee = "Flycow";
            var brandName = "brand-" + TestDataGenerator.GetRandomAlphabeticString(6);
            var brandCode = TestDataGenerator.GetRandomString(size: 4, charsToUse: TestDataGenerator.NumericChars);
            var playerPrefix = TestDataGenerator.GetRandomAlphabeticString(3);
            var country = TestDataGenerator.CountryNames;
            _dashboardPage.BrandFilter.SelectAll();
            var newBrandForm = _brandManagerPage.OpenNewBrandForm();
            var submittedBrandForm = newBrandForm.Submit(brandName, brandCode, playerPrefix);
            Assert.AreEqual("The brand has been successfully created.", submittedBrandForm.ConfirmationMessage);
            submittedBrandForm.CloseTab("View Brand");
            //check brand
            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();
            var viewBrandForm = _brandManagerPage.OpenViewBrandForm(brandName);
            Assert.AreEqual(licensee, viewBrandForm.Licensee);
            Assert.AreEqual(brandName, viewBrandForm.BrandName);
            Assert.AreEqual("Deposit", viewBrandForm.BrandType);
            Assert.AreEqual(brandCode, viewBrandForm.BrandCode);
            Assert.AreEqual("Inactive", viewBrandForm.Status);
            Assert.AreEqual(playerPrefix, viewBrandForm.PlayerPrefix);
            viewBrandForm.CloseTab("View Brand");
            //assign countries to brand
            var supportedCountriesPage = _brandManagerPage.Menu.ClickSupportedCountriesMenuItem();
            var assignCountriesForm = supportedCountriesPage.OpenAssignCountriesForm();
            var submittedAssignCountriesForm = assignCountriesForm.AssignCountries(licensee, brandName, country);
            Assert.AreEqual("The countries have been successfully assigned", submittedAssignCountriesForm.ConfirmationMessage);
            Assert.AreEqual(licensee, submittedAssignCountriesForm.Licensee);
            Assert.AreEqual(brandName, submittedAssignCountriesForm.Brand);
            Assert.IsTrue(submittedAssignCountriesForm.IsCountryDisplayed(country[0]));
            Assert.IsTrue(submittedAssignCountriesForm.IsCountryDisplayed(country[1]));
            Assert.IsTrue(submittedAssignCountriesForm.IsCountryDisplayed(country[2]));
            Assert.IsTrue(submittedAssignCountriesForm.IsCountryDisplayed(country[3]));
            submittedAssignCountriesForm.CloseTab("View Assigned Countries");
            //unassign countries to brand
            var unassignCountriesForm = supportedCountriesPage.OpenAssignCountriesForm();
            var submittedUnassignCountriesForm = unassignCountriesForm.UnassignCountries(licensee, brandName, new string[] { country[0], country[1] });
            Assert.AreEqual("The countries have been successfully assigned", submittedUnassignCountriesForm.ConfirmationMessage);
            Assert.AreEqual(licensee, submittedUnassignCountriesForm.Licensee);
            Assert.AreEqual(brandName, submittedUnassignCountriesForm.Brand);
            Assert.IsTrue(submittedUnassignCountriesForm.IsCountryDisplayed(country[2]));
            Assert.IsTrue(submittedUnassignCountriesForm.IsCountryDisplayed(country[3]));
        }

        [Test]
        public void Can_assign_more_countries_to_brand()
        {
            //create brand
            const string licensee = "Flycow";
            var brandName = "brand-" + TestDataGenerator.GetRandomAlphabeticString(6);
            var brandCode = TestDataGenerator.GetRandomString(size: 4, charsToUse: TestDataGenerator.NumericChars);
            var playerPrefix = TestDataGenerator.GetRandomAlphabeticString(3);
            var country = TestDataGenerator.CountryNames;
            _dashboardPage.BrandFilter.SelectAll();
            var newBrandForm = _brandManagerPage.OpenNewBrandForm();
            var submittedBrandForm = newBrandForm.Submit(brandName, brandCode, playerPrefix);
            Assert.AreEqual("The brand has been successfully created.", submittedBrandForm.ConfirmationMessage);
            submittedBrandForm.CloseTab("View Brand");
            //check brand
            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();
            _brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();
            var viewBrandForm = _brandManagerPage.OpenViewBrandForm(brandName);
            Assert.AreEqual(licensee, viewBrandForm.Licensee);
            Assert.AreEqual(brandName, viewBrandForm.BrandName);
            Assert.AreEqual("Deposit", viewBrandForm.BrandType);
            Assert.AreEqual(brandCode, viewBrandForm.BrandCode);
            Assert.AreEqual("Inactive", viewBrandForm.Status);
            Assert.AreEqual(playerPrefix, viewBrandForm.PlayerPrefix);
            viewBrandForm.CloseTab("View Brand");
            //assign countries to brand
            var supportedCountriesPage = _brandManagerPage.Menu.ClickSupportedCountriesMenuItem();
            var assignCountriesForm = supportedCountriesPage.OpenAssignCountriesForm();
            var submittedAssignCountriesForm = assignCountriesForm.AssignCountries(licensee, brandName, new string[]{ country[0], country[1] });            
            Assert.AreEqual("The countries have been successfully assigned", submittedAssignCountriesForm.ConfirmationMessage);
            Assert.AreEqual(licensee, submittedAssignCountriesForm.Licensee);
            Assert.AreEqual(brandName, submittedAssignCountriesForm.Brand);
            Assert.IsTrue(submittedAssignCountriesForm.IsCountryDisplayed(country[0]));
            Assert.IsTrue(submittedAssignCountriesForm.IsCountryDisplayed(country[1]));
            submittedAssignCountriesForm.CloseTab("View Assigned Countries");
            //assign more countries to brand
            assignCountriesForm = supportedCountriesPage.OpenAssignCountriesForm();
            var submittedMoreAssignCountriesForm = assignCountriesForm.AssignCountries(licensee, brandName, new string[] { country[2], country[3] });
            Assert.AreEqual("The countries have been successfully assigned", submittedMoreAssignCountriesForm.ConfirmationMessage);
            Assert.AreEqual(licensee, submittedMoreAssignCountriesForm.Licensee);
            Assert.AreEqual(brandName, submittedMoreAssignCountriesForm.Brand);
            Assert.IsTrue(submittedMoreAssignCountriesForm.IsCountryDisplayed(country[0]));
            Assert.IsTrue(submittedMoreAssignCountriesForm.IsCountryDisplayed(country[1]));
            Assert.IsTrue(submittedMoreAssignCountriesForm.IsCountryDisplayed(country[2]));
            Assert.IsTrue(submittedMoreAssignCountriesForm.IsCountryDisplayed(country[3]));
        }

        [Test]
        public void Can_assign_languages_to_brand()
        {            
            var languageCode = new string[] { "en-US", "zh-CN", "zh-TW" };
            const string defaultLicensee = "Flycow";
            var brandQueries = _container.Resolve<BrandQueries>();
            var licensee = brandQueries.GetLicensees().First(x => x.Name == defaultLicensee);
            //creatr brand
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var brand = brandTestHelper.CreateBrand(licensee);

            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();
                        
            var supportedLanguagesPage = _brandManagerPage.Menu.ClickSupportedLanguagesMenuItem();
            var assignLanguageForm = supportedLanguagesPage.OpenAssignLanguageForm();
            //assign languages to brand
            var submittedAssignedLanguageForm = assignLanguageForm.Submit(licensee.Name, brand.Name, languageCode);
            Assert.AreEqual("The languages have been successfully assigned", submittedAssignedLanguageForm.ConfirmationMessage);
            Assert.AreEqual(licensee.Name, submittedAssignedLanguageForm.Licensee);
            Assert.AreEqual(brand.Name, submittedAssignedLanguageForm.Brand);
            Assert.AreEqual(languageCode[0], submittedAssignedLanguageForm.Language(languageCode[0]));
            Assert.AreEqual(languageCode[1], submittedAssignedLanguageForm.Language(languageCode[1]));
            Assert.AreEqual(languageCode[2], submittedAssignedLanguageForm.Language(languageCode[2]));
            Assert.AreEqual(languageCode[0], submittedAssignedLanguageForm.DefaultLanguage);
        }

        [Test, Ignore("AFTREGO-4380 Rostyslav 03/28/2016")]
        public void Can_unassign_languages_to_brand()
        {
            var languageCode = new string[] { "en-US", "zh-CN", "zh-TW" };
            const string defaultLicensee = "Flycow";
            var brandQueries = _container.Resolve<BrandQueries>();
            var licensee = brandQueries.GetLicensees().First(x => x.Name == defaultLicensee);
            //creatr brand
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var brand = brandTestHelper.CreateBrand(licensee);

            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();

            var supportedLanguagesPage = _brandManagerPage.Menu.ClickSupportedLanguagesMenuItem();
            var assignLanguageForm = supportedLanguagesPage.OpenAssignLanguageForm();
            //assign languages to brand
            var submittedAssignedLanguageForm = assignLanguageForm.Submit(licensee.Name, brand.Name, languageCode);
            Assert.AreEqual("The languages have been successfully assigned", submittedAssignedLanguageForm.ConfirmationMessage);
            Assert.AreEqual(licensee.Name, submittedAssignedLanguageForm.Licensee);
            Assert.AreEqual(brand.Name, submittedAssignedLanguageForm.Brand);
            Assert.AreEqual(languageCode[0], submittedAssignedLanguageForm.Language(languageCode[0]));
            Assert.AreEqual(languageCode[1], submittedAssignedLanguageForm.Language(languageCode[1]));
            Assert.AreEqual(languageCode[2], submittedAssignedLanguageForm.Language(languageCode[2]));
            Assert.AreEqual(languageCode[0], submittedAssignedLanguageForm.DefaultLanguage);
            submittedAssignedLanguageForm.CloseTab("View Assigned Languages");
            //unassign languages to brand
            var unassignLanguageForm = supportedLanguagesPage.OpenAssignLanguageForm();
            var submittedUnassignedLanguageForm = unassignLanguageForm.SubmitUnassign(licensee.Name, brand.Name, new string[] { languageCode[2] });
            Assert.AreEqual("The languages have been successfully assigned", submittedAssignedLanguageForm.ConfirmationMessage);
            Assert.AreEqual(licensee.Name, submittedAssignedLanguageForm.Licensee);
            Assert.AreEqual(brand.Name, submittedAssignedLanguageForm.Brand);
            Assert.AreEqual(languageCode[0], submittedAssignedLanguageForm.Language(languageCode[0]));
            Assert.AreEqual(languageCode[1], submittedAssignedLanguageForm.Language(languageCode[1]));            
            Assert.AreEqual(languageCode[0], submittedAssignedLanguageForm.DefaultLanguage);
        }

        [Test]
        public void Can_not_unassigne_languages_from_active_brand()
        {            
            const string defaultLicensee = "Flycow";
            var brandQueries = _container.Resolve<BrandQueries>();
            var licensee = brandQueries.GetLicensees().First(x => x.Name == defaultLicensee);
            //creatr brand
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var brand = brandTestHelper.CreateBrand(licensee, isActive:true);

            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();

            var supportedLanguagesPage = _brandManagerPage.Menu.ClickSupportedLanguagesMenuItem();
            var assignLanguageForm = supportedLanguagesPage.OpenAssignLanguageForm();
            //check unassign button
            var unassignButtonDisplayed = assignLanguageForm.IsUnassignButtonDisplayed(licensee.Name, brand.Name);
            Assert.IsFalse(unassignButtonDisplayed);
        }

        [Test]
        public void Can_not_change_default_languages_for_active_brand()
        {
            const string defaultLicensee = "Flycow";
            var brandQueries = _container.Resolve<BrandQueries>();
            var licensee = brandQueries.GetLicensees().First(x => x.Name == defaultLicensee);
            //creatr brand
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);

            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();

            var supportedLanguagesPage = _brandManagerPage.Menu.ClickSupportedLanguagesMenuItem();
            var assignLanguageForm = supportedLanguagesPage.OpenAssignLanguageForm();
            //check default language's dropdown box
            var defaultLanguageDropdownBoxDisplayed = assignLanguageForm.IsDefaultLanguageDropdownBoxDisplayed(licensee.Name, brand.Name);
            Assert.IsFalse(defaultLanguageDropdownBoxDisplayed);
        }
    }
}
