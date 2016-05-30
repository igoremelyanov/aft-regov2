using System;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using VipLevel = AFT.RegoV2.Core.Brand.Interface.Data.VipLevel;

namespace AFT.RegoV2.Tests.Selenium.Fraud
{
    /// <summary>
    /// Represents tests related to  ->  Withdrawal workflow
    /// </summary>
    [Ignore("Svitlana: 02/15/2016; Ignored until  Fraud subdomain will be back")]
    public class WithdrawalProcessingTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private PlayerManagerPage _playerManagerPage;

        private Player _player;
        private string _playerUsername;
        protected readonly Guid DefaultBrandId = new Guid("00000000-0000-0000-0000-000000000138");
        private VipLevel _vipLevel;

        private PaymentTestHelper _paymentTestHelper;
        private PlayerCommands _playerCommands;
        private PlayerTestHelper _playerTestHelper;
        private AutoVerificationConfigurationTestHelper _autoVerificationConfigurationTestHelper;
        private AvcConfigurationBuilder _avcConfigurationBuilder;
        private AVCConfigurationDTO _avcDTO;
        private AutoVerificationCheckConfiguration _avc;



        public override void BeforeAll()
        {
            base.BeforeAll();

            _paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            _playerCommands = _container.Resolve<PlayerCommands>();
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _autoVerificationConfigurationTestHelper = _container.Resolve<AutoVerificationConfigurationTestHelper>();


            var brand = _container.Resolve<BrandQueries>().GetBrand(DefaultBrandId);

            _vipLevel = brand.DefaultVipLevel;
            //create a not default VIP Level for Brand

            //create a player for the DefaultBrandId
            _player = _playerTestHelper.CreatePlayer(isActive: true, brandId: DefaultBrandId);
            var playerId = _player.Id;
            _playerUsername = _player.Username;

            _paymentTestHelper.CreatePlayerBankAccount(playerId, DefaultBrandId, true);

            //change the VIP Level for Player
            _playerCommands.ChangeVipLevel(playerId, _vipLevel.Id, "changed vip level");

            //deposit 
            _paymentTestHelper.MakeDeposit(_playerUsername, 200);


            //create Auto Verification configuration which expected to be failed
            _avcConfigurationBuilder = new AvcConfigurationBuilder(DefaultBrandId, new[] { _vipLevel.Id }, "CAD");
            _avcConfigurationBuilder.SetupTotalDepositAmount(1500, ComparisonEnum.GreaterOrEqual);
            _avcDTO = _avcConfigurationBuilder.Configuration;

            _avc = _autoVerificationConfigurationTestHelper.CreateConfiguration(_avcDTO);
            _autoVerificationConfigurationTestHelper.Activate(_avc.Id);

        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

        }

        [TestCase("1.59")]
        public void Can_tag_withdrawal_request_to_Docs(string amount)
        {
            //create a withdrawal request
            OfflineWithdrawRequestData withdrawRequestData = new OfflineWithdrawRequestData();
            withdrawRequestData.Amount = amount;
            withdrawRequestData.Remarks = Guid.NewGuid().ToString();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.SetOfflineWithdrawRequest(withdrawRequestData);
            Assert.AreEqual("Offline withdraw request has been successfully submitted", offlineWithdrawalRequestForm.ValidationMessage);
            _playerManagerPage.CloseTab("View Offline Withdraw Request");

            //Navigate to Verification Queue
            var _verificationQueuePage = _dashboardPage.Menu.ClickVerificationQueueMenuItem();

            //Verify the record is present in Verification Queue
            var record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreNotEqual(record, null);

            //Verify status
            Assert.AreEqual("New", _verificationQueuePage.GetWithdrawalStatus(record));

            var _documentsPage = _verificationQueuePage.OpenDocumentsForm(record);
            _documentsPage.SubmitProcessing("Verification Queue: Tagged to Documents");

            Assert.AreEqual("Offline withdraw request has been successfully tagged for documents investigation", _documentsPage.SuccessAlert.Text);

            _documentsPage.CloseTab("Documents");

            //Verify the record is removed from Verification Queue
            record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreEqual(record, null);
            _verificationQueuePage.CloseTab("Verification Queue");

            //Verify the record is moved  to On Hold Queue
            var _onHoldQueuePage = _dashboardPage.Menu.ClickOnHoldQueueMenuItem();
            record = _onHoldQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreNotEqual(record, null);

            //Verify the status is Documents
            Assert.AreEqual("Documents", _onHoldQueuePage.GetWithdrawalStatus(record));
        }

        [TestCase("2.18")]
        public void Can_tag_withdrawal_request_to_Investigate(string amount)
        {
            //create a withdrawal request
            OfflineWithdrawRequestData withdrawRequestData = new OfflineWithdrawRequestData();
            withdrawRequestData.Amount = amount;
            withdrawRequestData.Remarks = Guid.NewGuid().ToString();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.SetOfflineWithdrawRequest(withdrawRequestData);
            Assert.AreEqual("Offline withdraw request has been successfully submitted", offlineWithdrawalRequestForm.ValidationMessage);
            _playerManagerPage.CloseTab("View Offline Withdraw Request");

            //Navigate to Verification Queue
            var _verificationQueuePage = _dashboardPage.Menu.ClickVerificationQueueMenuItem();

            //Verify the record is present in Verification Queue
            var record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreNotEqual(record, null);

            //Verify status
            Assert.AreEqual("New", _verificationQueuePage.GetWithdrawalStatus(record));

            var _documentsPage = _verificationQueuePage.OpenInvestigateForm(record);
            _documentsPage.SubmitProcessing("Verification Queue: Tagged to Investigate");

            Assert.AreEqual("Offline withdraw request has been successfully tagged for investigation", _documentsPage.SuccessAlert.Text);

            _documentsPage.CloseTab("Investigate");

            //Verify the record is removed from Verification Queue
            record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreEqual(record, null);
            _verificationQueuePage.CloseTab("Verification Queue");

            //Verify the record is moved  to On Hold Queue
            var _onHoldQueuePage = _dashboardPage.Menu.ClickOnHoldQueueMenuItem();
            record = _onHoldQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreNotEqual(record, null);

            //Verify the status is Investigation
            Assert.AreEqual("Investigation", _onHoldQueuePage.GetWithdrawalStatus(record));
        }

        [TestCase("5.99")]
        public void Can_manually_verify_withdrawal_request_in_verification_queue(string amount)
        {
            //create a withdrawal request
            OfflineWithdrawRequestData withdrawRequestData = new OfflineWithdrawRequestData();
            withdrawRequestData.Amount = amount;
            withdrawRequestData.Remarks = Guid.NewGuid().ToString();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.SetOfflineWithdrawRequest(withdrawRequestData);
            Assert.AreEqual("Offline withdraw request has been successfully submitted", offlineWithdrawalRequestForm.ValidationMessage);
            _playerManagerPage.CloseTab("View Offline Withdraw Request");

            //Navigate to Verification Queue
            var _verificationQueuePage = _dashboardPage.Menu.ClickVerificationQueueMenuItem();

            //Verify the record is present in Verification Queue
            var record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreNotEqual(record, null);

            //Verify status
            Assert.AreEqual("New", _verificationQueuePage.GetWithdrawalStatus(record));

            var _documentsPage = _verificationQueuePage.OpenVerifyForm(record);
            _documentsPage.SubmitProcessing("Verification Queue: Verified.");

            Assert.AreEqual("Offline withdraw request has been successfully verified", _documentsPage.SuccessAlert.Text);

            _documentsPage.CloseTab("Verify");

            //Verify the record is removed from Verification Queue
            record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreEqual(record, null);
            _verificationQueuePage.CloseTab("Verification Queue");

            //Verify the record is moved  to On Acceptance Queue
            var _acceptanceQueuePage = _dashboardPage.Menu.ClickAcceptanceQueueMenuItem();
            record = _acceptanceQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreNotEqual(record, null);

            //Verify the status is Verified
            Assert.AreEqual("Verified", _acceptanceQueuePage.GetWithdrawalStatus(record));
        }

        [TestCase("3.51")]
        public void Can_unverify_withdrawal_request_in_verification_queue(string amount)
        {
            //create a withdrawal request
            OfflineWithdrawRequestData withdrawRequestData = new OfflineWithdrawRequestData();
            withdrawRequestData.Amount = amount;
            withdrawRequestData.Remarks = Guid.NewGuid().ToString();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.SetOfflineWithdrawRequest(withdrawRequestData);
            Assert.AreEqual("Offline withdraw request has been successfully submitted", offlineWithdrawalRequestForm.ValidationMessage);
            _playerManagerPage.CloseTab("View Offline Withdraw Request");

            //Navigate to Verification Queue
            var _verificationQueuePage = _dashboardPage.Menu.ClickVerificationQueueMenuItem();

            //Verify the record is present in Verification Queue
            var record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreNotEqual(record, null);

            //Verify status
            Assert.AreEqual("New", _verificationQueuePage.GetWithdrawalStatus(record));

            var _documentsPage = _verificationQueuePage.OpenUnverifyForm(record);
            _documentsPage.SubmitProcessing("Verification Queue: Unverified.");

            Assert.AreEqual("Offline withdraw request has been successfully unverified", _documentsPage.SuccessAlert.Text);

            _documentsPage.CloseTab("Unverify");

            //Verify the record is removed from Verification Queue
            record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreEqual(record, null);
            _verificationQueuePage.CloseTab("Verification Queue");

            //Verify the record is not present in On Hold Queue
            var _onHoldQueuePage = _dashboardPage.Menu.ClickOnHoldQueueMenuItem();
            record = _onHoldQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreEqual(record, null);

            //Verify the record is not present in Acceptance Queue
            var _acceptanceQueuePage = _dashboardPage.Menu.ClickAcceptanceQueueMenuItem();
            record = _acceptanceQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreEqual(record, null);

        }

        [TestCase("15.40")]
        public void Can_unverify_withdrawal_request_in_on_hold_queue(string amount)
        {
            //create a withdrawal request
            OfflineWithdrawRequestData withdrawRequestData = new OfflineWithdrawRequestData();
            withdrawRequestData.Amount = amount;
            withdrawRequestData.Remarks = Guid.NewGuid().ToString();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.SetOfflineWithdrawRequest(withdrawRequestData);
            Assert.AreEqual("Offline withdraw request has been successfully submitted", offlineWithdrawalRequestForm.ValidationMessage);
            _playerManagerPage.CloseTab("View Offline Withdraw Request");
            _playerManagerPage.CloseTab("Players");

            //Navigate to Verification Queue
            var _verificationQueuePage = _dashboardPage.Menu.ClickVerificationQueueMenuItem();

            //Verify the record is present in Verification Queue
            var record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreNotEqual(record, null);

            //Verify status
            Assert.AreEqual("New", _verificationQueuePage.GetWithdrawalStatus(record));

            var _documentsPage = _verificationQueuePage.OpenDocumentsForm(record);
            _documentsPage.SubmitProcessing("Verification Queue: Tagged to Documents");

            Assert.AreEqual("Offline withdraw request has been successfully tagged for documents investigation", _documentsPage.SuccessAlert.Text);

            _documentsPage.CloseTab("Documents");

            //Verify the record is removed from Verification Queue
            record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreEqual(record, null);

            _verificationQueuePage.CloseTab("Verification Queue");

            //Verify the record is moved to On Hold Queue
            var _onHoldQueuePage = _dashboardPage.Menu.ClickOnHoldQueueMenuItem();
            record = _onHoldQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreNotEqual(record, null);

            //Unverify record
            var _unverifyPage = _onHoldQueuePage.OpenUnverifyForm(record);
            _unverifyPage.SubmitProcessing("On Hold Queue: Unverified");

            Assert.AreEqual("Offline withdraw request has been successfully unverified", _unverifyPage.SuccessAlert.Text);
            _unverifyPage.CloseTab("Unverify");

            //Verify the record is not present in On Hold Queue
            record = _onHoldQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreEqual(record, null);

            _onHoldQueuePage.CloseTab("On Hold Queue");

            //Verify the record is not present in Acceptance Queue
            var _acceptanceQueuePage = _dashboardPage.Menu.ClickAcceptanceQueueMenuItem();
            record = _acceptanceQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, amount);
            Assert.AreEqual(record, null);

        }
    }
}