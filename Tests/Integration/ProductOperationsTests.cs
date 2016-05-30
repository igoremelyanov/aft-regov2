using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Infrastructure.GameIntegration;
using AFT.RegoV2.Shared.Utils;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;

using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Integration
{
    public class ProductOperationsTests : MultiprocessTestsBase
    {
        private IGameRepository _gameRepository;
        private ProductOperations _productOperations;

        protected PlayerTestHelper PlayerTestHelper;
        protected GamesTestHelper GamesTestHelper;
        protected BrandTestHelper BrandTestHelper;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _gameRepository = Container.Resolve<IGameRepository>();
            _productOperations = Container.Resolve<ProductOperations>();

            PlayerTestHelper = Container.Resolve<PlayerTestHelper>();
            GamesTestHelper = Container.Resolve<GamesTestHelper>();
            BrandTestHelper = Container.Resolve<BrandTestHelper>();
        }

        [Test]
        public async Task Can_generate_lobby_token()
        {
            var lobby = _gameRepository.Lobbies.First();
            var token = await _productOperations.GetLobbyTokenAsync(lobby.Id);

            Assert.IsNotNullOrEmpty(token);
        }

        [Test]
        public async Task Can_get_lobby_data()
        {
            var brandId = new Guid("00000000-0000-0000-0000-000000000138");
            var brand = _gameRepository.Brands.Include(x => x.BrandLobbies).Single(x => x.Id == brandId);
            var lobbyId = brand.BrandLobbies.First().LobbyId;
            
            var player = PlayerTestHelper.CreatePlayer(true, brand.Id);
            var playerId = player.Id;
            WaitForPlayerRegistered(playerId);

            var data = await _productOperations.GetProductsForPlayerAsync(
                lobbyId, 
                playerId, 
                "http://example.org/", 
                TestDataGenerator.GetRandomIpAddress(), 
                string.Empty);

            Assert.AreEqual(playerId, data.player.id);
            Assert.AreEqual(lobbyId, data.products.lobbyid);
            Assert.AreNotEqual(0, data.products.groups.Count());
            //Assert.AreNotEqual(0, data.products.providers.Count());
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
