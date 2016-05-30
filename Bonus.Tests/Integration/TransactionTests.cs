using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Core;
using AFT.RegoV2.Bonus.Core.ApplicationServices;
using AFT.RegoV2.Bonus.Core.EventHandlers;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Infrastructure.DataAccess;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Tests.Common;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Integration
{
    internal class TransactionTests : IntegrationTestBase
    {
        protected override IUnityContainer CreateContainer()
        {
            return base.CreateContainer()
                .RegisterType<IBonusRepository, BonusRepository>(new PerResolveLifetimeManager());
        }

        [Test, Explicit]
        public void Multiple_Players_betting_and_depositing_simultaneously()
        {
            const int playersInParallel = 2;
            const int timesToBet = 10;

            var info = CreateTemplateInfo(BonusType.FirstDeposit, IssuanceMode.AutomaticWithCode);
            var bonus = CreateBonus(CreateTemplate(info));

            var scenario = new Action(() =>
            {
                Trace.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Creating player in thread #{Thread.CurrentThread.ManagedThreadId}");
                var playerId = CreatePlayer();

                Trace.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Depositing in thread #{Thread.CurrentThread.ManagedThreadId}");
                MakeDeposit(playerId, timesToBet * 20, bonus.Code);

                for (var i = 0; i < timesToBet; i++)
                {
                    Trace.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Betting #{i} in thread #{Thread.CurrentThread.ManagedThreadId}");
                    PlaceAndLoseBet(10, playerId);
                    PlaceAndWinBet(10, 20, playerId);
                }
            });

            var options = new ParallelOptions { MaxDegreeOfParallelism = playersInParallel };
            Parallel.Invoke(options, Enumerable.Repeat(scenario, playersInParallel).ToArray());

            Container.Resolve<IBonusRepository>()
                .GetCurrentVersionBonuses()
                .Single(b => b.Id == bonus.Id)
                .Statistic
                .TotalRedemptionCount
                .Should()
                .Be(playersInParallel);
        }

        [Test, Explicit]
        public void One_player_deposits_and_bets_simulteneously()
        {
            const decimal testAmount = 10m;
            const int simulationCount = 50;

            var playerId = CreatePlayer();

            //create initial balance
            MakeDeposit(playerId, simulationCount * testAmount * 2);
            var info = CreateTemplateInfo(BonusType.ReloadDeposit, IssuanceMode.AutomaticWithCode);
            var bonus = CreateBonus(CreateTemplate(info));

            var depositing = new Action(() =>
            {
                for (var i = 0; i < simulationCount; i++)
                {
                    Trace.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Depositing #{i} in thread #{Thread.CurrentThread.ManagedThreadId}");
                    MakeDeposit(playerId, testAmount, bonus.Code);
                }
            });

            var betting = new Action(() =>
            {
                for (var i = 0; i < simulationCount; i++)
                {
                    Trace.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Betting #{i} in thread #{Thread.CurrentThread.ManagedThreadId}");
                    PlaceAndLoseBet(testAmount, playerId);
                    PlaceAndLoseBet(testAmount, playerId);
                }
            });

            Parallel.Invoke(depositing, betting);

            Container.Resolve<IBonusRepository>()
                .Players
                .Single(p => p.Id == playerId)
                .Wallets
                .Single()
                .Main
                .Should()
                .Be(simulationCount * testAmount);
        }

        [Test, Explicit]
        public void One_player_bets_in_multiple_windows_simulteneously()
        {
            const decimal testAmount = 10m;
            const int simulationCount = 100;
            const int threadsCount = 2;

            var playerId = CreatePlayer();
            //creating initial balance for betting
            const decimal depositAmount = simulationCount * testAmount * threadsCount;
            MakeDeposit(playerId, depositAmount);

            var betting = new Action(() =>
            {
                for (var i = 0; i < simulationCount; i++)
                {
                    Trace.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Betting #{i} in thread #{Thread.CurrentThread.ManagedThreadId}");
                    PlaceAndLoseBet(testAmount, playerId);
                }
            });

            var options = new ParallelOptions { MaxDegreeOfParallelism = threadsCount };
            Parallel.Invoke(options, Enumerable.Repeat(betting, threadsCount).ToArray());

            var wallet = Container.Resolve<IBonusRepository>().Players
                .Single(p => p.Id == playerId)
                .Wallets
                .Single();

            wallet
                .Main
                .Should()
                .Be(0);

            wallet
                .Transactions
                .Where(t => t.Type == TransactionType.Deposit)
                .Sum(t => t.MainBalanceAmount)
                .Should()
                .Be(depositAmount);

            wallet
                .Transactions
                .Where(t => t.Type == TransactionType.BetLost)
                .Sum(t => t.MainBalanceAmount)
                .Should()
                .Be(-depositAmount);
        }

        private Guid CreatePlayer()
        {
            var playerId = Guid.NewGuid();
            Container.Resolve<PlayerSubscriber>().Handle(new PlayerRegistered
            {
                PlayerId = playerId,
                BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                UserName = TestDataGenerator.GetRandomString(),
                Email = TestDataGenerator.GetRandomEmail(),
                VipLevel = "Silver",
                CurrencyCode = "CAD",
                DateRegistered = DateTime.Now
            });

            return playerId;
        }
        private void MakeDeposit(Guid playerId, decimal depositAmount = 200, string bonusCode = null)
        {
            var paymentSubscriber = Container.Resolve<PaymentSubscriber>();
            var depositId = Guid.NewGuid();
            paymentSubscriber.Handle(new DepositSubmitted
            {
                DepositId = depositId,
                PlayerId = playerId,
                Amount = depositAmount
            });

            if (string.IsNullOrWhiteSpace(bonusCode) == false)
            {
                Container.Resolve<BonusCommands>().ApplyForBonus(new DepositBonusApplication
                {
                    PlayerId = playerId,
                    BonusCode = bonusCode,
                    Amount = depositAmount,
                    DepositId = depositId
                });
            }
            paymentSubscriber.Handle(new DepositApproved
            {
                DepositId = depositId,
                PlayerId = playerId,
                ActualAmount = depositAmount
            });
        }
        private new void PlaceAndLoseBet(decimal amount, Guid playerId)
        {
            var gameSubscriber = Container.Resolve<GameSubscriber>();
            var gameId = Guid.NewGuid();
            var roundId = Guid.NewGuid();
            gameSubscriber.Handle(new BetPlaced
            {
                PlayerId = playerId,
                Amount = amount,
                GameId = gameId,
                RoundId = roundId
            });
            gameSubscriber.Handle(new BetLost
            {
                PlayerId = playerId,
                RoundId = roundId
            });
        }
        private new void PlaceAndWinBet(decimal betAmount, decimal wonAmount, Guid playerId)
        {
            var gameSubscriber = Container.Resolve<GameSubscriber>();
            var gameId = Guid.NewGuid();
            var roundId = Guid.NewGuid();
            gameSubscriber.Handle(new BetPlaced
            {
                PlayerId = playerId,
                Amount = betAmount,
                GameId = gameId,
                RoundId = roundId
            });
            gameSubscriber.Handle(new BetWon
            {
                PlayerId = playerId,
                Amount = wonAmount,
                RoundId = roundId,
                GameId = gameId
            });
        }
    }
}