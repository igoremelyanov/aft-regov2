using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Player.ApplicationServices;

namespace AFT.RegoV2.Tests.Selenium.Fraud
{
    [Ignore("AFTREGO-4260 Hide Fraud / Risk UI components in order to isolate functionality for R1.0")]
    class AutoVerificationConfigurationWithdrawalTests : SeleniumBaseForAdminWebsite
    {
        private PlayerManagerPage _playerManagerPage;
        private VerificationQueuePage _verificationQueuePage;
        private AcceptanceQueuePage _acceptanceQueuePage;
        private DashboardPage _dashboardPage;
        private string _playerUsername;
        private string _playerFirstname;
        private string _playerLastname;
        private string _playerFullname;
        private PaymentTestHelper _paymentTestHelper;
        private PlayerQueries _playerQueries;
        private GamesTestHelper _gamesTestHelper;
        private BrandTestHelper _brandTestHelper;
        protected readonly Guid DefaultBrandId = new Guid("00000000-0000-0000-0000-000000000138");
        private Core.Brand.Interface.Data.Brand _brand;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
            _playerManagerPage = _dashboardPage.Menu.ClickPlayerManagerMenuItem();

            _brandTestHelper = _container.Resolve<BrandTestHelper>();
            _paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            _playerQueries = _container.Resolve<PlayerQueries>();
            _gamesTestHelper = _container.Resolve<GamesTestHelper>();
            var avcTestHelper = _container.Resolve<AutoVerificationConfigurationTestHelper>();
            var playerTestHelper = _container.Resolve<PlayerTestHelper>();
            var brandQueries = _container.Resolve<BrandQueries>();
            var playerCommands = _container.Resolve<PlayerCommands>();

            //create a brand for a default licensee
            _brand = brandQueries.GetBrand(DefaultBrandId);

            //create a not default VIP Level for Brand
            var vipLevel = _brandTestHelper.CreateNotDefaultVipLevel(DefaultBrandId);

            //create Auto Verification Configuration for custom Brand
            var gameRepository = _container.Resolve<IGameRepository>();

            var avcConfigurationBuilder = new AvcConfigurationBuilder(_brand.Id, new[] { vipLevel.Id }, "CAD");
            avcConfigurationBuilder
                .SetupWinnings(new List<WinningRuleDTO>
                {
                    new WinningRuleDTO
                    {
                        Id = Guid.NewGuid(),
                        ProductId = gameRepository.GameProviders.Single(g => g.Name == "Mock Sport Bets").Id,
                        Comparison = ComparisonEnum.Greater,
                        Amount = 200,
                        Period = PeriodEnum.FromSignUp
                    }
                });

            var configuration = avcConfigurationBuilder.Configuration;
            var createdConfiguration = avcTestHelper.CreateConfiguration(configuration);
            avcTestHelper.Activate(createdConfiguration.Id);

            // create a player with a bound bank account for a brand
            var player = playerTestHelper.CreatePlayer(true, _brand.Id);
            _playerUsername = player.Username;
            _playerFirstname = player.FirstName;
            _playerLastname = player.LastName;
            _playerFullname = _playerFirstname + " " + _playerLastname;
            _paymentTestHelper.CreatePlayerBankAccount(player.Id, DefaultBrandId, true);

            //change the VIP Level for Player
            playerCommands.ChangeVipLevel(player.Id, vipLevel.Id, "changed vip level");

            //deposit some money
            _paymentTestHelper.MakeDeposit(_playerUsername, 400);
            Thread.Sleep(5000); //wait for Deposit created event processing
        }

        [Test]
        public void Can_not_make_withdrawal_when_player_did_not_hit_Has_Winnings_amount_criteria_via_Admin_site()
        {
            //create a withdrawal request
            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.TryToSubmit("10", NotificationMethod.Email);
            Assert.AreEqual("Offline withdraw request has been successfully submitted", offlineWithdrawalRequestForm.ValidationMessage);

            //check the withdrawal request record in Verification Queue
            _verificationQueuePage = _dashboardPage.Menu.ClickVerificationQueueMenuItem();
            _verificationQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, "10");
            //check failed status for the withdrawal in Verification Queue
            Assert.IsTrue(_verificationQueuePage.CheckIfWithdrawalRequestStatusIsFailed(_playerUsername, _playerFullname));

        }

        [Test]
        public async Task Can_make_withdrawal_when_player_hits_Has_Winnings_amount_criteria_via_Admin_site()
        {
            //place bet and make winnings
            var playerId = _playerQueries.GetPlayerByUsername(_playerUsername).Id;
            var gameId = "FOOTBALL";

            var gameProviderCode = _gamesTestHelper.GetGameProviderCodeByGameExternalId(gameId);
            var placeBetTxId = Guid.NewGuid().ToString();
            var actualBetId = await _gamesTestHelper.PlaceBet(13, playerId, gameProviderCode, gameId, transactionId: placeBetTxId);
            //TODO: VladK.  - remove Sleep here 
            Thread.Sleep(5000);// wait for Bet event processing
            await _gamesTestHelper.WinBet(actualBetId, 201, placeBetTxId, gameProviderCode);
            //TODO: VladK.  - remove Sleep here 
            Thread.Sleep(5000); //wait for Bet event processing

            //create a withdrawal request
            _playerManagerPage.SelectPlayer(_playerUsername);
            var offlineWithdrawalRequestForm = _playerManagerPage.OpenOfflineWithdrawRequestForm(_playerUsername);
            offlineWithdrawalRequestForm.TryToSubmit("10", NotificationMethod.Email);
            Assert.AreEqual("Offline withdraw request has been successfully submitted", offlineWithdrawalRequestForm.ValidationMessage);

            //check the withdrawal request record in Acceptance Queue
            _acceptanceQueuePage = _dashboardPage.Menu.ClickAcceptanceQueueMenuItem();
            _acceptanceQueuePage.FindAndSelectWithdrawalRecord(_playerUsername, "10");
            //check verified status for the withdrawal in Acceptance Queue
            Assert.IsTrue(_acceptanceQueuePage.CheckIfWithdrawalRequestStatusIsVerified(_playerUsername, _playerFullname));

            //accept withdrawal request

            //check accepted withdrawal request in Release Queue
            //check status for the withdrawal in Release Queue
        }

    }
}

