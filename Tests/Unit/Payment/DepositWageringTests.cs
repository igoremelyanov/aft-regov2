using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using WinService.Workers;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    public class DepositWageringTests : AdminWebsiteUnitTestsBase
    {
        private GamesTestHelper _gamesTestHelper;
        private FakePaymentRepository _paymentRepository;
        private PaymentTestHelper _paymentTestHelper;
        private PlayerQueries _playerQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();
            
            _playerQueries = Container.Resolve<PlayerQueries>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();

            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.PopulatePermissions();
            securityTestHelper.CreateAndSignInSuperAdmin();

            var brandHelper = Container.Resolve<BrandTestHelper>();
            brandHelper.CreateActiveBrandWithProducts();

            Container.Resolve<PlayerTestHelper>().CreatePlayer();
            Container.Resolve<PaymentWorker>().Start();
        }

        [Test]
        public void Wagering_amount_setted_sucessfully()
        {
            var player = _playerQueries.GetPlayers().ToList().First();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);

            var deposit = _paymentRepository.OfflineDeposits.FirstOrDefault(x => x.PlayerId == player.Id);

            Assert.NotNull(deposit);
            Assert.AreEqual(deposit.DepositWagering, 1000);
        }

        [Test]
        public async Task Wagering_amount_recalculated_sucessfully_while_player_bets()
        {
            var player = _playerQueries.GetPlayers().ToList().First();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            await _gamesTestHelper.PlaceAndWinBet(100, 100, player.Id);

            var deposit = _paymentRepository.OfflineDeposits.FirstOrDefault(x => x.PlayerId == player.Id);

            Assert.NotNull(deposit);
            Assert.AreEqual(deposit.DepositWagering, 900);
        }

        [Test]
        public async Task Cannot_make_wagering_requirement_less_than_zero()
        {
            var player = _playerQueries.GetPlayers().ToList().First();
            _paymentTestHelper.MakeDeposit(player.Id, 200);
            await _gamesTestHelper.PlaceAndWinBet(50, 100, player.Id);
            await _gamesTestHelper.PlaceAndWinBet(200, 100, player.Id);

            var deposit = _paymentRepository.OfflineDeposits.FirstOrDefault(x => x.PlayerId == player.Id);

            Assert.NotNull(deposit);
            Assert.AreEqual(deposit.DepositWagering, 0);
        }
    }
}