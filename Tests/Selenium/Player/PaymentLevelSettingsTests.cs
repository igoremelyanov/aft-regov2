using System;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class PaymentLevelSettingsTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private PaymentTestHelper _paymentTestHelper;
        private PlayerTestHelper _playerTestHelper;
        protected readonly Guid DefaultBrandId = new Guid("00000000-0000-0000-0000-000000000138");
        private const string DefaultCurrency = "CAD";
        private Core.Brand.Interface.Data.Brand _brand;

        public override void BeforeAll()
        {
            base.BeforeAll();
            _paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            var brandTestHelper = _container.Resolve<BrandTestHelper>();

            var defaultLicenseeId = brandTestHelper.GetDefaultLicensee();
            Core.Brand.Interface.Data.Currency curreny = new Core.Brand.Interface.Data.Currency
            {
                Code = DefaultCurrency,
                Name = DefaultCurrency
            };
            //create a brand for a default licensee
            _brand = brandTestHelper.CreateBrand(defaultLicenseeId, null, null, curreny, true);

            //log in 
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
        }
      
        [Test, Ignore("Rostyslav AFTREGO-4629 04/20/2016")]
        public void Can_change_players_payment_level()
        {
            //create 2 players for the brand
            var player1 =_playerTestHelper.CreatePlayer(true, _brand.Id);
            var player2 = _playerTestHelper.CreatePlayer(true, _brand.Id);

            //create payment level for a brand
            var paymentLevelsPage = _dashboardPage.Menu.ClickPaymentLevelsMenuItem();

            var newPaymentLevelForm = paymentLevelsPage.OpenNewPaymentLevelForm();

            var bankAccount = _paymentTestHelper.CreateBankAccount(_brand.Id, DefaultCurrency);

            var paymentLevelCode = TestDataGenerator.GetRandomString();
            var paymentLevelName = paymentLevelCode + "Name";
            var submittedPaymentLevelForm = newPaymentLevelForm.Submit(_brand.Name, paymentLevelCode, paymentLevelName, bankAccount.AccountId, false);

            submittedPaymentLevelForm.CloseTab("View Payment Level");

            //open admin page
            var paymentLevelSettingsPage = _dashboardPage.Menu.ClickPaymentLevelSettingsMenuItem();

            //filte by brand , should list 2 players after filter
            paymentLevelSettingsPage.FilterGrid(_brand.Name);

            //select players
            paymentLevelSettingsPage.SelectPlayer(player1.Username);
            paymentLevelSettingsPage.SelectPlayer(player2.Username);

            //click [set Payment Level] button
            var setPaymentLevelPage =paymentLevelSettingsPage.OpenSetPaymentLevelPage();

            //input data and submit
            setPaymentLevelPage.Submit(paymentLevelName, "change players:" +player1.Username +","+player2.Username+", payment level");

            //confirmed message
            Assert.AreEqual("The payment level has been assigned", setPaymentLevelPage.ConfirmationMessage);

            //check player 1's payment level in player manager 
            var player1PaymentLevel = GetPlayerPaymentLevel(player1.Username);

            Assert.AreEqual(paymentLevelName, player1PaymentLevel);

            //check player 2's payment level in player manager 
            var player2PaymentLevel = GetPlayerPaymentLevel(player2.Username);

            Assert.AreEqual(paymentLevelName, player2PaymentLevel);
        }

        private string GetPlayerPaymentLevel(string username)
        {
            var playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage(username);
            playerInfoPage.OpenAccountInformationSection();
            var playerAccountInfo = playerInfoPage.GetAccountDetails();
            playerInfoPage.CloseTab("Player Info");

            return playerAccountInfo.PaymentLevel;
        }
    }
}
