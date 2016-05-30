using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Infrastructure.DataAccess.Player;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using ServiceStack.Common;
using Brand = AFT.RegoV2.Core.Brand.Interface.Data.Brand;

namespace AFT.RegoV2.Tests.Selenium
{
    [Ignore("Failing unstable on RC-1.0 - Igor, 27-Aiprl-2016")]
    internal class BonusTests : SeleniumBaseForAdminWebsite
    {
        private BackendMenuBar _menu;

        private const string LicenseeName = "Flycow";
        private const string BrandName = "138";
        private string _bonusName;
        private string _bonusTemplateName;
        private DashboardPage _dashboardPage;
        private BonusTestHelper _bonusTestHelper;
        private PlayerTestHelper _playerTestHelper;
        private PaymentTestHelper _paymentTestHelper;
        private PlayerQueries _playerQueries;
        private GamesTestHelper _gamesTestHelper;
        private List<string> _activeBonusesNamesList;
        private Brand _brand;

        public override void BeforeAll()
        {
            base.BeforeAll();
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _gamesTestHelper = _container.Resolve<GamesTestHelper>();
            _playerQueries = _container.Resolve<PlayerQueries>();
            _bonusTestHelper = _container.Resolve<BonusTestHelper>();
            _activeBonusesNamesList = new List<string>();
            var brandQueries = _container.Resolve<BrandQueries>();
            _brand = brandQueries.GetBrands().First(x => x.Name == BrandName);
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _menu = _dashboardPage.Menu;
            _paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            _bonusName = TestDataGenerator.GetRandomString(12);
            _bonusTemplateName = TestDataGenerator.GetRandomString(12);
        }

        public override void AfterAll()
        {
            if (!_activeBonusesNamesList.IsEmpty())
            {
                _driver.LoginToAdminWebsiteAsSuperAdmin();
                var bonusManagerPage = _menu.ClickBonusMenuItem();
                foreach (string BonusName in _activeBonusesNamesList)
                {
                    bonusManagerPage.DeactivateBonus(BonusName);
                }
            }

            QuitWebDriver();
        }

        [Test, Ignore("Svitlana: 31/03/2016, Waiting for Igor's Investigation/Updates")]
        public void Can_get_FirstDeposit_bonus_with_code_issuance_mode_via_admin_website()
        {
            const decimal bonusAmount = 10;

            //create a bonus template and a bonus
            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();
            var submittedBonusTemplateForm = bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(LicenseeName, BrandName)
                .SetTemplateName(_bonusTemplateName)
                .SelectBonusType(BonusType.FirstDeposit)
                .SelectIssuanceMode(IssuanceMode.AutomaticWithCode)
                .NavigateToRules()
                .SelectCurrency("RMB")
                .EnterBonusTier(bonusAmount)
                .NavigateToSummary();

            var bonusManagerPage = submittedBonusTemplateForm.Menu.ClickBonusMenuItem();
            var newBonusForm = bonusManagerPage.OpenNewBonusForm();
            var submittedBonusForm = newBonusForm.Submit(_bonusName, TestDataGenerator.GetRandomString(6), _bonusTemplateName, 0);

            Assert.AreEqual("Bonus has been successfully created.",
                submittedBonusForm.ConfirmationMessageAfterBonusSaving);

            submittedBonusForm.SwitchToBonusList();
            ActivateBonus(_bonusName);

            //create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "RMB");
            Thread.Sleep(5000); //wait for Player created event processing

            _driver.MakeOfflineDeposit(player.Username, 100, player.FullName, _bonusName);
            Thread.Sleep(5000); //wait for Deposit created event processing

            //login to admin site
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            //check the bonus
            playerManagerPage.SelectPlayer(player.Username);
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenTransactionsSection();
            var playersBonusAmount = playerInfoPage.GetTransactionBonusAmount("Bonus");
            Assert.AreEqual(bonusAmount, playersBonusAmount);

            //deactivate bonus
            DeactivateBonus();
        }

        //[Test, Ignore("Configuration of bonus is not available for R1")]
        public void Can_redeem_refer_friends_bonus()
        {
            const decimal bonusAmount = 150;
            const decimal wageringCondition = 2;
            const decimal minDepositAmount = 200;
            const decimal actualDepositAmount = 250;
            const decimal requiredWagering = actualDepositAmount * wageringCondition;

            //create a bonus template
            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();
            var submittedBonusTemplateForm = bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(LicenseeName, BrandName)
                .SetTemplateName(_bonusTemplateName)
                .SelectBonusType(BonusType.ReferFriend)
                .SelectIssuanceMode(IssuanceMode.ManualByPlayer)
                .NavigateToRules()
                .SelectCurrency("CAD")
                .EnterReferFriendsConfiguration(minDepositAmount, wageringCondition)
                .EnterBonusTier(bonusAmount, 1, 1)
                .NavigateToSummary();

            var bonusManagerPage = submittedBonusTemplateForm.Menu.ClickBonusMenuItem();
            var newBonusForm = bonusManagerPage.OpenNewBonusForm();

            //create a bonus
            var submittedBonusForm = newBonusForm.Submit(_bonusName, TestDataGenerator.GetRandomString(6), _bonusTemplateName, 0);
            submittedBonusForm.SwitchToBonusList();
            ActivateBonus(_bonusName);

            //create a referrer
            var referrerData = _driver.LoginAsSuperAdminAndCreatePlayer(LicenseeName, BrandName, "CAD");

            //refer a friend
            _driver.Manage().Cookies.DeleteAllCookies();
            var memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = memberWebsiteLoginPage.Login(referrerData.LoginName, referrerData.Password);
            var referFriendPage = playerProfilePage.Menu.ClickReferFriendsMenu();
            referFriendPage.ReferFriend();
            Assert.AreEqual("Phone numbers successfully submitted.", referFriendPage.Message);

            _driver.Manage().Cookies.DeleteAllCookies();

            //register referred
            var referralId = new PlayerRepository().Players.Single(a => a.Username == referrerData.LoginName).ReferralId;
            var referredRegistrationData = TestDataGenerator.CreateValidPlayerDataForMemberWebsite("CAD");
            var registerPage = new RegisterPage(_driver);
            registerPage.NavigateToMemberWebsite(referralId.ToString());
            registerPage.Register(referredRegistrationData);

            //depositing missing funds in order to complete wagering
            _paymentTestHelper.MakeDeposit(referredRegistrationData.Username, requiredWagering - actualDepositAmount);

            //make deposit for referred
            _driver.MakeOfflineDeposit(referredRegistrationData.Username, actualDepositAmount,
                referredRegistrationData.FullName);
            _driver.Manage().Cookies.DeleteAllCookies();

            //complete wagering requirements for referred
            memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().Refresh();

            playerProfilePage = memberWebsiteLoginPage.Login(referredRegistrationData.Username,
                referredRegistrationData.Password);
            var gameListPage = playerProfilePage.Menu.ClickPlayGamesMenu();
            var gamePage = gameListPage.StartGame("Poker");
            gamePage.PlaceInitialBet(requiredWagering, "");
            gamePage.WinBet(requiredWagering);
            _driver.Manage().Cookies.DeleteAllCookies();

            //claim refer a friend bonus reward
            memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();

            playerProfilePage = memberWebsiteLoginPage.Login(referrerData.LoginName, referrerData.Password);
            var claimBonusPage = playerProfilePage.Menu.ClickClaimBonusSubMenu();
            claimBonusPage.ClaimBonus();

            Assert.AreEqual("Redemption claimed successfully.", claimBonusPage.MessageValue);

            //login to admin site
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            //check the bonus
            playerManagerPage.SelectPlayer(referrerData.LoginName);
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenTransactionsSection();
            var playersBonusAmount = playerInfoPage.GetTransactionBonusAmount("Bonus");
            Assert.AreEqual(bonusAmount, playersBonusAmount);

            //deactivate bonus
            DeactivateBonus();
        }

        //[Test, Ignore("Configuration of bonus is not available for R1")]
        public void Can_receive_high_deposit_bonus_reward()
        {
            const decimal bonusAmount = 100;
            const decimal depositAmount = 300;
            const decimal requiredDeposit = 250;

            //create a bonus template and a bonus
            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();
            var submittedBonusTemplateForm = bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(LicenseeName, BrandName)
                .SetTemplateName(_bonusTemplateName)
                .SelectBonusType(BonusType.HighDeposit)
                .NavigateToRules()
                .SelectCurrency("CAD")
                .EnterHighDepositConfiguration(requiredDeposit, bonusAmount)
                .LimitMaxTotalBonusAmount(bonusAmount)
                .NavigateToSummary();

            var bonusManagerPage = submittedBonusTemplateForm.Menu.ClickBonusMenuItem();
            var newBonusForm = bonusManagerPage.OpenNewBonusForm();
            var submittedBonusForm = newBonusForm.Submit(_bonusName, TestDataGenerator.GetRandomString(6), _bonusTemplateName, 0);

            submittedBonusForm.SwitchToBonusList();
            ActivateBonus(_bonusName);

            var playerData = _driver.LoginAsSuperAdminAndCreatePlayer(LicenseeName, BrandName, "CAD");

            _driver.MakeOfflineDeposit(playerData.LoginName, depositAmount, playerData.FullName);

            //login to admin site
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            //check the bonus
            playerManagerPage.SelectPlayer(playerData.LoginName);
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenTransactionsSection();
            var playersBonusAmount = playerInfoPage.GetTransactionBonusAmount("Bonus");
            Assert.AreEqual(bonusAmount, playersBonusAmount);

            //deactivate bonus
            DeactivateBonus();
        }

        //[Test, Ignore("Configuration of bonus is not available for R1")]
        public void Can_redeem_mobile_plus_email_verification_bonus()
        {
            const decimal bonusAmount = 10;
            const string brandName = "831"; //Brand with Email activation method

            //Create a bonus template and a bonus
            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();
            var submittedBonusTemplateForm = bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(LicenseeName, brandName)
                .SetTemplateName(_bonusTemplateName)
                .SelectBonusType(BonusType.MobilePlusEmailVerification)
                .NavigateToRules()
                .SelectCurrency("CAD")
                .EnterBonusTier(bonusAmount)
                .NavigateToSummary();

            var bonusManagerPage = submittedBonusTemplateForm.Menu.ClickBonusMenuItem();
            var newBonusForm = bonusManagerPage.OpenNewBonusForm();
            var submittedBonusForm = newBonusForm.Submit(_bonusName, TestDataGenerator.GetRandomString(6), _bonusTemplateName, 0);
            submittedBonusForm.SwitchToBonusList();
            ActivateBonus(_bonusName);

            var playerData = _driver.LoginAsSuperAdminAndCreatePlayer(LicenseeName, brandName, "CAD", "en-US", "CA", false);
            Thread.Sleep(5000); //wait for Player created event processing

            //Verify email address
            var emailVerificationToken =
                new PlayerRepository().Players.Single(p => p.Username == playerData.LoginName)
                    .AccountActivationEmailToken;
            _driver.Manage().Cookies.DeleteAllCookies();
            var memberWebsiteAccountActivationPage = new AccountActivationPage(_driver, emailVerificationToken);
            memberWebsiteAccountActivationPage.NavigateTo();

            //Verify mobile phone number
            var memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = memberWebsiteLoginPage.Login(playerData.LoginName, playerData.Password);
            playerProfilePage.RequestMobileVerificationCode();
            var mobileVerificationCode =
                new PlayerRepository().Players.Single(p => p.Username == playerData.LoginName).MobileVerificationCode;
            playerProfilePage.VerifyMobileNumber(mobileVerificationCode);

            //login to admin site
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            //check the bonus
            playerManagerPage.SelectPlayer(playerData.LoginName);
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenTransactionsSection();
            var playersBonusAmount = playerInfoPage.GetTransactionBonusAmount("Bonus");
            Assert.AreEqual(bonusAmount, playersBonusAmount);

            //deactivate bonus
            DeactivateBonus();
        }

        //[Test, Ignore("Configuration of bonus is not available for R1")]
        public void Can_redeem_fund_in_bonus_with_code_issuance_mode()
        {
            const decimal fundInFrom = 100;
            const decimal amount = 200;
            const decimal fundInPercentage = 75;
            const decimal maxTierReward = 100;
            const string walletName = "Product 138";

            //Create a bonus template and a bonus
            var bonusTemplateManagerPage = _dashboardPage.Menu.ClickBonusTemplateMenuItem();
            var submittedBonusTemplateForm = bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(LicenseeName, BrandName)
                .SetTemplateName(_bonusTemplateName)
                .SelectBonusType(BonusType.HighDeposit)
                //temporary commented out 'cos selection is index based
                //.SelectBonusType(BonusUIType.FundIn)
                .SelectIssuanceMode(IssuanceMode.AutomaticWithCode)
                .NavigateToRules()
                .SelectRewardType(BonusRewardType.TieredPercentage)
                .SelectCurrency("CAD")
                .SelectFundInWallet(walletName)
                .EnterBonusTier(fundInPercentage, fromAmount: fundInFrom, maxTierReward: maxTierReward)
                .NavigateToSummary();

            var bonusManagerPage = submittedBonusTemplateForm.Menu.ClickBonusMenuItem();
            var newBonusForm = bonusManagerPage.OpenNewBonusForm();
            var bonusCode = TestDataGenerator.GetRandomString(6);
            var submittedBonusForm = newBonusForm.Submit(_bonusName, bonusCode, _bonusTemplateName, 0);
            submittedBonusForm.SwitchToBonusList();
            ActivateBonus(_bonusName);

            //create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            Thread.Sleep(5000); //wait for Player created event processing

            //Make a deposit to have funds for fund in
            _paymentTestHelper.MakeDeposit(player.Username, amount);

            Thread.Sleep(5000); //wait for Deposit created event processing
            var playerId = _playerQueries.GetPlayerByUsername(player.Username).Id;
            //Wait for record in DB
            WaitForOfflineDeposit(playerId, amount: 200, timeout: TimeSpan.FromSeconds(20));

            //login to member site
            var memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = memberWebsiteLoginPage.Login(player.Username, player.Password);

            // make fundIn 
            var fundInSection = playerProfilePage.Menu.ClickTransferFundSubMenu();
            fundInSection.FundIn(amount, bonusCode);
            Thread.Sleep(5000); //wait for Fundin transaction created event processing

            //login to admin site
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            playerManagerPage.SelectPlayer(player.Username);
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenTransactionsSection();
            var playersBonusAmount = playerInfoPage.GetTransactionBonusAmount("Bonus");
            Assert.AreEqual(maxTierReward, playersBonusAmount);

            //deactivate bonus
            DeactivateBonus();
        }

        [Test]
        public async Task Can_get_FirstDeposit_bonus_with_automatic_issuance_mode_via_member_website()
        {
            //create a bonus template and a bonus - Bonus type:First, Reward Type: Fixed, Issuance:Automatic
            var walletTemplateId = _brand.WalletTemplates.First().Id;
            var info = new CreateUpdateTemplateInfo
            {
                Name = TestDataGenerator.GetRandomString(),
                TemplateType = BonusType.FirstDeposit,
                BrandId = _brand.Id,
                WalletTemplateId = walletTemplateId,
                Mode = IssuanceMode.Automatic
            };
            var rules = new CreateUpdateTemplateRules
            {
                RewardTiers = new List<CreateUpdateRewardTier>
                {
                    new CreateUpdateRewardTier
                    {
                        CurrencyCode = "CAD",
                        BonusTiers = new List<CreateUpdateTemplateTier> {new CreateUpdateTemplateTier {Reward = 19}}
                    }
                }
            };

            var bonusTemplate = await _bonusTestHelper.CreateTemplate(info: info, rules: rules);
            var bonus = await _bonusTestHelper.CreateBonus(bonusTemplate);
            _bonusName = bonus.Name;
            if (bonus.IsActive) { _activeBonusesNamesList.Add(_bonusName); }

            //create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");

            //login to member site
            var memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = memberWebsiteLoginPage.Login(player.Username, player.Password);
            
            CashierPage cashierPage = playerProfilePage.Menu.OpenCashierPage();
            var offlineDepositRequestPage = cashierPage.OpenOfflineDepositPage();
            offlineDepositRequestPage.Submit(amount: "113", playerRemark: "my deposit");
            Assert.AreEqual("Congratulation on your deposit!", offlineDepositRequestPage.GetConfirmationMessage());
            var playerId = _playerQueries.GetPlayerByUsername(player.Username).Id;
            //Wait for record in DB
            WaitForOfflineDeposit(playerId, amount: 113, timeout: TimeSpan.FromSeconds(20));
            var firstdeposit = GetLastDepositByPlayerId(playerId);
            _paymentTestHelper.ConfirmOfflineDeposit(firstdeposit);
            _paymentTestHelper.VerifyOfflineDeposit(firstdeposit, true);
            _paymentTestHelper.ApproveOfflineDeposit(firstdeposit, true);

            //re-login to member site
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            _driver.Logout();

            //make sure that we've got a bonus
            playerProfilePage = memberWebsiteLoginPage.Login(player.Username, player.Password);
             playerProfilePage.Menu.OpenCashierPage();
            Assert.AreEqual("19", cashierPage.GetBonusBalance());

            //deactivate bonus
            DeactivateBonus();
        }

        [Test]
        [Ignore("Until Pavel's fixes for Claim Bonuses on Member site AFTREGO-3806 - 10-Feb-2016, Igor")]
        public async Task Can_get_ReloadDeposit_bonus_with_manual_issuance_mode()
        {
            //create a bonus template and a bonus - Bonus type:Reload, Reward Type: Fixed
            var walletTemplateId = _brand.WalletTemplates.First().Id;
            var info = new CreateUpdateTemplateInfo
            {
                Name = TestDataGenerator.GetRandomString(),
                TemplateType = BonusType.ReloadDeposit,
                BrandId = _brand.Id,
                WalletTemplateId = walletTemplateId,
                Mode = IssuanceMode.ManualByPlayer
            };
            var rules = new CreateUpdateTemplateRules
            {
                RewardTiers = new List<CreateUpdateRewardTier>
                {
                    new CreateUpdateRewardTier
                    {
                        CurrencyCode = "CAD",
                        BonusTiers = new List<CreateUpdateTemplateTier> {new CreateUpdateTemplateTier {Reward = 15}}
                    }
                }
            };

            var bonusTemplate = await _bonusTestHelper.CreateTemplate(info: info, rules: rules);
            var bonus = await _bonusTestHelper.CreateBonus(bonusTemplate);
            _bonusName = bonus.Name;
            if (bonus.IsActive) { _activeBonusesNamesList.Add(_bonusName); }

            //create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            var playerId = _playerQueries.GetPlayerByUsername(player.Username).Id;

            //make first deposit
            _paymentTestHelper.MakeDepositSelenium(playerId, 117);

            //make deposit again - to get reload bonus
            _driver.MakeOfflineDeposit(player.Username, 115, player.FullName, _bonusName);

            //login to member site
            var memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = memberWebsiteLoginPage.Login(player.Username, player.Password);
            await WaitForBonusRedemption(playerId, player.Username, TimeSpan.FromSeconds(20));

            //TODO: Until Pavel's fixes for Claim Bonuses on Member site AFTREGO-3806
            //can see Claim button
            var claimBonusPage = playerProfilePage.Menu.OpenClaimBonusPage();
            Assert.True(claimBonusPage.ClaimButton.Displayed);

            //claim the bonus
            claimBonusPage.ClaimBonus();
            Assert.AreEqual("Redemption claimed successfully.", claimBonusPage.MessageValue);

            //go to balance page
            var balanceDetailsPage = playerProfilePage.Menu.ClickBalanceInformationMenu();
            Assert.AreEqual("15.00", balanceDetailsPage.GetBonusBalance(walletTemplateId.ToString()));

            //deactivate bonus
            DeactivateBonus();
        }

        [Test]
        [Ignore("Till Igor's fixes - 16-Feb-2016, Igor")]
        public async Task Can_get_FirstDeposit_bonus_with_code_issuance_mode_via_member_website()
        {
            //create a bonus template and a bonus - Bonus type:First, Reward Type: Fixed, Issuance:AutomaticWithCode
            var walletTemplateId = _brand.WalletTemplates.First().Id;
            var info = new CreateUpdateTemplateInfo
            {
                Name = TestDataGenerator.GetRandomString(),
                TemplateType = BonusType.FirstDeposit,
                BrandId = _brand.Id,
                WalletTemplateId = walletTemplateId,
                Mode = IssuanceMode.AutomaticWithCode
            };
            var rules = new CreateUpdateTemplateRules
            {
                RewardTiers = new List<CreateUpdateRewardTier>
                {
                    new CreateUpdateRewardTier
                    {
                        CurrencyCode = "CAD",
                        BonusTiers = new List<CreateUpdateTemplateTier> {new CreateUpdateTemplateTier {Reward = 21}}
                    }
                }
            };

            var bonusTemplate = await _bonusTestHelper.CreateTemplate(info: info, rules: rules);
            var bonus = await _bonusTestHelper.CreateBonus(bonusTemplate);
            _bonusName = bonus.Name;
            if (bonus.IsActive) { _activeBonusesNamesList.Add(_bonusName); }

            //create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            Thread.Sleep(5000); //wait for Player created event processing

            //log in to brand website
            var brandWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            brandWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = brandWebsiteLoginPage.Login(player.Username, player.Password);
            var offlineDepositRequestPage = playerProfilePage.Menu.ClickOfflineDepositSubmenu();

            //create Deposit via member site
            offlineDepositRequestPage.Submit(amount: "112", playerRemark: "my deposit", bonusCode: bonus.Code);
            Assert.AreEqual("Offline deposit requested successfully.", offlineDepositRequestPage.ConfirmationMessage);
            Thread.Sleep(5000); //wait for Deposit created event processing
            var playerId = _playerQueries.GetPlayerByUsername(player.Username).Id;
            //Wait for record in DB
            WaitForOfflineDeposit(playerId, amount: 112, timeout: TimeSpan.FromSeconds(20));
            var deposit = GetLastDepositByPlayerId(playerId);
            _paymentTestHelper.ConfirmOfflineDeposit(deposit);
            _paymentTestHelper.VerifyOfflineDeposit(deposit, true);
            _paymentTestHelper.ApproveOfflineDeposit(deposit, true);
            Thread.Sleep(5000); //wait for deposit Approval event processing

            //log in to brand website
            brandWebsiteLoginPage.NavigateToMemberWebsite();
            _driver.Logout();
            playerProfilePage = brandWebsiteLoginPage.Login(player.Username, player.Password);
            var balanceDetailsPage = playerProfilePage.Menu.ClickBalanceInformationMenu();
            Assert.AreEqual("21.00", balanceDetailsPage.GetBonusBalance(walletTemplateId.ToString()));

            //deactivate bonus
            DeactivateBonus();
        }

        [Test]
        [Ignore("Until Pavel's fixes for Balance Information on Member site AFTREGO-???? - 10-Feb-2016, Igor")]
        public async Task Can_set_games_for_bonus_to_contribute_players_completion_of_wagering_requirement()
        {
            const decimal amount = 1000;

            //create a bonus - First Deposit, Automatic, Withdrawable, for main wallet
            var brand = _container.Resolve<IBrandRepository>().Brands.Include(x => x.WalletTemplates).Single(b => b.Name == BrandName);
            var walletTemplateId = brand.WalletTemplates.First(x=>x.IsMain).Id;

            var info = new CreateUpdateTemplateInfo
            {
                Name = TestDataGenerator.GetRandomString(),
                TemplateType = BonusType.FirstDeposit,
                BrandId = brand.Id,
                WalletTemplateId = walletTemplateId,
                IsWithdrawable = true,
                Mode = IssuanceMode.Automatic
            };
            var rules = new CreateUpdateTemplateRules
            {
                RewardTiers = new List<CreateUpdateRewardTier>
                {
                    new CreateUpdateRewardTier
                    {
                        CurrencyCode = "CAD",
                        BonusTiers = new List<CreateUpdateTemplateTier> {new CreateUpdateTemplateTier {Reward = 13}}
                    }
                }
            };
            var gameRepository = _container.Resolve<IGameRepository>();
            var wagering = new CreateUpdateTemplateWagering
            {
                HasWagering = true,
                Method = WageringMethod.Bonus,
                Multiplier = 1,
                GameContributions = new List<CreateUpdateGameContribution>
                {
                    new CreateUpdateGameContribution
                    {
                        GameId = gameRepository.Games.Single(g => g.Name == "Football").Id,
                        Contribution = 50
                    },
                    new CreateUpdateGameContribution
                    {
                        GameId = gameRepository.Games.Single(g => g.Name == "Hockey").Id,
                        Contribution = 100
                    }
                }
            };

            var bonusTemplate = await _bonusTestHelper.CreateTemplate(info: info, rules: rules, wagering: wagering);
            var bonus = await _bonusTestHelper.CreateBonus(bonusTemplate);
            _bonusName = bonus.Name;
            if (bonus.IsActive) { _activeBonusesNamesList.Add(_bonusName); }

            //create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            //Thread.Sleep(5000); //wait for Player created event processing

            //make a Deposit
            _paymentTestHelper.MakeDeposit(player.Username, amount);
            Thread.Sleep(5000); //wait for Deposit created event processing

            //place and win first bet  - must deduct only 50% wagering contribution for now
            var playerId = _playerQueries.GetPlayerByUsername(player.Username).Id;
            var gameId = "FOOTBALL";
            var gameProviderCode = _gamesTestHelper.GetGameProviderCodeByGameExternalId(gameId);
            var placeBetTxId = Guid.NewGuid().ToString();
            var actualBetId = await _gamesTestHelper.PlaceBet(13, playerId, gameProviderCode, gameId, transactionId: placeBetTxId);
            //TODO: VladK.  - remove Sleep here 
            Thread.Sleep(5000);// wait for Bet event processing
            await _gamesTestHelper.WinBet(actualBetId, 13, placeBetTxId, gameProviderCode);
            //TODO: VladK.  - remove Sleep here 
            Thread.Sleep(5000); //wait for Bet event processing

            //make sure that bonus haven't been unlocked yet
            var memberWebsiteLoginPage = new MemberWebsiteLoginPage(_driver);
            memberWebsiteLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = memberWebsiteLoginPage.Login(player.Username, player.Password);
            var playerBalanceInformationPage = playerProfilePage.Menu.ClickBalanceInformationMenu();
            //TODO: Until Pavel's fixes for Balance Information on Member site AFTREGO-???? - 10-Feb-2016, Igor
            Assert.AreEqual("987.00", playerBalanceInformationPage.GetMainBalance(walletTemplateId.ToString()));
            Assert.AreEqual("26.00", playerBalanceInformationPage.GetBonusBalance(walletTemplateId.ToString()));

            //place and win second bet - must deduct 50%+50% = 100% wagering contribution
            var secondPlaceBetTxId = Guid.NewGuid().ToString();
            actualBetId = await _gamesTestHelper.PlaceBet(13, playerId, gameProviderCode, gameId, transactionId:secondPlaceBetTxId);
            //TODO: VladK.  - remove Sleep here 
            Thread.Sleep(5000);// wait for Bet event processing
            await _gamesTestHelper.WinBet(actualBetId, 13, secondPlaceBetTxId, gameProviderCode);
            //TODO: VladK.  - remove Sleep here 
            Thread.Sleep(5000); //wait for Bet event processing

            //make sure that we ve got bonus on Main Balance
            playerBalanceInformationPage = playerProfilePage.Menu.ClickBalanceInformationMenu();
            Assert.AreEqual("1013.00", playerBalanceInformationPage.GetMainBalance(walletTemplateId.ToString()));
            Assert.AreEqual("0.00", playerBalanceInformationPage.GetBonusBalance(walletTemplateId.ToString()));

            //deactivate bonus
            DeactivateBonus();
        }

        [Test, Ignore("Svitlana: 31/03/2016, Waiting for Igor's Investigation/Updates")]
        public async Task Can_not_manually_issue_not_qualified_deposit_bonus_as_a_CS()
        {
          //Can_manually_clime_and_redeem_deposit_bonus_via_player_info_site_as_a_CS
            //create a bonus template and a bonus - Bonus type:Reload, Reward Type: Fixed
            var walletTemplateId = _brand.WalletTemplates.First().Id;
            var info = new CreateUpdateTemplateInfo
            {
                Name = TestDataGenerator.GetRandomString(),
                TemplateType = BonusType.ReloadDeposit,
                BrandId = _brand.Id,
                WalletTemplateId = walletTemplateId,
                Mode = IssuanceMode.Automatic
            };
            var rules = new CreateUpdateTemplateRules
            {
                RewardTiers = new List<CreateUpdateRewardTier>
                {
                    new CreateUpdateRewardTier
                    {
                        CurrencyCode = "CAD",
                        BonusTiers = new List<CreateUpdateTemplateTier> {new CreateUpdateTemplateTier {Reward = 14}}
                    }
                }
            };

            var bonusTemplate = await _bonusTestHelper.CreateTemplate(info: info, rules: rules);
            var bonus = await _bonusTestHelper.CreateBonus(bonusTemplate);
            _bonusName = bonus.Name;
            if (bonus.IsActive) { _activeBonusesNamesList.Add(_bonusName); }

            //create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            //Thread.Sleep(5000); //wait for Player created event processing

            //make a first Deposit
            _paymentTestHelper.MakeDeposit(player.Username, 217);
            //Thread.Sleep(5000); //wait for Deposit created event processing

            //make a reload Deposit
            _paymentTestHelper.MakeDeposit(player.Username, 213);
            //Thread.Sleep(5000); //wait for Deposit created event processing

            //login to admin site
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            //open Player Info -> Bonus section
            playerManagerPage.SelectPlayer(player.Username);
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenBonusSection();

            //check the Bonus Qualification Record in grid
            playerInfoPage.FindAndSelectBonusQualificationRecord(_bonusName, "Reload deposit");
            //Assert.AreEqual(_bonusName, playerInfoPage.GetBonusQualificationName(_bonusName, "Reload deposit"));

            //check qualified bonus transaction for the Player
            playerInfoPage.OpenBonusToIssueSection();
            Assert.AreEqual(_bonusName, playerInfoPage.GetBonusQualificationName());
            Assert.AreEqual("CAD14", playerInfoPage.GetBonusQualificationAmount());

            //issue Bonus for transaction
            playerInfoPage.IssuedBonusForQualifiedTransaction();
            Assert.AreEqual("Bonus issued successfully.", playerInfoPage.GetConformationMessage());

            //check the bounus transaction
            playerInfoPage.OpenTransactionsSection();
            Assert.AreEqual(14, playerInfoPage.GetTransactionMainAmount("Bonus"));

            //check the Balance
            playerInfoPage.OpenBalanceInformationSection();
            Assert.AreEqual("430", playerInfoPage.GetMainBalance());
            Assert.AreEqual("28", playerInfoPage.GetTotalBonusBalance());

            
            //Can_not_manually_clime_deposit_bonus_for_no_qualified_deposit_via_player_info_site_as_a_CS

            //create a new Player
            var player2 = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            //Thread.Sleep(5000); //wait for Player created event processing
            
            //relogin to admin site
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var player2ManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            // try to redeem not qualified bonus for new player

            //open Player Info -> Bonus section
            player2ManagerPage.SelectPlayer(player2.Username);
            var player2InfoPage = player2ManagerPage.OpenPlayerInfoPage();
            player2InfoPage.OpenBonusSection();

            //check the Bonus Qualification Record in grid
            player2InfoPage.FindAndSelectBonusQualificationRecord(_bonusName, "Reload deposit");

            //check qualified bonus transaction for the Player
            player2InfoPage.OpenBonusToIssueSection();
            Assert.AreEqual(_bonusName, player2InfoPage.GetBonusQualificationName());
            Assert.AreEqual("No qualified transactions found.", player2InfoPage.GetBonusQualificationMessage());

            //deactivate bonus
            DeactivateBonus();
        }

        private void DeactivateBonus()
        {
            _driver.LoginToAdminWebsiteAsSuperAdmin();
            var bonusManagerPage = _menu.ClickBonusMenuItem();
            bonusManagerPage.DeactivateBonus(_bonusName);
            _activeBonusesNamesList.Remove(_bonusName);
        }

        private void ActivateBonus(string bonusName)
        {
            _driver.LoginToAdminWebsiteAsSuperAdmin();
            var bonusManagerPage = _menu.ClickBonusMenuItem();
            bonusManagerPage.ActivateBonus(bonusName);
            _activeBonusesNamesList.Add(bonusName);
        }

        private void WaitForOfflineDeposit(Guid playerId, decimal amount, TimeSpan timeout)
        {
            Func<IEnumerable<OfflineDeposit>> deposits = () =>
                    _container.Resolve<IPaymentRepository>()
                        .OfflineDeposits.Where(r => r.Amount == amount && r.PlayerId == playerId);

            var stopwatch = Stopwatch.StartNew();
            while (deposits().Any() == false && stopwatch.Elapsed < timeout)
            {
                Thread.Sleep(500);
            }
            if (deposits().Any() == false)
            {
                throw new RegoException(string.Format("Deposit is not in DB for Player {0} after {1} seconds.", playerId, stopwatch.Elapsed));
            }
        }
        private async Task WaitForBonusRedemption(Guid playerId, string username, TimeSpan timeout)
        {
            Func<Task<bool>> anyBonusRedemptions = async () =>
                    (await _container.Resolve<IBonusApiProxy>().GetClaimableRedemptionsAsync(playerId)).Any(
                        r => r.Bonus.Name == _bonusName);

            var stopwatch = Stopwatch.StartNew();
            while ((await anyBonusRedemptions() == false) && (stopwatch.Elapsed < timeout))
            {
                Thread.Sleep(500);
            }
            if (await anyBonusRedemptions() == false)
            {
                throw new RegoException(string.Format("Redemption (for bonus {0}, for player {1}) is not in DB after {2} seconds.", _bonusName, username, stopwatch.Elapsed));
            }
        }
        private OfflineDeposit GetLastDepositByPlayerId(Guid playerId)
        {
            var offlineDeposit = _container.Resolve<IPaymentRepository>()
                .OfflineDeposits
                .Include(x => x.Brand)
                .Include(x => x.Player)
                .Include(x => x.BankAccount.Bank)
                .OrderByDescending(x => x.Created)
                .FirstOrDefault(x => x.Player.Id == playerId);
            if (offlineDeposit == null)
            {
                throw new RegoException(string.Format("OfflineDeposit for player's Id {0} was not found", playerId));
            }
            return offlineDeposit;
        }
    }
}