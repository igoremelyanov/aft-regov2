using System;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Helpers;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Infrastructure.Providers;
using AFT.RegoV2.Shared.Utils;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Integration
{
    public class OnlineDepositTests : MultiprocessTestsBase
    {
        private IBrandOperations _brandOperations;
        private IGameRepository _gameRepository;
        private PlayerQueries _playerQueries;
        private IOnlineDepositCommands _onlineDepositCommands;
        private IOnlineDepositQueries _onlineDepositQueries;
        private ReportQueries _reportQueries;
        private Core.Common.Data.Player.Player _testPlayer;
        private string _transactionNumber;
        private string OnlineDepositKey
        {
            get
            {
                var testKey = "testKey";
                if (ConfigurationManager.AppSettings["OnlineDepositKey"] != null)
                    testKey = ConfigurationManager.AppSettings["OnlineDepositKey"];
                return testKey;
            }
        }
        public override void BeforeEach()
        {
            base.BeforeEach();

            _brandOperations = Container.Resolve<IBrandOperations>();
            _gameRepository = Container.Resolve<IGameRepository>();
            _onlineDepositCommands = Container.Resolve<IOnlineDepositCommands>();
            _onlineDepositQueries = Container.Resolve<IOnlineDepositQueries>();
            _playerQueries = Container.Resolve<PlayerQueries>();
            _reportQueries = Container.Resolve<ReportQueries>();

            _testPlayer = Container.Resolve<PlayerTestHelper>().CreatePlayer();
            WaitForPlayerRegistered(_testPlayer.Id);
        }

        [Test]
        public async void Can_submit_online_deposit_request()
        {
            #region submit
            var depositRequest = new OnlineDepositRequest
            {
                Amount = 200,
                PlayerId = _testPlayer.Id,
                RequestedBy = _testPlayer.Username,
            };

            var requestResult = SubmitOnlineDeposit(depositRequest);
            requestResult.Should().NotBeNull("Deposit submit result should not be null");

            //check PlayerActivityLog is inserted
            var playerActivityLogs = GetPlayerActivityLog(_testPlayer.Id, "Deposit", "Create Online Deposit");
            playerActivityLogs.Should().NotBeNull("PlayerActivityLogs-Create");

            //Deposit Record
            var depositReportNew = GetDepositRecords(_transactionNumber, OnlineDepositStatus.Processing.ToString());
            depositReportNew.Should().NotBeNull();
            #endregion

            #region paynotify
            var notifyResponse = NotifyOnlineDeposit(_transactionNumber);
            notifyResponse.Should().Be("SUCCESS");

            //Player Activity Logs
            playerActivityLogs = GetPlayerActivityLog(_testPlayer.Id, "Deposit", "Approve Online Deposit");
            playerActivityLogs.Should().NotBeNull("PlayerActivityLogs-Approve");

            //Query Status
            var statusAfterNotify = QueryOnlineDeposit(_transactionNumber);
            statusAfterNotify.IsPaid.Should().Be(true);
            statusAfterNotify.Amount.Should().Be(depositRequest.Amount);

            //Deposit Record            
            var depositReportApproved = GetDepositRecords(_transactionNumber, OnlineDepositStatus.Approved.ToString());
            depositReportApproved.Should().NotBeNull();
            #endregion

            //check wallet
            var identity = Container.Resolve<ClaimsIdentityProvider>().GetActorIdentity(_testPlayer.Id, "Tests");
            Thread.CurrentPrincipal = new ClaimsPrincipal(identity);
            var wallet = await Container.Resolve<IWalletQueries>().GetPlayerBalance(_testPlayer.Id);
            wallet.Main.Should().Be(depositRequest.Amount, "wallet amount");
        }

        private async Task<SubmitOnlineDepositRequestResult> SubmitOnlineDeposit(OnlineDepositRequest depositRequest)
        {
            var requestResult = await _onlineDepositCommands.SubmitOnlineDepositRequest(depositRequest);

            requestResult.Should().NotBeNull();
            requestResult.RedirectParams.Amount.Should().Be(depositRequest.Amount, "Amount");
            //keep transactionNumber
            _transactionNumber = requestResult.RedirectParams.OrderId;

            return requestResult;
        }

        private string NotifyOnlineDeposit(string transactionNumber)
        {
            var notifyRequest = new OnlineDepositPayNotifyRequest
            {
                OrderIdOfMerchant = transactionNumber,
                OrderIdOfRouter = "ROID" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                OrderIdOfGateway = "GOID" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                Language = "zh-CN",
                PayMethod = "XPAY"
            };
            var sign = notifyRequest.OrderIdOfMerchant + notifyRequest.OrderIdOfRouter + notifyRequest.OrderIdOfGateway + notifyRequest.Language + OnlineDepositKey;
            notifyRequest.Signature = EncryptHelper.GetMD5HashInHexadecimalFormat(sign);
            var notifyResponse = _onlineDepositCommands.PayNotify(notifyRequest);
            return notifyResponse;
        }

        private CheckStatusResponse QueryOnlineDeposit(string transactionNumber)
        {
            var status = _onlineDepositQueries.CheckStatus(new CheckStatusRequest { TransactionNumber = transactionNumber });
            return status;
        }

        private DepositRecord GetDepositRecords(string transactionNumber, string status)
        {
            Func<DepositRecord> func = () =>
            {
                var depositReport = _reportQueries.GetDepositRecords().
                    FirstOrDefault(x => x.TransactionId == transactionNumber
                        && x.Status == status
                    );
                return depositReport;
            };

            return WaitFor(func, TimeSpan.FromSeconds(20));
        }

        private Core.Player.Data.PlayerActivityLog GetPlayerActivityLog(Guid playerId, string category, string activityDone)
        {
            Func<Core.Player.Data.PlayerActivityLog> func = () =>
            {
                var playerActivityLogs = _playerQueries.GetPlayerActivityLog().FirstOrDefault(
                 x => x.Category == category && x.PlayerId == playerId &&
                      x.ActivityDone == activityDone
                 );
                return playerActivityLogs;
            };

            return WaitFor(func, TimeSpan.FromSeconds(30));
        }

        private T WaitFor<T>(Func<T> action, TimeSpan timeout)
        {
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < timeout)
            {
                T obj = action.Invoke();
                if (obj != null)
                    return obj;

                Thread.Sleep(100);
            }

            throw new Exception("Timeout");
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
