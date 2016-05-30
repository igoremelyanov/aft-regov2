using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AFT.RegoV2.Core.Game.Interface.Exceptions;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;

using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Game
{
    internal class GamesCommandsTests : AdminWebsiteUnitTestsBase
    {
        private IGameCommands _commands;
        private Mock<IEventBus> _eventBusMock;
        private Mock<IGameWalletOperations> _gameWalletsOperationsMock;
        private FakeGameRepository _repository;
        private Guid _gameId;
        private Guid _gpId;
        private string _gpCode;
        private Guid _brandId;
        private Guid _playerId;
        private GameActionContext _GameActionContext 
        {
            get { return new GameActionContext {GameProviderCode = _gpCode}; }
        }

        public override void BeforeEach()
        {
            base.BeforeEach();

            _repository = new FakeGameRepository();
            _eventBusMock = new Mock<IEventBus>();
            _gameWalletsOperationsMock = new Mock<IGameWalletOperations>();

            _gameWalletsOperationsMock.Setup(
                t => t.PlaceBetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>()))
                .Returns(Task.FromResult(Guid.NewGuid()));
            _commands = new GameCommands(_repository, _gameWalletsOperationsMock.Object, _eventBusMock.Object);

            _gameId = Guid.NewGuid();
            _gpId = Guid.NewGuid();
            _gpCode = Guid.NewGuid().ToString();
            _brandId = Guid.NewGuid();
            _playerId = Guid.NewGuid();

            _repository.GameProviders.Add(new GameProvider
            {
                Id = _gpId,
                Code = _gpCode
            });
            _repository.Games.Add(new Core.Game.Interface.Data.Game
            {
                Id = _gameId,
                GameProviderId = _gpId,
                ExternalId = "TEST-GAME"
            });

            _repository.Brands.Add(new Core.Game.Interface.Data.Brand
            {
                Id = _brandId,
                TimezoneId = TimeZoneInfo.Utc.Id
            });

            _repository.Players.Add(new Core.Game.Interface.Data.Player()
            {
                Id = _playerId,
                BrandId = _brandId
            });
        }

        [TestCase("Pacific Standard Time")]
        [TestCase("Eastern Standard Time")]
        public async Task Bet_Placed_With_Brand_Timezone(string timezoneId)
        {
            // Arrange
            var brandId = Guid.NewGuid();
            _repository.Brands.Add(new Core.Game.Interface.Data.Brand
            {
                Id = brandId,
                TimezoneId = timezoneId
            });

            var playerId = Guid.NewGuid();
            _repository.Players.Add(new Core.Game.Interface.Data.Player()
            {
                Id = playerId,
                BrandId = brandId
            });
            
            var placeBetAction = GenerateRandomGameAction();

            // Act
            await _commands.PlaceBetAsync(placeBetAction, _GameActionContext, playerId);

            var winBetAction = GenerateRandomGameAction(Guid.NewGuid().ToString());
            winBetAction.TransactionReferenceId = placeBetAction.ExternalTransactionId;
            winBetAction.RoundId = placeBetAction.RoundId;
            winBetAction.Amount = 25;

            await _commands.WinBetAsync(winBetAction, _GameActionContext);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            var timeComparePattern = "yyyy-MM-dd HH:mm";
            actualRound.Should().NotBeNull();
            actualRound.Data.CreatedOn.ToString(timeComparePattern).Should()
                .Be(DateTimeOffset.UtcNow.ToBrandOffset(timezoneId).ToString(timeComparePattern));
            actualRound.Data.ClosedOn.Value.ToString(timeComparePattern).Should()
                .Be(DateTimeOffset.UtcNow.ToBrandOffset(timezoneId).ToString(timeComparePattern));

            var actualGameAction = actualRound.Data.GameActions[0];

            actualGameAction.GameActionType.Should().Be(GameActionType.Placed);
            actualGameAction.Timestamp.ToString(timeComparePattern).Should()
                .Be(DateTimeOffset.UtcNow.ToBrandOffset(timezoneId).ToString(timeComparePattern));
        }

        [Test]
        public async Task Can_Place_Bet()
        {
            // Arrange
            var placeBetAction = GenerateRandomGameAction();
            var brandId = _brandId;
            var playerId = _playerId;

            // Act
            await _commands.PlaceBetAsync(placeBetAction, _GameActionContext, playerId);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            actualRound.Should().NotBeNull();
            actualRound.Data.PlayerId.Should().Be(playerId);
            actualRound.Data.BrandId.Should().Be(brandId);
            actualRound.Data.GameId.Should().Be(_gameId);
            actualRound.Data.Status.Should().Be(RoundStatus.Open);
            actualRound.WonAmount.Should().Be(0);
            actualRound.AdjustedAmount.Should().Be(0);
            actualRound.Amount.Should().Be(placeBetAction.Amount);
            actualRound.Data.GameActions.Should().NotBeEmpty();

            var actualGameAction = actualRound.Data.GameActions[0];

            actualGameAction.GameActionType.Should().Be(GameActionType.Placed);
            actualGameAction.WalletTransactionId.Should().NotBeEmpty();
            actualGameAction.ExternalTransactionId.Should().Be(placeBetAction.ExternalTransactionId);
            actualGameAction.ExternalBatchId.Should().BeNull();
            actualGameAction.Description.Should().Be(placeBetAction.Description);
            actualGameAction.Amount.Should().Be(-placeBetAction.Amount);
            actualGameAction.Round.Id.Should().Be(actualRound.Data.Id);
            
            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetPlaced>()));
        }

        [Test]
        public async Task Can_Place_Bet_Twice()
        {
            // Arrange
            var placeBetAction = GenerateRandomGameAction();
            var brandId = _brandId;
            var playerId = _playerId;

            // Act
            await _commands.PlaceBetAsync(placeBetAction, _GameActionContext, playerId); // place initial bet

            var secondPlaceBetAction = GenerateRandomGameAction();
            secondPlaceBetAction.RoundId = placeBetAction.RoundId;
            secondPlaceBetAction.Amount = 20;
            await _commands.PlaceBetAsync(secondPlaceBetAction, _GameActionContext, playerId);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            actualRound.Should().NotBeNull();
            actualRound.Data.PlayerId.Should().Be(playerId);
            actualRound.Data.BrandId.Should().Be(brandId);
            actualRound.Data.GameId.Should().Be(_gameId);
            actualRound.Data.Status.Should().Be(RoundStatus.Open);
            actualRound.WonAmount.Should().Be(0);
            actualRound.AdjustedAmount.Should().Be(0);
            actualRound.Amount.Should().Be(placeBetAction.Amount + secondPlaceBetAction.Amount);
            actualRound.Data.GameActions.Count.Should().Be(2);

            var actualGameAction = actualRound.Data.GameActions[1];

            actualGameAction.GameActionType.Should().Be(GameActionType.Placed);
            actualGameAction.WalletTransactionId.Should().NotBeEmpty();
            actualGameAction.ExternalTransactionId.Should().Be(secondPlaceBetAction.ExternalTransactionId);
            actualGameAction.ExternalBatchId.Should().BeNull();
            actualGameAction.Description.Should().Be(secondPlaceBetAction.Description);
            actualGameAction.Amount.Should().Be(-secondPlaceBetAction.Amount);
            actualGameAction.Round.Id.Should().Be(actualRound.Data.Id);

            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetPlaced>()));
        }

        
        [Test]
        public void Cannot_Place_Duplicate_Bet()
        {
            // Arrange
            var externalTransactionId = Guid.NewGuid().ToString();

            // add a transaction
            _repository.Rounds.Add(new Round 
            {
                GameActions = new List<GameAction>
                {
                    new GameAction
                    {
                        ExternalTransactionId = externalTransactionId
                    }
                }
            });

            var placeBetAction = GenerateRandomGameAction(externalTransactionId);
            var playerId = _playerId;

            // Act
            Func<Task> act = async () => await _commands.PlaceBetAsync(placeBetAction, _GameActionContext, playerId);

            // Assert
            act.ShouldThrow<DuplicateGameActionException>();
        }

        [Test]
        public async Task Can_Win_Bet()
        {
            // Arrange
            var placeBetAction = GenerateRandomGameAction();
            var playerId = _playerId;

            await _commands.PlaceBetAsync(placeBetAction, _GameActionContext, playerId); // place initial bet

            var winBetAction = GenerateRandomGameAction(Guid.NewGuid().ToString());

            winBetAction.TransactionReferenceId = placeBetAction.ExternalTransactionId;
            winBetAction.RoundId = placeBetAction.RoundId;
            winBetAction.Amount = 25;

            _gameWalletsOperationsMock.Setup(
                t => t.WinBetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>()))
                .Returns(Task.FromResult(Guid.NewGuid()));

            // Act
            await _commands.WinBetAsync(winBetAction, _GameActionContext);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            actualRound.WonAmount.Should().Be(winBetAction.Amount);
            actualRound.AdjustedAmount.Should().Be(0);
            actualRound.Amount.Should().Be(placeBetAction.Amount);
            actualRound.Data.GameActions.Count.Should().Be(2);

            var actualWinBetGameAction = actualRound.Data.GameActions[1];

            actualWinBetGameAction.GameActionType.Should().Be(GameActionType.Won);
            actualWinBetGameAction.WalletTransactionId.Should().NotBeEmpty();
            actualWinBetGameAction.ExternalTransactionId.Should().Be(winBetAction.ExternalTransactionId);
            actualWinBetGameAction.ExternalBatchId.Should().BeNull();
            actualWinBetGameAction.Description.Should().Be(winBetAction.Description);
            actualWinBetGameAction.Amount.Should().Be(winBetAction.Amount);
            actualWinBetGameAction.Round.Id.Should().Be(actualRound.Data.Id);


            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetWon>()));
        }

        [Test]
        public void Cannot_Win_NonExisting_Bet()
        {
            // Arrange
            var winBetAction = GenerateRandomGameAction();

            // Act
            Func<Task> act = async () => await _commands.WinBetAsync(winBetAction, _GameActionContext);

            // Assert
            act.ShouldThrow<RoundNotFoundException>();
        }

        [Test]
        public async Task Can_Lose_Bet()
        {
            // Arrange
            var placeBetAction = GenerateRandomGameAction();
            var playerId = _playerId;

            await _commands.PlaceBetAsync(placeBetAction, _GameActionContext, playerId); // place initial bet

            var loseBetAction = GenerateRandomGameAction(Guid.NewGuid().ToString());

            loseBetAction.TransactionReferenceId = placeBetAction.ExternalTransactionId;
            loseBetAction.RoundId = placeBetAction.RoundId;
            loseBetAction.Amount = 0;
            loseBetAction.WalletTransactionId = Guid.Empty;


            _gameWalletsOperationsMock.Setup(
                t => t.LoseBetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult(Guid.NewGuid()));

            // Act
            await _commands.LoseBetAsync(loseBetAction, _GameActionContext);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            
            actualRound.WonAmount.Should().Be(0);
            actualRound.AdjustedAmount.Should().Be(0);
            actualRound.Amount.Should().Be(placeBetAction.Amount);
            actualRound.Data.GameActions.Count.Should().Be(2);

            var actualLoseBetGameAction = actualRound.Data.GameActions[1];

            actualLoseBetGameAction.GameActionType.Should().Be(GameActionType.Lost);
            actualLoseBetGameAction.WalletTransactionId.Should().BeEmpty();
            actualLoseBetGameAction.ExternalTransactionId.Should().Be(loseBetAction.ExternalTransactionId);
            actualLoseBetGameAction.ExternalBatchId.Should().BeNull();
            actualLoseBetGameAction.Description.Should().Be(loseBetAction.Description);
            actualLoseBetGameAction.Amount.Should().Be(loseBetAction.Amount);
            actualLoseBetGameAction.Round.Id.Should().Be(actualRound.Data.Id);


            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetLost>()));
        }

        [Test]
        public void Cannot_Lose_Bet_With_Nonzero_Amount()
        {
            // Arrange
            var loseBetAction = GenerateRandomGameAction(Guid.NewGuid().ToString());
            loseBetAction.Amount = 10; // non-zero
            
            // Act
            Func<Task> act = async () => await _commands.LoseBetAsync(loseBetAction, _GameActionContext);

            // Assert
            act.ShouldThrow<LoseBetAmountMustBeZeroException>();
        }

        [Test]
        public void Cannot_Lose_Nonexisting_Bet()
        {
            // Arrange
            var loseBetAction = GenerateRandomGameAction(Guid.NewGuid().ToString());
            loseBetAction.Amount = 0; // must be zero

            // Act
            Func<Task> act = async () => await _commands.LoseBetAsync(loseBetAction, _GameActionContext);

            // Assert
            act.ShouldThrow<RoundNotFoundException>();
        }

        [Test]
        public void Can_Free_Bet()
        {
            // Arrange
            var freeBetAction = GenerateRandomGameAction();
            var brandId = _brandId;
            var playerId = _playerId;

            _gameWalletsOperationsMock.Setup(
                t => t.FreeBetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>()))
                .Returns(Task.FromResult(Guid.NewGuid()));

            // Act
            _commands.FreeBetAsync(freeBetAction, _GameActionContext, playerId);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == freeBetAction.RoundId);

            actualRound.Should().NotBeNull();
            actualRound.Data.PlayerId.Should().Be(playerId);
            actualRound.Data.BrandId.Should().Be(brandId);
            actualRound.Data.GameId.Should().Be(_gameId);

            actualRound.Data.Status.Should().Be(RoundStatus.Closed);
            actualRound.WonAmount.Should().Be(freeBetAction.Amount);
            actualRound.AdjustedAmount.Should().Be(0);
            actualRound.Amount.Should().Be(0);
            actualRound.Data.GameActions.Should().NotBeEmpty();

            var actualGameAction = actualRound.Data.GameActions[0];

            actualGameAction.GameActionType.Should().Be(GameActionType.Free);
            actualGameAction.WalletTransactionId.Should().NotBeEmpty();
            actualGameAction.ExternalTransactionId.Should().Be(freeBetAction.ExternalTransactionId);
            actualGameAction.ExternalBatchId.Should().BeNull();
            actualGameAction.Description.Should().Be(freeBetAction.Description);
            actualGameAction.Amount.Should().Be(freeBetAction.Amount);
            actualGameAction.Round.Id.Should().Be(actualRound.Data.Id);

            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetPlacedFree>()));
        }

        [Test]
        public async Task Can_Adjust_Bet()
        {
            // Arrange
            var placeBetAction = GenerateRandomGameAction();
            var playerId = _playerId;

            await _commands.PlaceBetAsync(placeBetAction, _GameActionContext, playerId); // place initial bet

            var adjustingAction = GenerateRandomGameAction(Guid.NewGuid().ToString());

            adjustingAction.TransactionReferenceId = placeBetAction.ExternalTransactionId;
            adjustingAction.RoundId = placeBetAction.RoundId;
            adjustingAction.Amount = placeBetAction.Amount + 20;

            _gameWalletsOperationsMock.Setup(
                t => t.AdjustBetTransaction(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Guid.NewGuid());

            // Act
            _commands.AdjustTransaction(adjustingAction, _GameActionContext);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            actualRound.WonAmount.Should().Be(0);
            actualRound.AdjustedAmount.Should().Be(adjustingAction.Amount);
            actualRound.Amount.Should().Be( placeBetAction.Amount);
            actualRound.Data.GameActions.Count.Should().Be(2);

            var actualAdjustmentGameAction = actualRound.Data.GameActions[1];

            actualAdjustmentGameAction.GameActionType.Should().Be(GameActionType.Adjustment);
            actualAdjustmentGameAction.WalletTransactionId.Should().NotBeEmpty();
            actualAdjustmentGameAction.ExternalTransactionId.Should().Be(adjustingAction.ExternalTransactionId);
            actualAdjustmentGameAction.ExternalTransactionReferenceId.Should().Be(placeBetAction.ExternalTransactionId);
            actualAdjustmentGameAction.ExternalBatchId.Should().BeNull();
            actualAdjustmentGameAction.Description.Should().Be(adjustingAction.Description);
            actualAdjustmentGameAction.Amount.Should().Be(adjustingAction.Amount);
            actualAdjustmentGameAction.Round.Id.Should().Be(actualRound.Data.Id);


            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetAdjusted>()));
        }

        [Test]
        public async Task Can_Adjust_BetTransaction()
        {
            // Arrange
            var placeBetAction = GenerateRandomGameAction();
            var playerId = _playerId;

            await _commands.PlaceBetAsync(placeBetAction, _GameActionContext, playerId); // place initial bet

            var adjustingBetAction = GenerateRandomGameAction(Guid.NewGuid().ToString());

            adjustingBetAction.RoundId = placeBetAction.RoundId;
            adjustingBetAction.Amount = placeBetAction.Amount + 20;
            adjustingBetAction.TransactionReferenceId = placeBetAction.ExternalTransactionId;

            _gameWalletsOperationsMock.Setup(
                t => t.AdjustBetTransaction(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Guid.NewGuid());
            // Act
            _commands.AdjustTransaction(adjustingBetAction, _GameActionContext);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            actualRound.WonAmount.Should().Be(0);
            actualRound.AdjustedAmount.Should().Be(adjustingBetAction.Amount);
            actualRound.Amount.Should().Be(placeBetAction.Amount);
            actualRound.Data.GameActions.Count.Should().Be(2);

            var actualAdjustmentGameAction = actualRound.Data.GameActions[1];

            actualAdjustmentGameAction.GameActionType.Should().Be(GameActionType.Adjustment);
            actualAdjustmentGameAction.WalletTransactionId.Should().NotBeEmpty();
            actualAdjustmentGameAction.ExternalTransactionId.Should().Be(adjustingBetAction.ExternalTransactionId);
            actualAdjustmentGameAction.ExternalTransactionReferenceId.Should().Be(placeBetAction.ExternalTransactionId);
            actualAdjustmentGameAction.ExternalBatchId.Should().BeNull();
            actualAdjustmentGameAction.Description.Should().Be(adjustingBetAction.Description);
            actualAdjustmentGameAction.Amount.Should().Be(adjustingBetAction.Amount);
            actualAdjustmentGameAction.Round.Id.Should().Be(actualRound.Data.Id);


            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetAdjusted>()));
        }

        [Test]
        public void Cannot_Adjust_Nonexisting_Bet()
        {
            // Arrange
            var adjustingBetAction = GenerateRandomGameAction();

            // Act
            Action act = () => _commands.AdjustTransaction(adjustingBetAction, _GameActionContext);

            // Assert
            act.ShouldThrow<RoundNotFoundException>();
        }



        [Test]
        public async Task Can_Cancel_BetTransaction()
        {
            // Arrange
            var placeBetAction = GenerateRandomGameAction();
            var playerId = _playerId;

            await _commands.PlaceBetAsync(placeBetAction, _GameActionContext, playerId); // place initial bet

            var cancelBetAction = GenerateRandomGameAction(Guid.NewGuid().ToString());

            cancelBetAction.RoundId = placeBetAction.RoundId;
            cancelBetAction.TransactionReferenceId = placeBetAction.ExternalTransactionId;


            _gameWalletsOperationsMock.Setup(
                t => t.CancelBetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(Guid.NewGuid()));

            // Act
            await _commands.CancelTransactionAsync(cancelBetAction, _GameActionContext);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            actualRound.WonAmount.Should().Be(0);
            actualRound.AdjustedAmount.Should().Be(cancelBetAction.Amount);
            actualRound.Amount.Should().Be(placeBetAction.Amount);
            actualRound.Data.GameActions.Count.Should().Be(2);

            var actualCancelBetAction = actualRound.Data.GameActions[1];

            actualCancelBetAction.GameActionType.Should().Be(GameActionType.Cancel);
            actualCancelBetAction.WalletTransactionId.Should().NotBeEmpty();
            actualCancelBetAction.ExternalTransactionId.Should().Be(cancelBetAction.ExternalTransactionId);
            actualCancelBetAction.ExternalTransactionReferenceId.Should().Be(placeBetAction.ExternalTransactionId);
            actualCancelBetAction.ExternalBatchId.Should().BeNull();
            actualCancelBetAction.Description.Should().Be(cancelBetAction.Description);
            actualCancelBetAction.Amount.Should().Be(placeBetAction.Amount);
            actualCancelBetAction.Round.Id.Should().Be(actualRound.Data.Id);


            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetCancelled>()));
        }

        [Test]
        public void Cannot_Cancel_Nonexisting_Bet()
        {
            // Arrange
            var cancelBetAction = GenerateRandomGameAction();

            // Act
            Action act = () => _commands.AdjustTransaction(cancelBetAction, _GameActionContext);

            // Assert
            act.ShouldThrow<RoundNotFoundException>();
        } 
        
        // Helpers
        private GameActionData GenerateRandomGameAction(string externalTxId = null)
        {
            return new GameActionData
            {
                Amount = 10,
                RoundId = Guid.NewGuid().ToString(),
                CurrencyCode = TestDataGenerator.GetRandomAlphabeticString(3),
                Description = TestDataGenerator.GetRandomAlphabeticString(100),
                ExternalTransactionId = externalTxId ?? Guid.NewGuid().ToString(),
                ExternalGameId = "TEST-GAME",
                WalletTransactionId = Guid.NewGuid()
            };
        }
    }
}
