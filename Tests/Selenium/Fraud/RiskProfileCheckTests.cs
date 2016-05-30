using System;
using System.Collections.Generic;
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
    /// Represents tests related to  ->  Withdrawal and Risk Profile Check
    /// </summary>
    [Ignore("Svitlana: 02/15/2016; Ignored until  Fraud subdomain will be back (AFTREGO-4254 should be fixed); AFTREGO-4260 Hide Fraud / Risk UI components in order to isolate functionality for R1.0")]
    public class RiskProfileCheckTests : SeleniumBaseForAdminWebsite
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
        private RiskProfileCheckTestHelper _riskProfileCheckTestHelper;
        private AvcConfigurationBuilder _avcConfigurationBuilder;
        private RiskProfileCheckConfigurationBuilder _riskProfileCheckConfigurationBuilder;
        private AVCConfigurationDTO _avcDTO;
        private AutoVerificationCheckConfiguration _avc;
        private RiskProfileCheckDTO _rpcDTO;

        public override void BeforeAll()
        {
            base.BeforeAll();

            _paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            _playerCommands = _container.Resolve<PlayerCommands>();
            _brandTestHelper = _container.Resolve<BrandTestHelper>();
            _playerTestHelper = _container.Resolve<PlayerTestHelper>();
            _autoVerificationConfigurationTestHelper = _container.Resolve<AutoVerificationConfigurationTestHelper>();
            _riskProfileCheckTestHelper = _container.Resolve<RiskProfileCheckTestHelper>();

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

            //create Auto Verification configuration which expected to be failed
            _avcConfigurationBuilder = new AvcConfigurationBuilder(DefaultBrandId, new [] { _vipLevel.Id }, "CAD");
            _avcConfigurationBuilder.SetupTotalDepositAmount(1500, ComparisonEnum.GreaterOrEqual);
            _avcDTO = _avcConfigurationBuilder.Configuration;

            _avc = _autoVerificationConfigurationTestHelper.CreateConfiguration(_avcDTO);
            _autoVerificationConfigurationTestHelper.Activate(_avc.Id);

            //create Risk Profile Check configuration
            _riskProfileCheckConfigurationBuilder = new RiskProfileCheckConfigurationBuilder(DefaultBrandId, _avcDTO.Licensee, "CAD", new List<Guid> { _vipLevel.Id });
            _rpcDTO = _riskProfileCheckConfigurationBuilder.Configuration;
            var createdConfigurationEntity = _riskProfileCheckTestHelper.CreateConfiguration(_rpcDTO);
            _rpcDTO.Id = createdConfigurationEntity.Id;
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

        }

        [TestCase("33.33", 3)]
        public void Can_set_high_risk_level_when_player_hits_the_defined_rule_for_the_deposit_count_criteria(
            string withdrawalAmount, int criteriaCount)
        {
            _riskProfileCheckConfigurationBuilder.SetupDepositCount(criteriaCount, ComparisonEnum.GreaterOrEqual);
            _riskProfileCheckTestHelper.UpdateConfiguration(_rpcDTO);

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

            //Verify Risk Profile check result
            Assert.IsTrue(_verificationQueuePage.GetWithdrawalRiskProfileCheckResult(record).Contains("High"));

            //go to the initial state uncheck criteria
            _rpcDTO.HasDepositCount = false;
            _riskProfileCheckTestHelper.UpdateConfiguration(_rpcDTO);

        }


        [TestCase("11.11", 3)]
        public void Can_set_low_risk_level_when_player_did_not_meet_the_defined_rule_for_the_deposit_count_criteria(
            string withdrawalAmount, int criteriaCount)
        {
            _riskProfileCheckConfigurationBuilder.SetupDepositCount(criteriaCount, ComparisonEnum.Greater);
            _riskProfileCheckTestHelper.UpdateConfiguration(_rpcDTO);

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

            //Verify Risk Profile check result
            Assert.IsTrue(_verificationQueuePage.GetWithdrawalRiskProfileCheckResult(record).Contains("Low"));

            //go to the initial state uncheck criteria
            _rpcDTO.HasDepositCount = false;
            _riskProfileCheckTestHelper.UpdateConfiguration(_rpcDTO);

        }

        [TestCase("1.25", 0)]
        public void Can_set_high_risk_level_when_player_hits_the_defined_rule_for_the_withdrawal_count_criteria(
            string withdrawalAmount, int criteriaCount)
        {
            _riskProfileCheckConfigurationBuilder.SetupTotalWithdrawalCountAmount(criteriaCount, ComparisonEnum.GreaterOrEqual);
            _riskProfileCheckTestHelper.UpdateConfiguration(_rpcDTO);

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

            //Verify Risk Profile check result
            Assert.IsTrue(_verificationQueuePage.GetWithdrawalRiskProfileCheckResult(record).Contains("High"));

            //go to the initial state uncheck criteria
            _rpcDTO.HasWithdrawalCount = false;
            _riskProfileCheckTestHelper.UpdateConfiguration(_rpcDTO);

        }

        [TestCase("1.08", 0)]
        public void Can_set_low_risk_level_when_player_did_not_meet_the_defined_rule_for_the_withdrawal_count_criteria(
            string withdrawalAmount, int criteriaCount)
        {
            _riskProfileCheckConfigurationBuilder.SetupTotalWithdrawalCountAmount(criteriaCount, ComparisonEnum.Greater);
            _riskProfileCheckTestHelper.UpdateConfiguration(_rpcDTO);

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

            //Verify Risk Profile check result
            Assert.IsTrue(_verificationQueuePage.GetWithdrawalRiskProfileCheckResult(record).Contains("Low"));

            //go to the initial state uncheck criteria
            _rpcDTO.HasWithdrawalCount = false;
            _riskProfileCheckTestHelper.UpdateConfiguration(_rpcDTO);

        }

        [TestCase("3.01", 0)]
        public void Can_set_low_risk_level_when_player_did_not_meet_the_defined_rule_for_the_account_age_criteria(
            string withdrawalAmount, int criteriaDays)
        {
            _riskProfileCheckConfigurationBuilder.SetupAccountAge(criteriaDays, ComparisonEnum.Greater);
            _riskProfileCheckTestHelper.UpdateConfiguration(_rpcDTO);

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

            //Verify Risk Profile check result
            Assert.IsTrue(_verificationQueuePage.GetWithdrawalRiskProfileCheckResult(record).Contains("Low"));

            //go to the initial state uncheck criteria
            _rpcDTO.HasAccountAge = false;
            _riskProfileCheckTestHelper.UpdateConfiguration(_rpcDTO);
        }

        [TestCase("1.25", 0)]
        public void Can_set_high_risk_level_when_player_hits_the_defined_rule_for_the_account_age_criteria(
            string withdrawalAmount, int criteriaDays)
        {
            _riskProfileCheckConfigurationBuilder.SetupTotalWithdrawalCountAmount(criteriaDays, ComparisonEnum.GreaterOrEqual);
            _riskProfileCheckTestHelper.UpdateConfiguration(_rpcDTO);

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

            //Verify Risk Profile check result
            Assert.IsTrue(_verificationQueuePage.GetWithdrawalRiskProfileCheckResult(record).Contains("High"));

            //go to the initial state uncheck criteria
            _rpcDTO.HasAccountAge = false;
            _riskProfileCheckTestHelper.UpdateConfiguration(_rpcDTO);
        }

    }
}
