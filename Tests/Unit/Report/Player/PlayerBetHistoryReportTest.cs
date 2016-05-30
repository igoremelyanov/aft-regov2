using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.AdminWebsite.Controllers;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using WinService.Workers;
using PlayerData = AFT.RegoV2.Core.Common.Data.Player.Player;

namespace AFT.RegoV2.Tests.Unit.Report.Player
{
    internal class PlayerBetHistoryReportTest : ReportsTestsBase
    {
        private IReportRepository _reportRepository;
        private GamesTestHelper _gamesTestHelper;
        private ReportQueries _reportQueries;

        private Random _random;
        private PlayerData _player;
        private Core.Game.Interface.Data.Game _game;

        public override void BeforeEach()
        {
            base.BeforeEach();
            
            Container.Resolve<PaymentWorker>().Start();
            _reportRepository = Container.Resolve<IReportRepository>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
            _reportQueries = Container.Resolve<ReportQueries>();
            _random = new Random();

            _player = Container.Resolve<PlayerTestHelper>().CreatePlayer();
            PaymentTestHelper.MakeDeposit(_player.Id, 1000000);

            _game = _gamesTestHelper.GetMainWalletGame(_player.Id);
        }

        protected override void StartWorkers()
        {
            Container.Resolve<PlayerBetHistoryReportWorker>().Start();
        }

        [Test]
        public async Task Can_process_win_bet()
        {
            // Arrange
            var betAmount = RandomBetAmount();

            // Act
            await _gamesTestHelper.PlaceAndWinBet(betAmount, betAmount, _player.Id);

            // Assert
            Assert.AreEqual(1, _reportRepository.PlayerBetHistoryRecords.Count());
            var record = _reportRepository.PlayerBetHistoryRecords.Single();
            Assert.AreEqual(betAmount, record.BetAmount);
            Assert.AreEqual(CurrentBrand.Name, record.Brand);
            Assert.Less(DateTimeOffset.Now.AddDays(-2), record.DateBet);
            Assert.AreEqual(_game.Name, record.GameName);
            Assert.AreEqual(_player.Username, record.LoginName);
            Assert.AreEqual(_player.IpAddress ?? LocalIPAddress, record.UserIP);
            Assert.AreEqual(0, record.TotalWinLoss);
        }

        [Test]
        public async Task Can_process_lose_bet()
        {
            // Arrange
            var betAmount = RandomBetAmount();

            // Act
            await _gamesTestHelper.PlaceAndLoseBet(betAmount, _player.Id);

            // Assert
            Assert.AreEqual(1, _reportRepository.PlayerBetHistoryRecords.Count());
            var record = _reportRepository.PlayerBetHistoryRecords.Single();
            Assert.AreEqual(betAmount, record.BetAmount);
            Assert.AreEqual(CurrentBrand.Name, record.Brand);
            Assert.Less(DateTimeOffset.Now.AddDays(-2), record.DateBet);
            Assert.AreEqual(_game.Name, record.GameName);
            Assert.AreEqual(_player.Username, record.LoginName);
            Assert.AreEqual(_player.IpAddress ?? LocalIPAddress, record.UserIP);
            Assert.AreEqual(-betAmount, record.TotalWinLoss);
        }

        [Test]
        public async Task Can_process_cancelled_bet()
        {
            // Arrange
            var betAmount = RandomBetAmount();

            // Act
            await _gamesTestHelper.PlaceAndCancelBet(betAmount, _player.Id);

            // Assert
            Assert.AreEqual(1, _reportRepository.PlayerBetHistoryRecords.Count());
            var record = _reportRepository.PlayerBetHistoryRecords.Single();
            Assert.AreEqual(betAmount, record.BetAmount);
            Assert.AreEqual(CurrentBrand.Name, record.Brand);
            Assert.Less(DateTimeOffset.Now.AddDays(-2), record.DateBet);
            Assert.AreEqual(_game.Name, record.GameName);
            Assert.AreEqual(_player.Username, record.LoginName);
            Assert.AreEqual(_player.IpAddress ?? LocalIPAddress, record.UserIP);
            Assert.AreEqual(0, record.TotalWinLoss);
        }

        [Test]
        public async Task Can_export_report_data()
        {
            // Arrange
            await _gamesTestHelper.PlaceBet(RandomBetAmount(), _player.Id);

            var filteredRecords = ReportController.FilterAndOrder(
                _reportQueries.GetPlayerBetHistoryRecordsForExport(),
                new PlayerBetHistoryRecord(),
                "DateBet", "asc");

            // Act
            var content = Encoding.Unicode.GetString(ReportController.ExportToExcel(filteredRecords));

            // verify data
            Assert.AreNotEqual(content.IndexOf("<table"), -1);
        }

        private int RandomBetAmount()
        {
            return _random.Next(100, 1000);
        }
    }
}