using System;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using ServiceStack.Common;

namespace AFT.RegoV2.Tests.Selenium
{
    [Ignore("Failing unstable on RC-1.0 - Igor, 27-Aiprl-2016")]
    class BalanceTests : SeleniumBaseForAdminWebsite
    {
        private RegistrationDataForMemberWebsite _playerData;
        private static string _username;
        private static string _password;
        private const decimal Amount = 100.25M;
        private PlayerProfilePage _playerProfilePage;
        private GamePage _gamePage;
        private GameListPage _gameListPage;
        private MemberWebsiteLoginPage _memberWebsiteLoginPage;
        private PlayerTestHelper _playerTestHelper;
        private DashboardPage _dashboardPage ;
        private BonusTestHelper _bonusTestHelper;
        private const string DefaultBrand = "138";
        private string _activeBonusesName;
        private BackendMenuBar _menu;

        public override void BeforeAll()
        {
            base.BeforeAll();
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _bonusTestHelper = _container.Resolve<BonusTestHelper>();
        }
        
        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _menu = _dashboardPage.Menu;
        }

        public override void AfterAll()
        {
            if (!_activeBonusesName.IsNullOrEmpty())
            {
                _driver.LoginToAdminWebsiteAsSuperAdmin();
                var bonusManagerPage = _menu.ClickBonusMenuItem();
                bonusManagerPage.DeactivateBonus(_activeBonusesName);
            }

            QuitWebDriver();
        }

        [Test]
        [Ignore("Failing unstable on RC-1.0 - Igor, 25-Aiprl-2016")]
        public void Can_make_offline_deposit_and_view_updated_balance_on_game_page()
        {
            // register a player on member website
            _playerData = _container.Resolve<PlayerTestHelper>().CreatePlayerForMemberWebsite("CAD");
            _username = _playerData.Username;
            _password = _playerData.Password;
            
            //check empty balance of the player
            _memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _memberWebsiteLoginPage.NavigateToMemberWebsite();
            _playerProfilePage = _memberWebsiteLoginPage.Login(_username, _password);

            _gameListPage = _playerProfilePage.Menu.ClickPlayGamesMenu();
            _gamePage = _gameListPage.StartGame("Football");
            var initialBalance = _gamePage.Balance;
            Assert.AreEqual("Balance: $0.00", initialBalance);
            
           
            var playerName = _gamePage.PlayerName;
            var expectedPlayerLoginName = String.Format("Name: {0}", _playerData.Username);
            Assert.AreEqual(expectedPlayerLoginName, playerName);

            // login to admin website and make an offline deposit request
            _driver.Manage().Cookies.DeleteAllCookies();
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _driver.MakeOfflineDeposit(_username, Amount, _playerData.FullName);

            var playerManagerPage = dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(_username);
            playerInfoPage.OpenTransactionsSection();
            
            Assert.AreEqual(Amount, playerInfoPage.GetTransactionMainAmount("Deposit"));
            
            // check the balance on the member website
            _memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            _memberWebsiteLoginPage.NavigateToMemberWebsite();
            
            _playerProfilePage = _memberWebsiteLoginPage.Login(_username, _password);
            var gameListPage = _playerProfilePage.Menu.ClickPlayGamesMenu();
            _gamePage = gameListPage.StartGame("Football");
            var currentBalance = _gamePage.Balance;
            
            Assert.AreEqual("Balance: $100.25", currentBalance);
        }

        [Test]
        public void Can_make_offline_deposit_and_view_main_balance_on_player_info()
        {
            const decimal depositAmount = 300.25M;

            // create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");

            // make offline deposit for the player
            var paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            paymentTestHelper.MakeDeposit(player.Username, depositAmount, waitForProcessing:true);

            // check the player's balance
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(player.Username);
            playerInfoPage.OpenBalanceInformationSection();

            Assert.AreEqual(depositAmount.ToString(), playerInfoPage.GetMainBalance());
        }

        [Test]
        public async Task Can_redeem_bonus_and_view_bonus_balance_on_player_info()
        {
            const decimal depositAmount = 300.25M;

            // create a bonus
            var bonusTemplate = await _bonusTestHelper.CreateFirstDepositTemplateAsync(DefaultBrand, IssuanceMode.AutomaticWithCode);
            var bonus = await _bonusTestHelper.CreateBonus(bonusTemplate);
            _activeBonusesName = bonus.Name;
            // create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            var playerQueries = _container.Resolve<PlayerQueries>();
            var playerId = playerQueries.GetPlayerByUsername(player.Username).Id;
            Thread.Sleep(5000); //wait for Player created event processing

            // make offline deposit and claim the bonus
            var paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            paymentTestHelper.MakeDepositSelenium(playerId, depositAmount, bonus.Code);

            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(player.Username);
            playerInfoPage.OpenBalanceInformationSection();

            Assert.AreEqual("27", playerInfoPage.GetTotalBonusBalance());
        }
    }
}