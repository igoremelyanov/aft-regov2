using System.Threading;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class EndToEndTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private MemberWebsiteLoginPage _brandWebsiteLoginPage;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private RegistrationDataForMemberWebsite _playerData;

        [Test, CategorySmoke]
        public void Player_can_register_and_play_and_admin_can_track_player_transactions()
        {
            // generate test data
            var userData = TestDataGenerator.CreateValidAdminUserRegistrationData(
               role: "SuperAdmin", status: "Active", licensee: DefaultLicensee, brand: DefaultBrand, currency: "RMB");
            var bonusTemplateName = "reload-bonus-templ" + TestDataGenerator.GetRandomString(4);
            var bonusName = "reload-bonus" + TestDataGenerator.GetRandomString(5);
            var bonusCode = TestDataGenerator.GetRandomString(5);
            _playerData = TestDataGenerator.CreateValidPlayerDataForMemberWebsite("RMB");
            const decimal bonusAmount = 200;
            const decimal depositAmount = 100;

            // register an admin user on an admin website
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            _driver.CreateUserBasedOnPredefinedRole(userData);

            //Not a fix: investigation purposes only
            Thread.Sleep(5000); //wait for new User event proceeds.  

            // log in as SuperAdmin and create a First deposit bonus
            var adminLogin = userData.UserName;
            var adminPassword = userData.Password;
            _dashboardPage = _driver.LoginToAdminWebsiteAs(adminLogin, adminPassword);

            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();
            var submittedBonusTemplateForm = bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(DefaultLicensee, DefaultBrand)
                .SetTemplateName(bonusTemplateName)
                .SelectBonusType(BonusType.FirstDeposit)
                .SelectIssuanceMode(IssuanceMode.AutomaticWithCode)
                .NavigateToRules()
                .SelectCurrency("RMB")
                .EnterBonusTier(bonusAmount)
                .NavigateToSummary()
                .CloseTab();

            var bonusManagerPage = submittedBonusTemplateForm.Menu.ClickBonusMenuItem();
            var newBonusForm = bonusManagerPage.OpenNewBonusForm();
            var submittedBonusForm = newBonusForm.Submit(bonusName, bonusCode, bonusTemplateName, numberOfdaysToClaimBonus: 30);
            submittedBonusForm.CloseTab();
            bonusManagerPage.ActivateBonus(bonusName);
            var activeBonus = bonusManagerPage.CheckIfBonusStatusIsActive(bonusName);

            Assert.IsTrue(activeBonus);

            //Thread.Sleep(5000); //wait for new Bonus event proceeds for all domains.

            // register a player on a brand website
            _brandWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _brandWebsiteLoginPage.NavigateToMemberWebsite();
            var brandWebsiteRegisterPage = _brandWebsiteLoginPage.GoToRegisterPage();
            brandWebsiteRegisterPage.Register(_playerData);
            //var registrationPageStep2 = brandWebsiteRegisterPage.SubmitRgisterForm(_playerData);

            //Assert.AreEqual(true, registrationSuccess);
            //Assert.AreEqual("SUCCESS! YOU ARE NOW REGISTERED.", registrationPageStep2.SuccessMessage);

            // deposit money to the player's account
            var adminWebsiteLoginPage = new AdminWebsiteLoginPage(_driver);
            adminWebsiteLoginPage.NavigateToAdminWebsite();

            _dashboardPage = adminWebsiteLoginPage.Login(adminLogin, adminPassword);
            _driver.MakeOfflineDeposit(_playerData.Username, depositAmount, _playerData.FullName, bonusName);

            Thread.Sleep(15000); // wait for deposit event proceeds for all domains and fake ugs

            // check the player's main and bonus balance on the admin website
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            playerManagerPage.SelectPlayer(_playerData.Username);
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenBalanceInformationSection();

            Assert.AreEqual(depositAmount.ToString(), playerInfoPage.GetMainBalance());
            Assert.AreEqual(bonusAmount.ToString(), playerInfoPage.GetMainBonusBalance());

            // log in as the player to the member website and choose a game
            _brandWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _brandWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = _brandWebsiteLoginPage.Login(_playerData.Username, _playerData.Password);
            var gameListPage = playerProfilePage.Menu.ClickPlayGamesMenu();
            var gamePage = gameListPage.StartGame("Football");

            // check the player balance
            var initialBalance = gamePage.Balance;
            Assert.AreEqual("Balance: $300.00", initialBalance);

            // make a bet and check a transaction's type and amount
            gamePage.PlaceInitialBet(10, "initial game action");
            //Assert.AreEqual("-10.00", gamePage.BetAmount);
            var betTransactionAmount = gamePage.GetTransactionDetail(0, "amount");
            Assert.AreEqual("-10.00", betTransactionAmount);
            Assert.AreEqual("Balance: $290.00", gamePage.Balance);

            // win a bet
            gamePage.WinBet(amount: 20);
            var winTransactionAmount = gamePage.GetTransactionDetail(1, "amount");
            Assert.AreEqual("20.00", winTransactionAmount);
            Assert.AreEqual("Balance: $310.00", gamePage.Balance);

            //lose a bet
            //gamePage.LoseBet(amount: 300);
            //var loseTransactionAmount = gamePage.GetTransactionDetail(2, "amount");
            //Assert.AreEqual("-300.00", loseTransactionAmount);
            //Assert.AreEqual("Balance: $10.00", gamePage.Balance);

            Thread.Sleep(10000); // wait for game events proceed for all domains and fake ugs

            // check the player's transactions on the admin website
            var dashboard = _driver.LoginToAdminWebsiteAsSuperAdmin();
            playerManagerPage = dashboard.Menu.ClickPlayerManagerMenuItem();
            playerInfoPage = playerManagerPage.OpenPlayerInfoPage(_playerData.Username);
            playerInfoPage.OpenTransactionsSection();

            Assert.AreEqual(-10, playerInfoPage.GetTransactionMainAmount("Bet placed"));
            Assert.AreEqual(20, playerInfoPage.GetTransactionMainAmount("Bet won"));
            playerInfoPage.OpenBalanceInformationSection();
            Assert.AreEqual("110", playerInfoPage.GetMainBalance());
            Assert.AreEqual(bonusAmount.ToString(), playerInfoPage.GetMainBonusBalance());
        }
    }
}
