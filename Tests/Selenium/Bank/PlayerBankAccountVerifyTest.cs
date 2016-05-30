using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;
using Microsoft.Practices.Unity;


namespace AFT.RegoV2.Tests.Selenium.Bank
{
    [Ignore("Failing unstable on RC-1.0 - Igor, 27-Aiprl-2016")]
    class PlayerBankAccountVerifyTest : SeleniumBaseForAdminWebsite
    {       
        private DashboardPage _dashboardPage;        
        private PlayerBankAccountVerifyPage _playerBankAccountVerifyPage;
        private PlayerTestHelper _playerTestHelper;        
        private PaymentTestHelper _paymentTestHelper;
        private PlayerBankAccount _playerBankAccount;
        private Core.Common.Data.Player.Player _player;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _driver.Logout();          
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            _player = _playerTestHelper.CreatePlayer();
            _playerBankAccount = _paymentTestHelper.CreatePlayerBankAccount(_player.Id, _player.BrandId);
            
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _playerBankAccountVerifyPage = _dashboardPage.Menu.ClickPlayerBankAccountVerifyMenuItem();
        }

        public override void AfterEach()
        {
            base.AfterEach();
            _driver.Logout();
        }

        [Test]
        //[Ignore ("Till Volodimir's investigations, 21-April-2016")]
        public void Can_view_player_bank_account_verify()
        {   
            //open view form                                        
            var _viewForm = _playerBankAccountVerifyPage.OpenViewForm(_playerBankAccount.AccountName);
            Assert.AreEqual(_player.Username, _viewForm.Username);
            Assert.AreEqual(_playerBankAccount.Bank.BankName, _viewForm.BankName);
            Assert.AreEqual(_playerBankAccount.Province, _viewForm.Province);
            Assert.AreEqual(_playerBankAccount.City, _viewForm.City);
            Assert.AreEqual(_playerBankAccount.Branch, _viewForm.Branch);
            Assert.AreEqual(_playerBankAccount.SwiftCode, _viewForm.SwiftCode);
            Assert.AreEqual(_playerBankAccount.Address, _viewForm.Address);
            Assert.AreEqual(_playerBankAccount.AccountName, _viewForm.BankAccountName);
            Assert.AreEqual(_playerBankAccount.AccountNumber, _viewForm.BankAccountNumber);
            Assert.AreEqual("Pending", _viewForm.Status);
        }

        [Test]
        public void Can_verify_player_bank_account()
        {
            //verify player's bank account
            var _message = _playerBankAccountVerifyPage.VerifyAndReturnMessage(_playerBankAccount.AccountName);
            Assert.AreEqual("The bank account has been successfully verified.", _message);
            _playerBankAccountVerifyPage.CloseDialog();

            //check status of bank account
            var _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var _withdrawRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_player.Username);
            Assert.AreEqual(_playerBankAccount.Bank.BankName, _withdrawRequestForm.BankNameValue);
            Assert.AreEqual(_playerBankAccount.Province, _withdrawRequestForm.ProvinceValue);
            Assert.AreEqual(_playerBankAccount.City, _withdrawRequestForm.CityValue);
            Assert.AreEqual(_playerBankAccount.Branch, _withdrawRequestForm.Branch);
            Assert.AreEqual(_playerBankAccount.SwiftCode, _withdrawRequestForm.SwiftCode);
            Assert.AreEqual(_playerBankAccount.Address, _withdrawRequestForm.Address);
            Assert.AreEqual(_playerBankAccount.AccountName, _withdrawRequestForm.BankAccountNameValue);
            Assert.AreEqual(_playerBankAccount.AccountNumber, _withdrawRequestForm.BankAccountNumberValue);
        }

        [Test]
        public void Can_reject_player_bank_account()
        {
            //verify player's bank account
            var _message = _playerBankAccountVerifyPage.RejectAndReturnMessage(_playerBankAccount.AccountName);
            Assert.AreEqual("The bank account has been successfully rejected.", _message);
            _playerBankAccountVerifyPage.CloseDialog();

            //check status of bank account
            var _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();
            var _playerInfoPage = _playerManagerPage.OpenPlayerInfoPage(_player.Username);
            _playerInfoPage.OpenBankAccountsSection();
            _playerInfoPage.FindAndSelectRecord(_playerBankAccount.AccountName);
            var _viewForm = _playerInfoPage.OpenViewBankAccountTab();
            Assert.AreEqual(_player.Username, _viewForm.Username);
            Assert.AreEqual(_playerBankAccount.Bank.BankName, _viewForm.BankName);
            Assert.AreEqual(_playerBankAccount.Province, _viewForm.Province);
            Assert.AreEqual(_playerBankAccount.City, _viewForm.City);
            Assert.AreEqual(_playerBankAccount.Branch, _viewForm.Branch);
            Assert.AreEqual(_playerBankAccount.SwiftCode, _viewForm.SwiftCode);
            Assert.AreEqual(_playerBankAccount.Address, _viewForm.Address);
            Assert.AreEqual(_playerBankAccount.AccountName, _viewForm.BankAccountName);
            Assert.AreEqual(_playerBankAccount.AccountNumber, _viewForm.BankAccountNumber);
            Assert.AreEqual("Rejected", _viewForm.Status);
        }
    }
}
