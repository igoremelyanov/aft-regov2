using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;


namespace AFT.RegoV2.Tests.Unit.Game
{

    internal class GameQueriesTests : AdminWebsiteUnitTestsBase
    {
        private IGameRepository _repository { get; set; }
        private IGameQueries _queries { get; set; }
        private readonly Guid _brandId = Guid.NewGuid();
        private static Random random = new Random();


        public override void BeforeEach()
        {
            base.BeforeEach();

            _repository = Container.Resolve<FakeGameRepository>();
            _queries = Container.Resolve<GameQueries>();

            PopulateFakeData();
        }

        [Test]
        public void Can_Get_Playable_Balance()
        {
            var wallets = _repository.Wallets.ToList();
            var i = random.Next(1, wallets.Count - 1);
            var randomWallet = wallets[i];

            var amount = random.Next(0, 1000);

            var bonusProxyMock = new Mock<IBonusApiProxy>();
            bonusProxyMock.Setup(m => m.GetPlayerBalanceAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(new BonusBalance
            {
                Main = amount,
                Bonus = amount / 2
            });
            _queries = new GameQueries(_repository, new GameWalletQueries(_repository));

            var playerId = randomWallet.PlayerId;
            
            var pamount = _queries.GetPlayableBalance(playerId);

            Assert.AreEqual(randomWallet.Balance, pamount.Balance);
        }

        [Test]
        public void Can_Get_Playable_Balances_Nultiple_Players()
        {
            var wallets = _repository.Wallets.ToList();

            var players = new List<Guid>();

            var expected = new Dictionary<Guid, decimal>();
            const int count = 3;
            for (var i = 0; i < count; i += 1)
            {
                wallets[i].Balance = (i + 1)*100;
                players.Add(wallets[i].PlayerId);

                expected.Add(wallets[i].PlayerId, wallets[i].Balance);
            }

            var balances = _queries.GetPlayableBalances(players);

            Assert.AreEqual(count, balances.Count);
            foreach (var balance in balances)
            {
                Assert.Contains(balance.Key, players);
                Assert.AreEqual(expected[balance.Key], balance.Value);
            }
        }



        #region Preparation
        private void PopulateFakeData()
        {
            FakeGameProviders();
            FakePlayers();

        }

        private void FakePlayers(int count = 5)
        {
            for (var i = 0; i < count; i += 1)
            {
                var id = Guid.NewGuid();
                _repository.Players.Add(new Core.Game.Interface.Data.Player
                {
                    Id = id,
                    BrandId = _brandId,
                    DisplayName = Guid.NewGuid().ToString().Substring(0, 4),
                    Name = Guid.NewGuid().ToString().Substring(0, 4)
                });
                FakeWalletsForPlayer(id);
            }
        }

        private void FakeWalletsForPlayer(Guid playerId)
        {
            _repository.Wallets.Add(new Wallet
            {
                Id = Guid.NewGuid(),
                Balance = 0,
                PlayerId = playerId,
            });
        }

        private void FakeGameProviders(int count = 5)
        {
            for (var i = 0; i < count; i += 1)
            {
                var id = Guid.NewGuid();
                _repository.GameProviders.Add(new GameProvider
                {
                    Id = id,
                    Name = Guid.NewGuid().ToString().Substring(0, 3),
                    Games = FakeGames(id)
                });
                
            }
        }

        private List<Core.Game.Interface.Data.Game> FakeGames(Guid gpid, int count = 5)
        {
            var result = new List<Core.Game.Interface.Data.Game>();

            for (var i = 0; i < count; i += 1)
            {
                var g = new Core.Game.Interface.Data.Game
                {
                    Id = Guid.NewGuid(),
                    GameProviderId = gpid,
                    Name = Guid.NewGuid().ToString().Substring(0, 6)
                };
                result.Add(g);
                _repository.Games.Add(g);
            }
            return result;
        }
        #endregion
    }
}
