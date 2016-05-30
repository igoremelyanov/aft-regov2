using System;
using System.Threading;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using System.Globalization;

namespace AFT.RegoV2.Tests.Selenium
{
    class PlayerManagerTests : SeleniumBaseForAdminWebsite
    {
        private PlayerManagerPage _playerManagerPage;
        private DashboardPage _dashboardPage;
        private BrandQueries _brandQueries;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private static readonly Guid DefaultBrandId = Guid.Parse("00000000-0000-0000-0000-000000000138");
        private PlayerTestHelper _playerTestHelper;
        private IPaymentSettingsCommands _paymentSettingsCommands;
        private ICommonSettingsProvider _settingsProvider;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            _settingsProvider = _container.Resolve<ICommonSettingsProvider>();
            _brandQueries = _container.Resolve<BrandQueries>();
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _paymentSettingsCommands = _container.Resolve<IPaymentSettingsCommands>();
        }

        [Test, Ignore("AFTREGO-4518 Rostyslav 04/06/2016")]
        public void Can_create_and_view_player()
        {
            // create a player
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand, currency: "RMB", country: "US");

            var newPlayerForm = _playerManagerPage.OpenNewPlayerForm();
            var submittedForm = newPlayerForm.Register(playerData);
            Assert.AreEqual("The player has been successfully created", submittedForm.ConfirmationMessage);
            submittedForm.CloseTab("View Player");
            //check a player
            var playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            playerInfoPage.OpenAccountInformationSection();
            var playerAccountInfo = playerInfoPage.GetAccountDetails();


            Assert.AreEqual(playerData.LoginName, playerAccountInfo.Username);
            Assert.AreEqual(playerData.FirstName, playerAccountInfo.FirstName);
            Assert.AreEqual(playerData.LastName, playerAccountInfo.LastName);
            Assert.AreEqual(playerData.Title, playerAccountInfo.Title);
            Assert.AreEqual(playerData.Gender, playerAccountInfo.Gender);
            Assert.AreEqual(playerData.Email, playerAccountInfo.EmailAddress);
            Assert.AreEqual(playerData.MobileNumber, playerAccountInfo.MobileNumber);
            Assert.AreEqual(playerData.Address, playerAccountInfo.AddressLine1);
            Assert.AreEqual(playerData.City, playerAccountInfo.City);
            Assert.AreEqual(playerData.ZipCode, playerAccountInfo.PostalCode);
            Assert.AreEqual("United States", playerAccountInfo.Country);
            Assert.AreEqual("RMBLevel", playerAccountInfo.PaymentLevel);
            Assert.AreEqual(playerData.ContactPreference, playerAccountInfo.PrimaryContact);
            Assert.AreEqual("Email, Sms", playerAccountInfo.AccountAlerts);
            Assert.AreEqual("Email, Sms, Phone", playerAccountInfo.MarketingAlerts);           
        }

        [Test, Ignore("AFTREGO-4427 Rostyslav 04/05/2016")]
        public void Can_deactivate_player()
        {
            // create a player
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand);
            var newPlayerForm = _playerManagerPage.OpenNewPlayerForm();
            var submittedForm = newPlayerForm.Register(playerData);
            Assert.AreEqual("The player has been successfully created", submittedForm.ConfirmationMessage);
            submittedForm.CloseTab("View Player");

            //deactivate a player
            var playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            playerInfoPage.OpenAccountInformationSection();
            playerInfoPage.DeactivatePlayer();
            playerInfoPage.CloseTab("Player Info");

            //check player in admin website
            _driver.Navigate().Refresh();
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            _playerManagerPage.SelectPlayer(playerData.LoginName);
            Assert.AreEqual("Inactive", _playerManagerPage.Status);

            //check deactivate a player in member website (try to login)
            var _memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _memberWebsiteLoginPage.NavigateToMemberWebsite();
            _memberWebsiteLoginPage.TryToLogin(playerData.LoginName, playerData.Password);
            Assert.AreEqual("Non active", _memberWebsiteLoginPage.GetErrorMsg());
        }

        [Test, Ignore("AFTREGO-4518 Rostyslav 04/06/2016")]
        public void Can_freeze_player()
        {
            // create a player
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand);
            var newPlayerForm = _playerManagerPage.OpenNewPlayerForm();
            var submittedForm = newPlayerForm.Register(playerData);
            Assert.AreEqual("The player has been successfully created", submittedForm.ConfirmationMessage);
            submittedForm.CloseTab("View Player");

            //activate frozen status a player in admin website            
            var _playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            _playerInfoPage.OpenAccountInformationSection();
            var freezeStatus = _playerInfoPage.ChangeFreezeStatusOfPlayer();
            Assert.AreEqual("Yes", freezeStatus);

            //check freeze statu a player in member website (can't do withdrawal)
            var _memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);            
            _memberWebsiteLoginPage.NavigateToMemberWebsite();
            var _playerProfilePage = _memberWebsiteLoginPage.Login(playerData.LoginName, playerData.Password);
            var _withdrawalPage = _playerProfilePage.Menu.ClickWithdrawalDropMenu();
            var _frozenDescription = _withdrawalPage.FrozenDescription();
            Assert.IsTrue(_frozenDescription.Contains("Your account has been frozen and cannot proceed the withdrawal request."));

            //check freeze statu a player in member website(can't do betting)
        }

        [Test, Ignore("AFTREGO-4506 Rostyslav 04/05/2016")]
        public void Can_create_bank_account_and_bind_to_player_record()
        {            
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand);
            var playerBankAccountData = new PlayerBankAccountData
            {
                BankName = "Bank of Canada",
                Province = TestDataGenerator.GetRandomString(),
                City = TestDataGenerator.GetRandomString(),
                Branch = TestDataGenerator.GetRandomString(),
                SwiftCode = TestDataGenerator.GetRandomString(),
                Address = TestDataGenerator.GetRandomString(),
                BankAccountName = TestDataGenerator.GetRandomString(),
                BankAccountNumber = TestDataGenerator.GetRandomString(7, "0123456789")                                                       
            };

            // create a player
            var newPlayerForm = _playerManagerPage.OpenNewPlayerForm();
            var submittedForm = newPlayerForm.Register(playerData);
            Assert.AreEqual("The player has been successfully created", submittedForm.ConfirmationMessage);
            submittedForm.CloseTab("View Player");

            //create bank account
            var _playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            _playerInfoPage.OpenBankAccountsSection();
            var _newBankAccountForm = _playerInfoPage.OpenNewBankAccountTab();
            var _submittedBankAccountForm = _newBankAccountForm.Submit(playerBankAccountData);
            Assert.AreEqual("The bank account has been successfully created", _submittedBankAccountForm.ConfirmationMessage);
            _playerManagerPage.CloseTab("View Bank Account");
            _playerManagerPage.CloseTab("Player Info");

            //verify bank account
            var playerBankAccountVerifyPage = _playerManagerPage.Menu.ClickPlayerBankAccountVerifyMenuItem();
            playerBankAccountVerifyPage.Verify(playerBankAccountData.BankAccountName);

            //open withdraw request form
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            _playerManagerPage.SelectPlayer(playerData.LoginName);
            var withdrawRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(playerData.LoginName);
            Assert.AreEqual(playerBankAccountData.BankName, withdrawRequestForm.BankNameValue);
            Assert.AreEqual(playerBankAccountData.Province, withdrawRequestForm.ProvinceValue);
            Assert.AreEqual(playerBankAccountData.City, withdrawRequestForm.CityValue);
            Assert.AreEqual(playerBankAccountData.BankAccountName, withdrawRequestForm.BankAccountNameValue);
            Assert.AreEqual(playerBankAccountData.BankAccountNumber, withdrawRequestForm.BankAccountNumberValue);
        }

        [Test, Ignore("AFTREGO-4518 Rostyslav 04/06/2016")]
        public void Can_view_default_payment_level_automatically_applied_to_player()
        {
            // create a player
            const string paymentLevel = "RMBLevel";
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand, currency: "RMB");

            var newPlayerForm = _playerManagerPage.OpenNewPlayerForm();
            var submittedForm = newPlayerForm.Register(playerData);
            Assert.AreEqual("The player has been successfully created", submittedForm.ConfirmationMessage);

            submittedForm.CloseTab("View Player");
            var playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            playerInfoPage.OpenAccountInformationSection();
            var playerAccountInfo = playerInfoPage.GetAccountDetails();

            Assert.AreEqual(paymentLevel, playerAccountInfo.PaymentLevel);
        }

        [Test, Ignore("AFTREGO-4518 Rostyslav 04/06/2016")]
        public void Can_view_default_vip_level_automatically_applied_to_player()
        {
            // create VIP level for "138" brand
            var brand = _brandQueries.GetBrand(DefaultBrandId);
            var vipLevelData = brand.DefaultVipLevel;

            // create a player
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand, currency: "CAD");
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var newPlayerForm = playerManagerPage.OpenNewPlayerForm();
            var submittedPlayerForm = newPlayerForm.Register(playerData);
            submittedPlayerForm.CloseTab("View Player");

            // view VIP level applied in Player Info
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            playerInfoPage.OpenAccountInformationSection();
            var playerAccountInfo = playerInfoPage.GetAccountDetails();

            Assert.AreEqual(vipLevelData.Name, playerAccountInfo.VIPLevel);
        }

        [Test]
        [Ignore("Failing unstable test - 25-April-2016, Igor")]
        public void Cannot_login_to_brand_website_as_deactivated_player()
        {
            var player = _playerTestHelper.CreatePlayerForMemberWebsite();

            //deactivate a player
            var playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(player.Username);
            playerInfoPage.OpenAccountInformationSection();
            playerInfoPage.DeactivatePlayer();

            //Refresh the page as a temporary solution
            _driver.Navigate().Refresh();
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            _playerManagerPage.SelectPlayer(player.Username);
            Assert.AreEqual("Inactive", _playerManagerPage.Status);

            //try to log in to the brand website
            var brandWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            brandWebsiteLoginPage.NavigateToMemberWebsite();
            brandWebsiteLoginPage.TryToLogin(player.Username, player.Password);

            Assert.AreEqual("Non active", brandWebsiteLoginPage.GetErrorMsg());

            var expectedUrl = _settingsProvider.GetMemberWebsiteUrl() + "Home/PlayerProfile";
            var actualUrl = _driver.Url;

            Assert.AreNotEqual(expectedUrl, actualUrl);
        }

        [Test, Ignore("AFTREGO-4518 Rostyslav 04/06/2016")]
        public void Can_edit_player_account_details()
        {
            // create a player
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand, currency: "CAD");
            var editedPlayerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand);
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var newplayerForm = playerManagerPage.OpenNewPlayerForm();
            var submittedForm = newplayerForm.Register(playerData);
            Assert.AreEqual("The player has been successfully created", submittedForm.ConfirmationMessage);            
            
            // login as the user and edit player's details
            _driver.Navigate().Refresh();
            var dashboardPage = new DashboardPage(_driver);
            playerManagerPage = dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfo = playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            playerInfo.OpenAccountDetailsInEditMode();
            playerInfo.Edit(editedPlayerData);
            playerInfo.CloseTab("Player Info");

            //check edited player
            playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            playerInfo.OpenAccountInformationSection();
            var playerAccountInfo = playerInfo.GetAccountDetails();
            Assert.AreEqual(editedPlayerData.FirstName, playerInfo.FirstName);
            Assert.AreEqual(editedPlayerData.LastName, playerInfo.LastName);
            Assert.AreEqual(editedPlayerData.Email, playerAccountInfo.EmailAddress);
            Assert.AreEqual(editedPlayerData.MobileNumber, playerAccountInfo.MobileNumber);            
            Assert.AreEqual("United States", playerAccountInfo.Country);
        }

        [Test, Ignore("AFTREGO-4518 Rostyslav 04/06/2016")]
        public void Can_send_new_password_to_player()
        {
            //create a player
            var playerData = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(DefaultLicensee, DefaultBrand,
               currency: "CAD");
            var newPlayerForm = _playerManagerPage.OpenNewPlayerForm();
            var submittedPlayerForm = newPlayerForm.Register(playerData);
            Assert.AreEqual("The player has been successfully created", submittedPlayerForm.ConfirmationMessage);
            submittedPlayerForm.CloseTab("View Player");

            // send a new password to the player
            var newPassword = TestDataGenerator.GetRandomString(8);
            var playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(playerData.LoginName);
            playerInfoPage.OpenAccountInformationSection();

            var sendNewPasswordDialog = playerInfoPage.OpenSendNewPasswordDialog();
            sendNewPasswordDialog.SpecifyNewPassword(newPassword);
            sendNewPasswordDialog.Send();

            Assert.AreEqual("New password has been successfully sent", sendNewPasswordDialog.ConfirmationMessage);

            // register a player on a brand website
            var brandWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            brandWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = brandWebsiteLoginPage.Login(playerData.LoginName, newPassword);

            //Assert.AreEqual(playerData.LoginName, playerProfilePage.GetUserName());
            Assert.That(playerProfilePage.GetUserName(), Is.StringContaining(playerData.LoginName));
        }

        [Test]
        [Ignore("Till Rostislav's investigations, 21-April-2016")]
        //TODO: VladS  AFTREGO-4540, Igor, 08-April-2016
        public void Can_change_vip_level_of_player_and_view_new_payment_settings_are_applied()
        {
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            var playerTestHelper = _container.Resolve<PlayerTestHelper>();
            var defaultLicenseeId = brandTestHelper.GetDefaultLicensee();

            //create a brand for a default licensee
            var brand = brandTestHelper.CreateBrand(defaultLicenseeId, null, null, null, true);
            paymentTestHelper.CreatePaymentSettings(brand, PaymentType.Deposit);

            // create a player with a bound bank account for a brand
            var player = playerTestHelper.CreatePlayer(true, brand.Id);
            Thread.Sleep(5000);
            var playerUsername = player.Username;
            paymentTestHelper.CreatePlayerBankAccount(player.Id, DefaultBrandId, true);
            var defaultVipLevel = player.VipLevel;

            // create second brand's vip level and payment settings based on it
            var secondVipLevel = brandTestHelper.CreateVipLevel(brand.Id, isDefault: false);
            var settings = new PaymentSettingsValues
            {
                MinAmountPerTransaction = 5,
                MaxAmountPerTransaction = 9
            };
            var paymentSettingsForSecondVipLevel = paymentTestHelper.CreatePaymentSettings(brand, PaymentType.Deposit, secondVipLevel.Id.ToString(), settings);
            _paymentSettingsCommands.Disable(paymentSettingsForSecondVipLevel.Id, "remark");

            //login to admin website, select to display the custom brand
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();

            //make offline deposit and check the relevant payment settings are applied for player
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            _playerManagerPage.SelectPlayer(playerUsername);
            var offlineDepositRequestForm = _playerManagerPage.OpenOfflineDepositRequestForm();

            const decimal depositAmount = 10;
            var submittedOfflineDepositRequest = offlineDepositRequestForm.Submit(depositAmount);
            Assert.AreEqual("Offline deposit request has been created successfully", submittedOfflineDepositRequest.Confirmation);
            submittedOfflineDepositRequest.CloseTab("View Offline Deposit Request");

            //deactivate the default vip level and make the second vip level default
            var vipLevelManagerPage = _playerManagerPage.Menu.ClickVipLevelManagerMenuItem();
            var deactivateVipLevelDialog = vipLevelManagerPage.OpenDeactivateDialog(defaultVipLevel.Name, true);
            vipLevelManagerPage = deactivateVipLevelDialog.Deactivate();

            //change the vip level of the player
            _playerManagerPage = vipLevelManagerPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(playerUsername);
            playerInfoPage.OpenAccountInformationSection();
            //_driver.Manage().Window.Maximize();
            var changeVipLevelDialog = playerInfoPage.OpenChangeVipLevelDialog();
            changeVipLevelDialog.Submit(secondVipLevel.Name);
            playerInfoPage.CloseTab("Player Info");

            //enable payment settings for second vip level
            _paymentSettingsCommands.Enable(paymentSettingsForSecondVipLevel.Id, "remark");

            //try to make deposit again
            _playerManagerPage.SelectPlayer(playerUsername);
            var secondOfflineDepositRequestForm = _playerManagerPage.OpenOfflineDepositRequestForm();
            secondOfflineDepositRequestForm.TryToSubmit(depositAmount);

           //TODO: VladS Payment settings applied but message doesn't show up.
           //Assert.That(offlineDepositRequestForm.ErrorMessage, Is.StringContaining("Deposit failed. The entered amount exceeds the allowed value. Maximum value is 9.00."));
        }

        [Test]
        public void Can_change_payment_level_of_player_and_view_new_payment_level_applied()
        {
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            var playerTestHelper = _container.Resolve<PlayerTestHelper>();
            var playerQueries = _container.Resolve<PlayerQueries>();
            var defaultLicenseeId = brandTestHelper.GetDefaultLicensee();

            //create a brand for a default licensee
            var brand = brandTestHelper.CreateBrand(defaultLicenseeId, null, null, null, true);
            paymentTestHelper.CreatePaymentSettings(brand, PaymentType.Deposit);

            // create a player with a bound bank account for a brand
            var player = playerTestHelper.CreatePlayer(true, brand.Id);
            Thread.Sleep(5000);
            var playerUsername = player.Username;

            //login to admin website, select to display the custom brand
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();

            //make offline deposit and check the relevant payment settings are applied for player
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            _playerManagerPage.SelectPlayer(playerUsername);
            var offlineDepositRequestForm = _playerManagerPage.OpenOfflineDepositRequestForm();
            Thread.Sleep(1000);
            var defaultBankName = offlineDepositRequestForm.Bank.Text;
            Assert.AreNotEqual(defaultBankName, string.Empty);

            offlineDepositRequestForm.CloseTab("Offline Deposit Request");

            //create new payment level with new bankAccount
            var bankAccount = paymentTestHelper.CreateBankAccount(brand.Id, player.CurrencyCode);
            var paymentLevel = paymentTestHelper.CreatePaymentLevel(brand.Id, player.CurrencyCode, bankAccountId: bankAccount.Id);

            //change the payment level of the player
            _playerManagerPage = _playerManagerPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(playerUsername);
            playerInfoPage.OpenAccountInformationSection();
            var changePaymentLevelDialog = playerInfoPage.OpenChangePaymentLevelDialog();
            changePaymentLevelDialog.Submit(paymentLevel.Name, "change payment level");
            Thread.Sleep(1000);

            //assert account details
            var playerAccountInfo = playerInfoPage.GetAccountDetails();
            Assert.AreEqual(paymentLevel.Name, playerAccountInfo.PaymentLevel);
            playerInfoPage.CloseTab("Player Info");

            //open offline deposit form again to confirm the bankaccount is changed
            _playerManagerPage.SelectPlayer(playerUsername);
            offlineDepositRequestForm = _playerManagerPage.OpenOfflineDepositRequestForm();
            Thread.Sleep(1000);
            var newBankName = offlineDepositRequestForm.Bank.Text;

            Assert.AreEqual(newBankName, bankAccount.Bank.BankName + " / " + bankAccount.AccountName);
            Assert.AreNotEqual(defaultBankName, newBankName);

            var submittedOfflineDepositRequest = offlineDepositRequestForm.Submit(10);
            Assert.AreEqual("Offline deposit request has been created successfully", submittedOfflineDepositRequest.Confirmation);
        }

        [Test, Ignore("AFTREGO-4395 Rostyslav 04/05/2016")]
        public void Can_Self_Exclude_account_and_cancel()
        {
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            var playerTestHelper = _container.Resolve<PlayerTestHelper>();
            var defaultLicenseeId = brandTestHelper.GetDefaultLicensee();
            DateTime localDate = DateTime.Now;            

            //create a brand for a default licensee
            var brand = brandTestHelper.CreateBrand(defaultLicenseeId, isActive: true);
            paymentTestHelper.CreatePaymentSettings(brand, PaymentType.Deposit);

            // create a player with a bound bank account for a brand
            var player = playerTestHelper.CreatePlayer(true, brand.Id);
            var playerUsername = player.Username;

            //login to admin website, select to display the custom brand
            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();

            //self exclude account and cancel (duration 6 months)
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            _playerManagerPage.SelectPlayer(playerUsername);
            var playerInfoPage = _playerManagerPage.OpenPlayerInfoPage();
            var responsibleGamblingSection = playerInfoPage.OpenResponsibleGamblingSection();

            var durationEndData6Months = responsibleGamblingSection.SetSelfExcludeData("6 months");            
            var data6Months = localDate.AddMonths(6);
            Assert.IsTrue(durationEndData6Months.Contains(data6Months.ToString("d", CultureInfo.CreateSpecificCulture("ja-JP"))));
            responsibleGamblingSection.CloseTab("Player Info");

            VerifyIsCheckBoxSelfExcludeSelected(responsibleGamblingSection, playerInfoPage, playerUsername);

            UnselectedCheckBoxSelfExcludeAndVerify(responsibleGamblingSection, playerInfoPage);

            //self exclude account and cancel (duration 1 year)
            OpenResponsibleGamblingSectionInPlayerInfoPage(responsibleGamblingSection, playerInfoPage, playerUsername);

            var durationEndData1Year = responsibleGamblingSection.SetSelfExcludeData("1 year");
            var data1Year = localDate.AddYears(1);
            Assert.IsTrue(durationEndData1Year.Contains(data1Year.ToString("d", CultureInfo.CreateSpecificCulture("ja-JP"))));
            responsibleGamblingSection.CloseTab("Player Info");

            VerifyIsCheckBoxSelfExcludeSelected(responsibleGamblingSection, playerInfoPage, playerUsername);

            UnselectedCheckBoxSelfExcludeAndVerify(responsibleGamblingSection, playerInfoPage);

            //self exclude account and cancel (duration 5 years)
            OpenResponsibleGamblingSectionInPlayerInfoPage(responsibleGamblingSection, playerInfoPage, playerUsername);

            var durationEndData5Years = responsibleGamblingSection.SetSelfExcludeData("5 years");
            var data5Years = localDate.AddYears(5);
            Assert.IsTrue(durationEndData5Years.Contains(data5Years.ToString("d", CultureInfo.CreateSpecificCulture("ja-JP"))));
            responsibleGamblingSection.CloseTab("Player Info");

            VerifyIsCheckBoxSelfExcludeSelected(responsibleGamblingSection, playerInfoPage, playerUsername);

            UnselectedCheckBoxSelfExcludeAndVerify(responsibleGamblingSection, playerInfoPage);

            //self exclude account and cancel (duration permanent)
            OpenResponsibleGamblingSectionInPlayerInfoPage(responsibleGamblingSection, playerInfoPage, playerUsername);

            var durationEndDataPermanent = responsibleGamblingSection.SetSelfExcludeData("permanent");
            Assert.IsTrue(durationEndDataPermanent.Contains("10000/01/01"));
            responsibleGamblingSection.CloseTab("Player Info");

            VerifyIsCheckBoxSelfExcludeSelected(responsibleGamblingSection, playerInfoPage, playerUsername);

            UnselectedCheckBoxSelfExcludeAndVerify(responsibleGamblingSection, playerInfoPage);
        }

        public void OpenResponsibleGamblingSectionInPlayerInfoPage(ResponsibleGamblingSection responsibleGamblingSection, PlayerInfoPage playerInfoPage, string playerUsername)
        {
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            _playerManagerPage.SelectPlayer(playerUsername);
            playerInfoPage = _playerManagerPage.OpenPlayerInfoPage();
            responsibleGamblingSection = playerInfoPage.OpenResponsibleGamblingSection();
        }

        public void VerifyIsCheckBoxSelfExcludeSelected(ResponsibleGamblingSection responsibleGamblingSection, PlayerInfoPage playerInfoPage, string playerUsername)
        {
            _playerManagerPage.SelectPlayer(playerUsername);
            _playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenResponsibleGamblingSection();
            Assert.IsTrue(responsibleGamblingSection.IsCheckBoxSelfExcludeSelected());
        }
        public void UnselectedCheckBoxSelfExcludeAndVerify(ResponsibleGamblingSection responsibleGamblingSection, PlayerInfoPage playerInfoPage)
        {
            responsibleGamblingSection.UnSelectedCheckBoxSelfExclude();
            responsibleGamblingSection.CloseTab("Player Info");
            _playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenResponsibleGamblingSection();
            Assert.IsFalse(responsibleGamblingSection.IsCheckBoxSelfExcludeSelected());
            responsibleGamblingSection.CloseTab("Player Info");
        }

        [Test, Ignore("AFTREGO-4396 Rostyslav 04/05/2016")]
        public void Can_Time_Out_account_and_cancel()
        {
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            var playerTestHelper = _container.Resolve<PlayerTestHelper>();
            var defaultLicenseeId = brandTestHelper.GetDefaultLicensee();
            DateTime localDate = DateTime.Now;
            
            //create a brand for a default licensee
            var brand = brandTestHelper.CreateBrand(defaultLicenseeId, isActive: true);
            paymentTestHelper.CreatePaymentSettings(brand, PaymentType.Deposit);

            // create a player with a bound bank account for a brand
            var player = playerTestHelper.CreatePlayer(true, brand.Id);
            var playerUsername = player.Username;

            //login to admin website, select to display the custom brand
            _driver.Navigate().Refresh();
            _dashboardPage.BrandFilter.SelectAll();

            //self exclude account and cancel (duration 24 hours)
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            _playerManagerPage.SelectPlayer(playerUsername);
            var playerInfoPage = _playerManagerPage.OpenPlayerInfoPage();
            var responsibleGamblingSection = playerInfoPage.OpenResponsibleGamblingSection();

            var durationEndData24Hrs = responsibleGamblingSection.SetTimeOutData("24 hrs");
            var data24Hrs = localDate.AddDays(1);
            Assert.IsTrue(durationEndData24Hrs.Contains(data24Hrs.ToString("d", CultureInfo.CreateSpecificCulture("ja-JP"))));            
            responsibleGamblingSection.CloseTab("Player Info");

            VerifyIsCheckBoxTimeOutSelected(responsibleGamblingSection, playerInfoPage, playerUsername);

            UnselectedCheckBoxTimeOutAndVerify(responsibleGamblingSection, playerInfoPage);

            //self exclude account and cancel (duration 1 week)
            OpenResponsibleGamblingSectionInPlayerInfoPage(responsibleGamblingSection, playerInfoPage, playerUsername);

            var durationEndData1Week = responsibleGamblingSection.SetTimeOutData("1 week");
            var data1Week = localDate.AddDays(7);
            Assert.IsTrue(durationEndData1Week.Contains(data1Week.ToString("d", CultureInfo.CreateSpecificCulture("ja-JP"))));
            responsibleGamblingSection.CloseTab("Player Info");

            VerifyIsCheckBoxTimeOutSelected(responsibleGamblingSection, playerInfoPage, playerUsername);

            UnselectedCheckBoxTimeOutAndVerify(responsibleGamblingSection, playerInfoPage);

            //self exclude account and cancel (duration 1 month)
            OpenResponsibleGamblingSectionInPlayerInfoPage(responsibleGamblingSection, playerInfoPage, playerUsername);

            var durationEndData1Month = responsibleGamblingSection.SetTimeOutData("1 month");
            var data1Month = localDate.AddMonths(1);
            Assert.IsTrue(durationEndData1Month.Contains(data1Month.ToString("d", CultureInfo.CreateSpecificCulture("ja-JP"))));
            responsibleGamblingSection.CloseTab("Player Info");

            VerifyIsCheckBoxTimeOutSelected(responsibleGamblingSection, playerInfoPage, playerUsername);

            UnselectedCheckBoxTimeOutAndVerify(responsibleGamblingSection, playerInfoPage);

            //self exclude account and cancel (duration 1 month)
            OpenResponsibleGamblingSectionInPlayerInfoPage(responsibleGamblingSection, playerInfoPage, playerUsername);

            var durationEndData6Weeks = responsibleGamblingSection.SetTimeOutData("6 weeks");
            var data6Weeks = localDate.AddDays(42);
            Assert.IsTrue(durationEndData6Weeks.Contains(data6Weeks.ToString("d", CultureInfo.CreateSpecificCulture("ja-JP"))));
            responsibleGamblingSection.CloseTab("Player Info");

            VerifyIsCheckBoxTimeOutSelected(responsibleGamblingSection, playerInfoPage, playerUsername);

            UnselectedCheckBoxTimeOutAndVerify(responsibleGamblingSection, playerInfoPage);
        }
       

        public void VerifyIsCheckBoxTimeOutSelected(ResponsibleGamblingSection responsibleGamblingSection, PlayerInfoPage playerInfoPage, string playerUsername)
        {
            _playerManagerPage.SelectPlayer(playerUsername);
            _playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenResponsibleGamblingSection();
            Assert.IsTrue(responsibleGamblingSection.IsCheckBoxTimeOutSelected());
        }
        public void UnselectedCheckBoxTimeOutAndVerify(ResponsibleGamblingSection responsibleGamblingSection, PlayerInfoPage playerInfoPage)
        {
            responsibleGamblingSection.UnSelectedCheckBoxTimeOut();
            responsibleGamblingSection.CloseTab("Player Info");
            _playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenResponsibleGamblingSection();
            Assert.IsFalse(responsibleGamblingSection.IsCheckBoxTimeOutSelected());
            responsibleGamblingSection.CloseTab("Player Info");
        }
    }
} 
