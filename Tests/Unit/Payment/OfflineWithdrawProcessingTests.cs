using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using WinService.Workers;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    public class OfflineWithdrawProcessingTests : AdminWebsiteUnitTestsBase
    {
        #region Fields
        private FakePaymentRepository _paymentRepository;
        private PlayerQueries _playerQueries;
        private IWithdrawalService _withdrawalService;
        private PaymentTestHelper _paymentTestHelper;
        private Core.Common.Data.Player.Player _player;
        private Guid _observedWithdrawalId;
        public BonusBalance Balance { get; set; }
        #endregion

        #region Methods

        public override void BeforeEach()
        {
            base.BeforeEach();

            Balance = new BonusBalance();
            var bonusApiMock = new Mock<IBonusApiProxy>();
            bonusApiMock.Setup(proxy => proxy.GetPlayerBalanceAsync(It.IsAny<Guid>(), It.IsAny<Guid?>())).ReturnsAsync(Balance);
            bonusApiMock.Setup(proxy => proxy.GetWageringBalancesAsync(It.IsAny<Guid>(), It.IsAny<Guid?>())).ReturnsAsync(new PlayerWagering());
            Container.RegisterInstance(bonusApiMock.Object);

            _withdrawalService = Container.Resolve<IWithdrawalService>();
            _playerQueries = Container.Resolve<PlayerQueries>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            Container.Resolve<PaymentWorker>().Start();
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();

            var brandHelper = Container.Resolve<BrandTestHelper>();
            var brand = brandHelper.CreateActiveBrandWithProducts();

            var player = Container.Resolve<PlayerTestHelper>().CreatePlayer();
            _paymentTestHelper.CreatePlayerBankAccount(player.Id, brand.Id, true);

            _player = _playerQueries.GetPlayers().ToList().First();

            _observedWithdrawalId = Guid.NewGuid();
        }

        [Test]
        public void Withdrawal_goes_from_VQ_to_OHQ_after_tagged_documents()
        {
            //Create a withdrawal in the "Verification Queue"
            var withdrawal = new OfflineWithdraw();
            SetWithdrawalProperties(withdrawal, WithdrawalStatus.AutoVerificationFailed);

            //Move withdrawal to the "On Hold Queue"
            _withdrawalService.SetDocumentsState(_observedWithdrawalId, "Investigate withdrawal request.");

            //Assert that the withdrawal request was moved to the "On Hold Queue"
            var withdrawalsInTheOnHoldQueue = _withdrawalService.GetWithdrawalsOnHold();     
            Assert.IsTrue(withdrawalsInTheOnHoldQueue.Any(wd => wd.Id == _observedWithdrawalId));
        }

        [Test]
        public void Withdrawal_goes_from_VQ_to_OHQ_after_investigation()
        {
            //Create a withdrawal in the "Verification Queue"
            var withdrawal = new OfflineWithdraw();
            SetWithdrawalProperties(withdrawal, WithdrawalStatus.AutoVerificationFailed);

            //Move withdrawal to the "On Hold Queue"
            _withdrawalService.SetInvestigateState(_observedWithdrawalId, "Investigate withdrawal request.");

            //Assert that the withdrawal request was moved to the "On Hold Queue"
            var withdrawalsInTheOnHoldQueue = _withdrawalService.GetWithdrawalsOnHold();           
            Assert.IsTrue(withdrawalsInTheOnHoldQueue.Any(wd => wd.Id == _observedWithdrawalId));
        }

        [Test]
        public void Withdrawal_goes_from_VQ_to_AQ_after_verification()
        {
            //Create a withdrawal to go to the "Verification Queue"
            var withdrawal = new OfflineWithdraw();
            SetWithdrawalProperties(withdrawal, WithdrawalStatus.AutoVerificationFailed);

            //Move withdrawal to the "Acceptance Queue"
            _withdrawalService.Verify(_observedWithdrawalId, "Verify withdrawal request.");

            //Assert that the withdrawal request was moved to the "Acceptance queue"
            var withdrawalsInTheAcceptanceQueue = _withdrawalService.GetWithdrawalsVerified();          
            Assert.IsTrue(withdrawalsInTheAcceptanceQueue.Any(wd => wd.Id == _observedWithdrawalId));
        }

        [Test]
        public void Reverted_withdrawal_goes_from_AQ_to_VQ()
        {
            //Create a withdrawal to go to the "On Acceptance queue"
            var withdrawal = new OfflineWithdraw();

            SetWithdrawalProperties(withdrawal, WithdrawalStatus.Verified);

            _withdrawalService.Revert(_observedWithdrawalId, "Revert withdrawal request.");

            var withdrawalsInTheVerificationQueue = _withdrawalService.GetWithdrawalsForVerificationQueue();

            //Assert that the withdrawal request was moved to the "On Hold Queue"
            Assert.IsTrue(withdrawalsInTheVerificationQueue.Any(wd => wd.Id == _observedWithdrawalId));
        }

        [Test]
        public void Cancelled_withdrawal_is_not_available_in_any_queue()
        {
            CancelOfflineWithdrawalRequest(WithdrawalStatus.Canceled);
        }

        [Test]
        public void Unverified_withdrawal_is_not_available_in_any_queue()
        {
            CancelOfflineWithdrawalRequest(WithdrawalStatus.Unverified);
        }

        #endregion

        #region Private methods
        private void SetWithdrawalProperties(OfflineWithdraw withdrawal, WithdrawalStatus status)
        {
            _paymentRepository.OfflineWithdraws.Add(withdrawal);

            withdrawal.Amount = 1000;
            withdrawal.Id = _observedWithdrawalId;
            withdrawal.PlayerBankAccount =
                _paymentRepository.PlayerBankAccounts
                .Include(x => x.Player)
                .First(x => x.Player.Id == _player.Id);

            withdrawal.Status = status;
            _paymentRepository.SaveChanges();
        }

        private void CancelOfflineWithdrawalRequest(WithdrawalStatus stateToGetInto)
        {
            _paymentTestHelper.MakeDeposit(_player.Id, 2000);
            Balance.Main = 2000;

            var response = _paymentTestHelper.MakeWithdraw(_player.Id, amount: 100);
            _withdrawalService.Revert(response.Id, "Revert to Verification Queue");
            /*
            */
            //Cancel withdrawal request
            if (stateToGetInto == WithdrawalStatus.Canceled)
            {
                _withdrawalService.Cancel(response.Id, "Cancel Withdrawal Request");
            }
            else if (stateToGetInto == WithdrawalStatus.Unverified)
            {
                _withdrawalService.Unverify(response.Id, "Unverify Withdrawal Request");
            }

            //Assert there's nothing in the "On Hold Queue"
            var withdrawalsInTheOnHoldQueue = _withdrawalService.GetWithdrawalsOnHold();
            Assert.IsEmpty(withdrawalsInTheOnHoldQueue);

            //Assert there's nothing in the "Release Queue"
            var withdrawalsInTheReleaseQueue = _withdrawalService.GetWithdrawalsForApproval();
            Assert.IsEmpty(withdrawalsInTheReleaseQueue);

            //Assert there's nothing in the "Acceptance Queue"
            var withdrawalsInTheAccQueue = _withdrawalService.GetWithdrawalsForAcceptance();
            Assert.IsEmpty(withdrawalsInTheAccQueue);

            //Assert there's nothing in the "Verification Queue"
            var withdrawalsInTheVerificationQueue = _withdrawalService.GetWithdrawalsForVerificationQueue();
            Assert.IsEmpty(withdrawalsInTheVerificationQueue);
        }
        #endregion
    }
}
