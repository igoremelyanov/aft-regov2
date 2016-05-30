using System;
using AFT.RegoV2.Core.Game.Interface.Exceptions;
using AFT.RegoV2.Core.Game.Interface.Data;
using NUnit.Framework;
using Round = AFT.RegoV2.Core.Game.Entities.Round;

namespace AFT.RegoV2.GameApi.Tests.Unit.Bets
{
    [Category("Unit")]
    internal class BetTests
    {
        const string PlayerIpAddress = "127.0.0.0";

        [Test]
        public void Can_Create_Round()
        {
            Guid brandId = Guid.NewGuid(), gameId = Guid.NewGuid(), playerId = Guid.NewGuid();
            var round = CreateRound(gameId, playerId, brandId);

            Assert.AreEqual(brandId, round.Data.BrandId);
            Assert.AreEqual(gameId, round.Data.GameId);
            Assert.AreEqual(playerId, round.Data.PlayerId);
        }

        [Test]
        public void Can_Place_Bet()
        {
            Guid brandId = Guid.NewGuid(), gameId = Guid.NewGuid(), playerId = Guid.NewGuid();
            var round = CreateRound(gameId, playerId, brandId);
            var walletTxId = Guid.NewGuid();

            round.Place(1234.56m, 
                "test", 
                walletTxId, 
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString());

            Assert.AreEqual(1, round.Data.GameActions.Count);
            Assert.AreEqual(RoundStatus.Open, round.Data.Status);
            Assert.AreEqual(1234.56m, round.Amount);

            var gameAction = round.Data.GameActions[0];

            Assert.AreEqual(-1234.56m, gameAction.Amount);
            Assert.AreEqual("test", gameAction.Description);
            Assert.AreEqual(GameActionType.Placed, gameAction.GameActionType);
            Assert.AreEqual(walletTxId, gameAction.WalletTransactionId);
        }

        [Test]
        public void Can_Win()
        {
            Guid brandId = Guid.NewGuid(), gameId = Guid.NewGuid(), playerId = Guid.NewGuid();
            var round = CreateRound(gameId, playerId, brandId);
            var walletTxId = Guid.NewGuid();

            var placeBetTxId = Guid.NewGuid().ToString();
            round.Place(1234.56m, "test", walletTxId, placeBetTxId, Guid.NewGuid().ToString());

            walletTxId = Guid.NewGuid();
            round.Win(10000m, 
                "won", 
                walletTxId, 
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(), // externalBetId
                placeBetTxId);

            Assert.AreEqual(2, round.Data.GameActions.Count);
            Assert.AreEqual(RoundStatus.Closed, round.Data.Status);
            Assert.AreEqual(1234.56m, round.Amount);
            Assert.AreEqual(10000m, round.WonAmount);

            var gameAction = round.Data.GameActions[1];

            Assert.AreEqual(10000m, gameAction.Amount);
            Assert.AreEqual("won", gameAction.Description);
            Assert.AreEqual(GameActionType.Won, gameAction.GameActionType);
            Assert.AreEqual(walletTxId, gameAction.WalletTransactionId);
        }


        [Test]
        public void Can_Free_Bet()
        {
            Guid brandId = Guid.NewGuid(), gameId = Guid.NewGuid(), playerId = Guid.NewGuid();
            var round = CreateRound(gameId, playerId, brandId);
            var walletTxId = Guid.NewGuid();

            round.Free(10000m, "freebet", walletTxId, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            
            Assert.AreEqual(1, round.Data.GameActions.Count);
            Assert.AreEqual(RoundStatus.Closed, round.Data.Status);
            Assert.AreEqual(0, round.Amount);
            Assert.AreEqual(10000m, round.WonAmount);

            var gameAction = round.Data.GameActions[0];

            Assert.AreEqual(10000m, gameAction.Amount);
            Assert.AreEqual("freebet", gameAction.Description);
            Assert.AreEqual(GameActionType.Free, gameAction.GameActionType);
            Assert.AreEqual(walletTxId, gameAction.WalletTransactionId);
        }

        [Test]
        public void Can_Lose()
        {
            Guid brandId = Guid.NewGuid(), gameId = Guid.NewGuid(), playerId = Guid.NewGuid();
            var round = CreateRound(gameId, playerId, brandId);
            var walletTxId = Guid.NewGuid();

            var placeBetTxId = Guid.NewGuid().ToString();
            round.Place(1234.56m, "test", walletTxId, placeBetTxId, Guid.NewGuid().ToString());

            round.Lose(String.Empty, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), placeBetTxId);

            Assert.AreEqual(RoundStatus.Closed, round.Data.Status);
        }

        [Test]
        [ExpectedException(typeof(GameActionNotFoundException))]
        public void Can_Adjust_New_GameAction()
        {
            Guid brandId = Guid.NewGuid(), gameId = Guid.NewGuid(), playerId = Guid.NewGuid();
            var round = CreateRound(gameId, playerId, brandId);
            var walletTxId = Guid.NewGuid();

            round.Adjust(-100m, "adjust description", walletTxId,
                externalTransactionId: Guid.NewGuid().ToString(),
                externalBetId:Guid.NewGuid().ToString(),
                externalTransactionReferenceId: Guid.NewGuid().ToString());
        }

        [Test]
        public void Can_Adjust_Existing_GameAction()
        {
            Guid brandId = Guid.NewGuid(), gameId = Guid.NewGuid(), playerId = Guid.NewGuid();
            var round = CreateRound(gameId, playerId, brandId);
            var walletTxId = Guid.NewGuid();

            var placeBetTxId = Guid.NewGuid().ToString();
            round.Place(1234.56m, "test", walletTxId, placeBetTxId, Guid.NewGuid().ToString());

            walletTxId = Guid.NewGuid();
            round.Adjust(1000m, "adjust", walletTxId, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), placeBetTxId);
            
            Assert.AreEqual(2, round.Data.GameActions.Count);
            Assert.AreEqual(RoundStatus.Open, round.Data.Status);
            Assert.AreEqual(1234.56m, round.Amount);
            Assert.AreEqual(1000m, round.AdjustedAmount);

            var gameAction = round.Data.GameActions[1];

            Assert.AreEqual(1000m, gameAction.Amount);
            Assert.AreEqual("adjust", gameAction.Description);
            Assert.AreEqual(GameActionType.Adjustment, gameAction.GameActionType);
            Assert.AreEqual(walletTxId, gameAction.WalletTransactionId);
        }

        [Test]
        [ExpectedException(typeof(GameActionNotFoundException))]
        public void Can_Cancel_Nonexisting_GameAction()
        {
            Guid brandId = Guid.NewGuid(), gameId = Guid.NewGuid(), playerId = Guid.NewGuid();
            var round = CreateRound(gameId, playerId, brandId);
            var walletTxId = Guid.NewGuid();

            round.Cancel(100m, "cancel", walletTxId, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        [Test]
        public void Can_Cancel_Existing_GameAction()
        {
            Guid brandId = Guid.NewGuid(), gameId = Guid.NewGuid(), playerId = Guid.NewGuid();
            var round = CreateRound(gameId, playerId, brandId);
            var walletTxId = Guid.NewGuid();

            var placeBetTxId = Guid.NewGuid().ToString();

            round.Place(1234.56m, "test", walletTxId, placeBetTxId, Guid.NewGuid().ToString());

            walletTxId = Guid.NewGuid();
            round.Cancel(1234.56m, "cancel", walletTxId, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), placeBetTxId);

            Assert.AreEqual(2, round.Data.GameActions.Count);
            Assert.AreEqual(RoundStatus.Open, round.Data.Status);
            Assert.AreEqual(1234.56m, round.Amount);

            var gameAction = round.Data.GameActions[1];

            Assert.AreEqual(1234.56, gameAction.Amount);
            Assert.AreEqual("cancel", gameAction.Description);
            Assert.AreEqual(GameActionType.Cancel, gameAction.GameActionType);
            Assert.AreEqual(walletTxId, gameAction.WalletTransactionId);
        }

        private Round CreateRound(Guid gameId, Guid playerId, Guid brandId)
        {
            var brand = new Brand
            {
                Id = brandId,
                TimezoneId = TimeZoneInfo.Utc.Id
            };

            return new Round(Guid.NewGuid().ToString(), gameId, playerId, brand);
        }
    }

}