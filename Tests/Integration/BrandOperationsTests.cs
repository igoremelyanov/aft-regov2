using System;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Infrastructure.GameIntegration;
using AFT.RegoV2.Shared.Utils;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;

using AFT.UGS.Core.BaseModels.Enums;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Integration
{
    public class BrandOperationsTests : MultiprocessTestsBase
    {
        private IGameRepository _gameRepository;
        private BrandOperations _brandOperations;

        protected PlayerTestHelper PlayerTestHelper;
        protected PaymentTestHelper PaymentTestHelper;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _gameRepository = Container.Resolve<IGameRepository>();
            _brandOperations = Container.Resolve<BrandOperations>();

            PlayerTestHelper = Container.Resolve<PlayerTestHelper>();
            PaymentTestHelper = Container.Resolve<PaymentTestHelper>();
        }

        [Test]
        public async Task Can_generate_brand_token()
        {
            var brand = _gameRepository.Brands.First();
            var token = await _brandOperations.GetBrandTokenAsync(brand.Id);

            Assert.IsNotNullOrEmpty(token);
        }

        [Test]
        public async Task Can_generate_player_auth_token()
        {
            var player = PlayerTestHelper.CreatePlayer();
            WaitForPlayerRegistered(player.Id);

            var token = await _brandOperations.GetPlayerAuthTokenAsync(player.Id, TestDataGenerator.GetRandomIpAddress(), PlatformType.Mobile);

            Assert.IsNotNullOrEmpty(token);
        }

        [Test]
        public async Task Can_get_player_balance()
        {
            var amount = TestDataGenerator.GetRandomDepositAmount();
            var player = PlayerTestHelper.CreatePlayer();
            WaitForPlayerRegistered(player.Id);

            var balanceOfNew = await _brandOperations.GetPlayerBalanceAsync(player.Id, player.CurrencyCode);
            PaymentTestHelper.MakeDeposit(player.Id, amount, waitForProcessing: true);
            var balanceDeposited = await _brandOperations.GetPlayerBalanceAsync(player.Id, player.CurrencyCode);

            Assert.AreEqual(0, balanceOfNew);
            Assert.AreEqual(amount, balanceDeposited);
        }

        [Test]
        public async Task Can_fund_in()
        {
            var amount = TestDataGenerator.GetRandomDepositAmount();
            var player = PlayerTestHelper.CreatePlayer();
            WaitForPlayerRegistered(player.Id);

            var balanceFromAnswer = await _brandOperations.FundInAsync(player.Id, amount, player.CultureCode, Guid.NewGuid().ToString());
            var balanceFromRequest = await _brandOperations.GetPlayerBalanceAsync(player.Id, player.CurrencyCode);

            Assert.AreEqual(amount, balanceFromAnswer);
            Assert.AreEqual(amount, balanceFromRequest);
        }

        [Test]
        public async Task Can_fund_out()
        {
            var amount = TestDataGenerator.GetRandomDepositAmount();
            var player = PlayerTestHelper.CreatePlayer();
            WaitForPlayerRegistered(player.Id);

            PaymentTestHelper.MakeDeposit(player.Id, amount * 2, waitForProcessing: true);

            var balanceFromAnswer = await _brandOperations.FundOutAsync(player.Id, amount, player.CultureCode, Guid.NewGuid().ToString());
            var balanceFromRequest = await _brandOperations.GetPlayerBalanceAsync(player.Id, player.CurrencyCode);

            Assert.AreEqual(amount, balanceFromAnswer);
            Assert.AreEqual(amount, balanceFromRequest);
        }

        [Test]
        public async Task Fund_in_is_idempotent()
        {
            var transactionId = Guid.NewGuid().ToString();
            var amount = TestDataGenerator.GetRandomDepositAmount();
            var player = PlayerTestHelper.CreatePlayer();
            WaitForPlayerRegistered(player.Id);

            var balanceFromAnswer1 = await _brandOperations.FundInAsync(player.Id, amount, player.CultureCode, transactionId);
            var balanceFromAnswer2 = await _brandOperations.FundInAsync(player.Id, amount, player.CultureCode, transactionId);
            var balanceFromRequest = await _brandOperations.GetPlayerBalanceAsync(player.Id, player.CurrencyCode);

            Assert.AreEqual(amount, balanceFromAnswer1);
            Assert.AreEqual(amount, balanceFromAnswer2);
            Assert.AreEqual(amount, balanceFromRequest);
        }

        [Test]
        public async Task Fund_out_is_idempotent()
        {
            var transactionId = Guid.NewGuid().ToString();
            var amount = TestDataGenerator.GetRandomDepositAmount();
            var player = PlayerTestHelper.CreatePlayer();
            WaitForPlayerRegistered(player.Id);
            
            PaymentTestHelper.MakeDeposit(player.Id, amount * 2, waitForProcessing: true);

            var balanceFromAnswer1 = await _brandOperations.FundOutAsync(player.Id, amount, player.CultureCode, transactionId);
            var balanceFromAnswer2 = await _brandOperations.FundOutAsync(player.Id, amount, player.CultureCode, transactionId);
            var balanceFromRequest = await _brandOperations.GetPlayerBalanceAsync(player.Id, player.CurrencyCode);

            Assert.AreEqual(amount, balanceFromAnswer1);
            Assert.AreEqual(amount, balanceFromAnswer2);
            Assert.AreEqual(amount, balanceFromRequest);
        }

        private void WaitForPlayerRegistered(Guid playerId)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(20);
            WaitHelper.WaitResult(() =>
            {
                return _gameRepository.Players.SingleOrDefault(p => p.Id == playerId);
            }, timeout, "Timeout waiting for player with id {0} to be registered".Args(playerId));
        }
    }
}
