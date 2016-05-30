using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Infrastructure.DataAccess.Game;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using WinService.Workers;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    public class OfflineWithdrawalWithholdTests : AdminWebsiteUnitTestsBase
    {
        #region Fields

        private FakePaymentRepository _paymentRepository;
        private PlayerQueries _playerQueries;
        private IActorInfoProvider _actorInfoProvider;
        private IWithdrawalService _withdrawalService;
        private PaymentTestHelper _paymentTestHelper;
        private GamesTestHelper _gamesTestHelper;
        private IWalletQueries _walletQueries;
        private GameRepository _walletRepository;

        #endregion

        public override void BeforeEach()
        {
            base.BeforeEach();
            
            _withdrawalService = Container.Resolve<IWithdrawalService>();
            _playerQueries = Container.Resolve<PlayerQueries>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            _actorInfoProvider = Container.Resolve<IActorInfoProvider>();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
            _walletQueries = Container.Resolve<IWalletQueries>();
            _walletRepository = Container.Resolve<GameRepository>();

            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();

            Container.Resolve<PaymentWorker>().Start();

            var brandHelper = Container.Resolve<BrandTestHelper>();
            var brand = brandHelper.CreateActiveBrandWithProducts();

            var player = Container.Resolve<PlayerTestHelper>().CreatePlayer();
            _paymentTestHelper.CreatePlayerBankAccount(player.Id, brand.Id, true);
        }

        [Test, Ignore("KB: need to rewrite not basing on WalletQueries")]
        public async Task Can_withhold_withdrawal_money()
        {
            var player = _playerQueries.GetPlayers().ToList().First();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 1100, player.Id);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1000,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            _withdrawalService.Request(offlineWithdrawalRequest);

            var playerBalance = await _walletQueries.GetPlayerBalance(player.Id);
            Assert.AreEqual(100, playerBalance.Free);
            Assert.AreEqual(1000, playerBalance.WithdrawalLock);
        }

        [Test, Ignore("KB: need to rewrite not basing on WalletQueries")]
        public async Task Can_revert_withhold_withdrawal_money_after_withdrawal_canceletion()
        {
            var player = _playerQueries.GetPlayers().ToList().First();
            _walletRepository.SaveChanges();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(1000, 1100, player.Id);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1000,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _actorInfoProvider.Actor.UserName
            };

            var response = _withdrawalService.Request(offlineWithdrawalRequest);

            var playerBalance = await _walletQueries.GetPlayerBalance(player.Id);
            Assert.AreEqual(100, playerBalance.Free);
            Assert.AreEqual(1000, playerBalance.WithdrawalLock);

            _withdrawalService.Cancel(
                response.Id,
                TestDataGenerator.GetRandomString());

            playerBalance = await _walletQueries.GetPlayerBalance(player.Id);
            Assert.AreEqual(1100, playerBalance.Free);
            Assert.AreEqual(0, playerBalance.WithdrawalLock);
        }
    }
}
