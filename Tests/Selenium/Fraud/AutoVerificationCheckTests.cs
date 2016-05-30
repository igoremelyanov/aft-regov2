using System;
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
    /// Represents tests related to  ->  Withdrawal and Auto Verification Check
    /// </summary>
    [Ignore("Svitlana: 02/15/2016; Ignored until  Fraud subdomain will be back (AFTREGO-4254 should be fixed); AFTREGO-4260 Hide Fraud / Risk UI components in order to isolate functionality for R1.0")]
    public class AutoVerificationCheckTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private PlayerManagerPage _playerManagerPage;

        private Player _player;
        private string _playerUsername;
        protected readonly Guid DefaultBrandId = new Guid("00000000-0000-0000-0000-000000000138");
        private VipLevel _vipLevel;

        private PaymentTestHelper _paymentTestHelper;
        private PlayerCommands _playerCommands;
        private BrandTestHelper _brandTestHelper;
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
            _brandTestHelper = _container.Resolve<BrandTestHelper>();
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _autoVerificationConfigurationTestHelper = _container.Resolve<AutoVerificationConfigurationTestHelper>();


            //create a not default VIP Level for Brand
            _vipLevel = _brandTestHelper.CreateNotDefaultVipLevel(DefaultBrandId);

            //create a player for the DefaultBrandId
            var player = _playerTestHelper.CreatePlayer(isActive: true, brandId: DefaultBrandId);
            var playerId = player.Id;
            _player = _container.Resolve<PlayerQueries>().GetPlayer(playerId);
            _playerUsername = _player.Username;

            _paymentTestHelper.CreatePlayerBankAccount(playerId, DefaultBrandId, true);

            //change the VIP Level for Player
            _playerCommands.ChangeVipLevel(playerId, _vipLevel.Id, "changed vip level");

            //deposit 
            _paymentTestHelper.MakeDeposit(_playerUsername, 100);
            _paymentTestHelper.MakeDeposit(_playerUsername, 200);
            _paymentTestHelper.MakeDeposit(_playerUsername, 300);


            _avcConfigurationBuilder = new AvcConfigurationBuilder(DefaultBrandId, new[] { _vipLevel.Id }, "CAD");

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

        [TestCase("6.14", 8000)]
        public void User_fail_AVC_check_when_player_did_not_meet_the_defined_rule_for_the_total_deposit_amount_criteria
            (string withdrawalAmount, int criteriaAmount)
        {
            _avcConfigurationBuilder.SetupTotalDepositAmount(criteriaAmount, ComparisonEnum.GreaterOrEqual);
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);

            //create a withdrawal request
            OfflineWithdrawRequestData withdrawRequestData = new OfflineWithdrawRequestData();
            withdrawRequestData.Amount = withdrawalAmount;
            withdrawRequestData.Remarks = Guid.NewGuid().ToString();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.SetOfflineWithdrawRequest(withdrawRequestData);
            Assert.AreEqual("Offline withdraw request has been successfully submitted",
                offlineWithdrawalRequestForm.ValidationMessage);
            _playerManagerPage.CloseTab("View Offline Withdraw Request");

            //Navigate to Verification Queue
            var _verificationQueuePage = _dashboardPage.Menu.ClickVerificationQueueMenuItem();

            //Verify the record is present in Verification Queue
            var record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, withdrawalAmount);
            Assert.AreNotEqual(record, null);

            //Verify status
            Assert.AreEqual("New", _verificationQueuePage.GetWithdrawalStatus(record));

            //go to the initial state uncheck criteria
            _avcDTO.HasTotalDepositAmount = false;
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);
        }

        [TestCase("10.36", 600)]
        public void User_pass_AVC_check_when_player_hits_the_defined_rule_for_the_total_deposit_amount_criteria(
            string withdrawalAmount, int criteriaAmount)
        {
            _avcConfigurationBuilder.SetupTotalDepositAmount(criteriaAmount, ComparisonEnum.GreaterOrEqual);
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);

            //create a withdrawal request
            OfflineWithdrawRequestData withdrawRequestData = new OfflineWithdrawRequestData();
            withdrawRequestData.Amount = withdrawalAmount;
            withdrawRequestData.Remarks = Guid.NewGuid().ToString();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.SetOfflineWithdrawRequest(withdrawRequestData);
            Assert.AreEqual("Offline withdraw request has been successfully submitted",
                offlineWithdrawalRequestForm.ValidationMessage);
            _playerManagerPage.CloseTab("View Offline Withdraw Request");


            //Verify the record is moved  to On Acceptance Queue
            var _acceptanceQueuePage = _dashboardPage.Menu.ClickAcceptanceQueueMenuItem();
            var record = _acceptanceQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, withdrawalAmount);
            Assert.AreNotEqual(record, null);

            //Verify the status is Verified
            Assert.AreEqual("Verified", _acceptanceQueuePage.GetWithdrawalStatus(record));

            //go to the initial state uncheck criteria
            _avcDTO.HasTotalDepositAmount = false;
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);
        }


        [TestCase("14.14", 3)]
        public void User_pass_AVC_check_when_player_hits_the_defined_rule_for_the_deposit_count_criteria(
            string withdrawalAmount, int criteriaCount)
        {
            _avcConfigurationBuilder.SetupDepositCount(criteriaCount, ComparisonEnum.GreaterOrEqual);
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);

            //create a withdrawal request
            OfflineWithdrawRequestData withdrawRequestData = new OfflineWithdrawRequestData();
            withdrawRequestData.Amount = withdrawalAmount;
            withdrawRequestData.Remarks = Guid.NewGuid().ToString();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.SetOfflineWithdrawRequest(withdrawRequestData);
            Assert.AreEqual("Offline withdraw request has been successfully submitted",
                offlineWithdrawalRequestForm.ValidationMessage);
            _playerManagerPage.CloseTab("View Offline Withdraw Request");

            //Verify the record is moved  to On Acceptance Queue
            var _acceptanceQueuePage = _dashboardPage.Menu.ClickAcceptanceQueueMenuItem();
            var record = _acceptanceQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, withdrawalAmount);
            Assert.AreNotEqual(record, null);

            //Verify the status is Verified
            Assert.AreEqual("Verified", _acceptanceQueuePage.GetWithdrawalStatus(record));

            //go to the initial state uncheck criteria
            _avcDTO.HasDepositCount = false;
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);
        }

        [TestCase("61.61", 900)]
        public void User_fail_AVC_check_when_player_did_not_meet_the_defined_rule_for_the_deposit_count_criteria(
            string withdrawalAmount, int criteriaCount)
        {
            _avcConfigurationBuilder.SetupDepositCount(criteriaCount, ComparisonEnum.GreaterOrEqual);
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);

            //create a withdrawal request
            OfflineWithdrawRequestData withdrawRequestData = new OfflineWithdrawRequestData();
            withdrawRequestData.Amount = withdrawalAmount;
            withdrawRequestData.Remarks = Guid.NewGuid().ToString();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.SetOfflineWithdrawRequest(withdrawRequestData);
            Assert.AreEqual("Offline withdraw request has been successfully submitted",
                offlineWithdrawalRequestForm.ValidationMessage);
            _playerManagerPage.CloseTab("View Offline Withdraw Request");

            //Navigate to Verification Queue
            var _verificationQueuePage = _dashboardPage.Menu.ClickVerificationQueueMenuItem();

            //Verify the record is present in Verification Queue
            var record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, withdrawalAmount);
            Assert.AreNotEqual(record, null);

            //Verify status
            Assert.AreEqual("New", _verificationQueuePage.GetWithdrawalStatus(record));

            //go to the initial state uncheck criteria
            _avcDTO.HasDepositCount = false;
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);

        }

        [TestCase("1.03", 0)]
        public void User_pass_AVC_check_when_player_did_not_meet_the_defined_rule_for_the_withdrawal_count_criteria(
            string withdrawalAmount, int criteriaCount)
        {
            _avcConfigurationBuilder.SetupTotalWithdrawalCountAmount(criteriaCount, ComparisonEnum.GreaterOrEqual);
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);

            //create a withdrawal request
            OfflineWithdrawRequestData withdrawRequestData = new OfflineWithdrawRequestData();
            withdrawRequestData.Amount = withdrawalAmount;
            withdrawRequestData.Remarks = Guid.NewGuid().ToString();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.SetOfflineWithdrawRequest(withdrawRequestData);
            Assert.AreEqual("Offline withdraw request has been successfully submitted",
                offlineWithdrawalRequestForm.ValidationMessage);
            _playerManagerPage.CloseTab("View Offline Withdraw Request");

            //Verify the record is moved  to On Acceptance Queue
            var _acceptanceQueuePage = _dashboardPage.Menu.ClickAcceptanceQueueMenuItem();
            var record = _acceptanceQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, withdrawalAmount);
            Assert.AreNotEqual(record, null);

            //Verify the status is Verified
            Assert.AreEqual("Verified", _acceptanceQueuePage.GetWithdrawalStatus(record));

            //go to the initial state uncheck criteria
            _avcDTO.HasWithdrawalCount = false;
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);
        }

        [TestCase("2.02", 0)]
        public void User_fail_AVC_check_when_player_did_not_meet_the_defined_rule_for_the_withdrawal_count_criteria(
            string withdrawalAmount, int criteriaAmount)
        {
            _avcConfigurationBuilder.SetupTotalWithdrawalCountAmount(criteriaAmount, ComparisonEnum.Greater);
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);

            //create a withdrawal request
            OfflineWithdrawRequestData withdrawRequestData = new OfflineWithdrawRequestData();
            withdrawRequestData.Amount = withdrawalAmount;
            withdrawRequestData.Remarks = Guid.NewGuid().ToString();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.SetOfflineWithdrawRequest(withdrawRequestData);
            Assert.AreEqual("Offline withdraw request has been successfully submitted",
                offlineWithdrawalRequestForm.ValidationMessage);
            _playerManagerPage.CloseTab("View Offline Withdraw Request");

            //Navigate to Verification Queue
            var _verificationQueuePage = _dashboardPage.Menu.ClickVerificationQueueMenuItem();

            //Verify the record is present in Verification Queue
            var record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, withdrawalAmount);
            Assert.AreNotEqual(record, null);

            //Verify status
            Assert.AreEqual("New", _verificationQueuePage.GetWithdrawalStatus(record));
            //go to the initial state uncheck criteria
            _avcDTO.HasWithdrawalCount = false;
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);
        }

        [TestCase("1.08", 0)]
        public void User_pass_AVC_check_when_player_hits_the_defined_rule_for_the_account_age_criteria(
            string withdrawalAmount, int criteriaDays)
        {
            _avcConfigurationBuilder.SetupAccountAge(criteriaDays, ComparisonEnum.GreaterOrEqual);
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);

            //create a withdrawal request
            OfflineWithdrawRequestData withdrawRequestData = new OfflineWithdrawRequestData();
            withdrawRequestData.Amount = withdrawalAmount;
            withdrawRequestData.Remarks = Guid.NewGuid().ToString();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.SetOfflineWithdrawRequest(withdrawRequestData);
            Assert.AreEqual("Offline withdraw request has been successfully submitted",
                offlineWithdrawalRequestForm.ValidationMessage);
            _playerManagerPage.CloseTab("View Offline Withdraw Request");

            //Verify the record is moved  to On Acceptance Queue
            var _acceptanceQueuePage = _dashboardPage.Menu.ClickAcceptanceQueueMenuItem();
            var record = _acceptanceQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, withdrawalAmount);
            Assert.AreNotEqual(record, null);

            //Verify the status is Verified
            Assert.AreEqual("Verified", _acceptanceQueuePage.GetWithdrawalStatus(record));

            //go to the initial state uncheck criteria
            _avcDTO.HasAccountAge = false;
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);
        }

        [TestCase("1.07", 0)]
        public void User_fail_AVC_check_when_player_did_not_meet_the_defined_rule_for_the_account_age_criteria(
            string withdrawalAmount, int criteriaAmount)
        {
            _avcConfigurationBuilder.SetupAccountAge(criteriaAmount, ComparisonEnum.Greater);
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);

            //create a withdrawal request
            OfflineWithdrawRequestData withdrawRequestData = new OfflineWithdrawRequestData();
            withdrawRequestData.Amount = withdrawalAmount;
            withdrawRequestData.Remarks = Guid.NewGuid().ToString();

            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.SetOfflineWithdrawRequest(withdrawRequestData);
            Assert.AreEqual("Offline withdraw request has been successfully submitted",
                offlineWithdrawalRequestForm.ValidationMessage);
            _playerManagerPage.CloseTab("View Offline Withdraw Request");

            //Navigate to Verification Queue
            var _verificationQueuePage = _dashboardPage.Menu.ClickVerificationQueueMenuItem();

            //Verify the record is present in Verification Queue
            var record = _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, withdrawalAmount);
            Assert.AreNotEqual(record, null);

            //Verify status
            Assert.AreEqual("New", _verificationQueuePage.GetWithdrawalStatus(record));
            //go to the initial state uncheck criteria

            //go to the initial state uncheck criteria
            _avcDTO.HasAccountAge = false;
            _autoVerificationConfigurationTestHelper.UpdateConfiguration(_avcDTO);
        }
    }
}