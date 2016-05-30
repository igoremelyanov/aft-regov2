using System.Threading;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class FakeUGSMockGameWebsiteTests : SeleniumBaseForGameWebsite
    {
        private RegistrationDataForMemberWebsite _playerData;
        private static string _username;
        private const decimal Amount = 10000.25M;
        //private PlayerOverviewPage _playerOverviewPage;
        private GamePage _gamePage;
        private GameListPage _gameListPage;
        private MemberWebsiteLoginPage _memberWebsiteLoginPage;
        private DashboardPage _dashboardPage;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            // register a player on member website
            _playerData = TestDataGenerator.CreateValidPlayerDataForMemberWebsite("RMB");
            _username = _playerData.Username;
            
            _memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _memberWebsiteLoginPage.NavigateToMemberWebsite();
            var registerPage = _memberWebsiteLoginPage.GoToRegisterPage();
            registerPage.Register(_playerData);

            ////make a deposit by TestHelper
            //_container.Resolve<PaymentTestHelper>().MakeDeposit(_username, Amount);

            //Thread.Sleep(15000); // wait for deposit event proceeds for all domains and fake ugs

            ////go to a Mock games
            //_playerOverviewPage = registerPage.GoToOverviewPage();
            //var playerProfilePage1 = _playerOverviewPage.HeaderMenu.OpenMyAccount();
            //_gameListPage = playerProfilePage1.Menu.ClickPlayGamesMenu();

            // make a deposit to the player's account by Admin site
            var adminWebsiteLoginPage = new AdminWebsiteLoginPage(_driver);
            adminWebsiteLoginPage.NavigateToAdminWebsite();
            _dashboardPage = adminWebsiteLoginPage.Login("SuperAdmin", "SuperAdmin");
            _driver.MakeOfflineDeposit(_playerData.Username, Amount, _playerData.FullName);

            Thread.Sleep(15000); // wait for deposit event proceeds for all domains and fake ugs

            // log in as the player to the member website and go to a Mock games
            _memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _memberWebsiteLoginPage.NavigateToMemberWebsite();
            var playerOverviewPage = _memberWebsiteLoginPage.Login(_playerData.Username, _playerData.Password);
            var playerProfilePage = playerOverviewPage.HeaderMenu.OpenMyAccount();
            _gameListPage = playerProfilePage.Menu.ClickPlayGamesMenu();
        }

        [Test]
        public void Can_run_FakeUGS_mockgame_website_and_make_bets()
        {
            _gamePage = _gameListPage.StartGame("Football");

            // check the player balance
            var initialBalance = _gamePage.Balance;
            Assert.AreEqual("Balance: $10,000.25", initialBalance);
            
            var expectedPlayerName = string.Format("Name: {0}", _playerData.Username);
            var playerName = _gamePage.PlayerName;
            Assert.AreEqual(expectedPlayerName, playerName);

            var expectedTag = "Tag: 138";
            Assert.AreEqual(expectedTag, _gamePage.Tag);

            // make a bet
            _gamePage.PlaceInitialBet(100, "description test");
            Assert.AreEqual("-100.00", _gamePage.RoundBetAmount);

            // check a transaction's type and amount
            var txAmount = _gamePage.GetTransactionDetail(0, "amount");
            Assert.AreEqual("-100.00", txAmount);

            var txType = _gamePage.GetTransactionDetail(0, "type");
            Assert.AreEqual("Placed", txType);
            Assert.AreEqual("Balance: $9,900.25", _gamePage.Balance);

            // place another bet
            _gamePage.PlaceSubBet(150, "description test");
            Assert.AreEqual("-250.00", _gamePage.RoundBetAmount);

            // check a transaction's type and amount
            txAmount = _gamePage.GetTransactionDetail(1, "amount");
            Assert.AreEqual("-150.00", txAmount);

            txType = _gamePage.GetTransactionDetail(1, "type");
            Assert.AreEqual("Placed", txType);
            Assert.AreEqual("Balance: $9,750.25", _gamePage.Balance);

            // win a bet
            _gamePage.WinBet(amount:300);
            Assert.AreEqual("50.00", _gamePage.RoundBetAmount);
            Assert.AreEqual("Balance: $10,050.25", _gamePage.Balance);

            // adjust
            //TODO: Not implemented in wallet
            //_gamePage.AdjustTransaction(txNumber:2, amount:400);
            //Assert.AreEqual("550.00", _gamePage.BetAmount);
            //Assert.AreEqual("Balance: $10,550.25", _gamePage.Balance);

            // cancel
            _gamePage.CancelTransaction(txNumber:2, amount:300);
            Assert.AreEqual("-250.00", _gamePage.RoundBetAmount);
            Assert.AreEqual("Balance: $9,750.25", _gamePage.Balance);

            // verify transactions on the player info tab
            var dashboard = _driver.LoginToAdminWebsiteAsSuperAdmin(); 
            var playersList = dashboard.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = playersList.OpenPlayerInfoPage(_username);
            playerInfoPage.OpenTransactionsSection();

            var betPlacedAmounts = playerInfoPage.GetTransactionsMainAmount("Bet placed");

            Assert.Contains(-100m, betPlacedAmounts);
            Assert.Contains(-150m, betPlacedAmounts);
            Assert.AreEqual(300, playerInfoPage.GetTransactionMainAmount("Bet won"));
            //TODO: Not implemented in wallet
            //Assert.AreEqual(400, playerInfoPage.GetTransactionTotalAmount("Bet adjusted"));
            Assert.AreEqual(-300, playerInfoPage.GetTransactionMainAmount("Bet canceled"));
        }
    }
}