using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.GameApi.Interface.Classes;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using AFT.RegoV2.GameApi.Tests.Core;
using AFT.RegoV2.GameApi.Tests.Integration.Steps.Entities;
using AFT.RegoV2.Shared;

using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Table = TechTalk.SpecFlow.Table;

namespace AFT.RegoV2.GameApi.Tests.Integration.Steps
{
    [Binding, Scope(Feature = "Core Game Api")]
    public class CoreGameApi : SpecFlowIntegrationTestBase
    {
        [Given(@"I get authentication token for player ""(.*)"" with password ""(.*)""")]
        public async Task GivenIGetAuthenticationTokenForPlayer(string player, string password)
        {
            var token = await AuthorizePlayer(player, password);

            Set(SR.token, token);
            Set(SR.PlayerName, player);

            token.Should().NotBeNullOrEmpty();
        }

        [Given(@"I get authorization token for game provider ""(.*)"" with secret ""(.*)""")]
        public async Task GivenIGetAuthorizationTokenForPlayer(string clientId, string clientSecret)
        {
            SetGameProvider(clientId, clientSecret);

            var accessToken = await GetAccessTokenFor("api/mock/oauth/token", clientId, clientSecret);

            Set(SR.accesstoken, accessToken);

            accessToken.Should().NotBeNullOrEmpty();
        }

        [When(@"I call to validate token")]
        public async Task WhenICallToValidateToken()
        {
            var token = Get<string>(SR.token);
            var response =
                await JsonPostSecure<ValidateTokenResponse>(
                    Config.GameApiUrl + "api/mock/token/validate",
                    Get<string>(SR.accesstoken),
                    new ValidateToken
                    {
                        PlayerIpAddress = DefaultPlayertIp,
                        AuthToken = token
                    });

            Set(SR.response, response);
        }
        [Then(@"I will receive successful validation result")]
        public void ThenIWillReceiveSuccessfulValidationResult()
        {
            var response = Get<ValidateTokenResponse>(SR.response);

            response.Should().NotBeNull();
            response.ErrorCode.Should().Be(GameApiErrorCode.NoError);
            response.PlayerDisplayName.Should().NotBeNull();
            response.PlayerId.Should().NotBe(Guid.Empty);


            var playerData = GetOrCreate(SR.PlayerData, () => new Dictionary<string, PlayerData>());
            if (playerData.ContainsKey(Get<string>(SR.PlayerName)) == false)
                playerData.Add(Get<string>(SR.PlayerName), new PlayerData
                {
                    PlayerId = response.PlayerId
                });


        }
        [Given(@"I validate the token")]
        public async Task GivenIValidateTheToken()
        {
            await WhenICallToValidateToken();
            ThenIWillReceiveSuccessfulValidationResult();
        }
        [Given(@"the player ""(.*)"" main balance is \$(.*)")]
        public async Task GivenThePlayerMainBalanceIsAndBonusBalanceIs(string playerName, decimal mainBalance)
        {
            var brandOperations = Container.Resolve<IBrandOperations>();

            var gsiDb = GetOrCreateGamesDb();
            
            var player = await gsiDb.Players.SingleAsync(p => p.Name == playerName);

            await brandOperations.FundInAsync(player.Id, mainBalance, player.CurrencyCode, Guid.NewGuid().ToString());

        }

        [When(@"the player ""(.*)"" withdraws \$(.*)")]
        public async Task WhenIWithdraw(string playerName, decimal mainBalance)
        {
            var brandOperations = Container.Resolve<IBrandOperations>();

            var gsiDb = GetOrCreateGamesDb();

            var player = await gsiDb.Players.SingleAsync(p => p.Name == playerName);

            await brandOperations.FundOutAsync(player.Id, mainBalance, player.CurrencyCode, Guid.NewGuid().ToString());

        }
        [When(@"I bet \$(.*) for game ""(.*)""")]
        public async Task WhenIBet(decimal amount, string gameId)
        {
            var token = Get<string>(SR.token);
            var roundIdsList = GetOrCreate(SR.roundIdsList, () => new List<string>());
            var txIdList = GetOrCreate(SR.txIdsList, () => new List<string>());
            var response = 
                await JsonPostSecure<PlaceBetResponse>(Config.GameApiUrl + "api/mock/bets/place", 
                        Get<string>(SR.accesstoken),
                        new PlaceBet
                        {
                            AuthToken = token,
                            Transactions = new List<BetCommandTransactionRequest>
                            {
                                new BetCommandTransactionRequest
                                {
                                    Id = txIdList.AddAndPass(Set(SR.transactionId, NewStringId)),
                                    GameId = gameId,
                                    RoundId = roundIdsList.AddAndPass(Set(SR.roundId, NewStringId)),
                                    CurrencyCode = "RMB",
                                    TimeStamp = DateTimeOffset.UtcNow,
                                    Amount = amount
                                }
                            }
                        });

            Set(SR.response, response);
        }

        [When(@"""(.*)"" bets \$(.*) for game ""(.*)""")]
        public async Task WhenPlayerBets(string playerName, decimal amount, string gameId)
        {
            var token = Get<string>(SR.token);
            var txId = NewStringId;

            var playerData = Get<Dictionary<string, PlayerData>>(SR.PlayerData);
            playerData[playerName].LastPlaceBetTransactionId = txId;
            playerData[playerName].RoundId = NewStringId;
            var response =
                await JsonPostSecure<PlaceBetResponse>(Config.GameApiUrl + "api/mock/bets/place",
                        Get<string>(SR.accesstoken),
                        new PlaceBet
                        {
                            AuthToken = token,
                            Transactions = new List<BetCommandTransactionRequest>
                            {
                                new BetCommandTransactionRequest
                                {
                                    Id = Set(SR.transactionId, txId),
                                    GameId = gameId,
                                    RoundId = playerData[playerName].RoundId,
                                    CurrencyCode = "RMB",
                                    TimeStamp = DateTimeOffset.UtcNow,
                                    Amount = amount
                                }
                            }
                        });

            Set(SR.response, response);
        }

        [When(@"I win \$(.*)")]
        public async Task WhenIWin(decimal amount)
        {
            var roundId = Get<string>(SR.roundId);
            var placeBetTxId = Get<string>(SR.transactionId);

            var transactions = new List<BetCommandTransactionRequest>
            {
                new BetCommandTransactionRequest
                {
                    Id = Set(SR.transactionId, NewStringId),
                    ReferenceId = placeBetTxId,
                    RoundId = roundId,
                    CurrencyCode = "RMB",
                    TimeStamp = DateTimeOffset.UtcNow,
                    Amount = amount
                }
            };
            await SendWinBetTransactions(transactions);
        }


        [When(@"I lose the bet")]
        public async Task WhenILoseTheBet()
        {
            var token = Get<string>(SR.token);
            var roundId = Get<string>(SR.roundId);
            var placeBetTxId = Get<string>(SR.transactionId);

            var response =
                await JsonPostSecure<LoseBetResponse>(Config.GameApiUrl + "api/mock/bets/lose",
                    Get<string>(SR.accesstoken),
                    new LoseBet
                    {
                        AuthToken = token,
                        Transactions = new List<BetCommandTransactionRequest>
                        {
                            new BetCommandTransactionRequest
                            {
                                Id = Set(SR.transactionId, NewStringId),
                                ReferenceId = placeBetTxId,
                                RoundId = roundId,
                                CurrencyCode = "RMB",
                                TimeStamp = DateTimeOffset.UtcNow,
                                Amount = 0 // MUST BE 0
                            }
                        }
                    });
            Set(SR.response, response);
        }
        [When(@"I get free bet for \$(.*) for game ""(.*)""")]
        public async Task WhenIGetFreeBetFor(decimal amount, string gameId)
        {
            var token = Get<string>(SR.token);
            var betIdsList = GetOrCreate(SR.roundIdsList, () => new List<string>());
            var response =
                await JsonPostSecure<FreeBetResponse>(Config.GameApiUrl + "api/mock/bets/freebet",
                    Get<string>(SR.accesstoken),
                    new FreeBet
                    {
                        AuthToken = token,
                        Transactions = new List<BetCommandTransactionRequest>
                        {
                            new BetCommandTransactionRequest
                            {
                                Id = Set(SR.transactionId, NewStringId),
                                GameId = gameId,
                                RoundId = betIdsList.AddAndPass(Set(SR.roundId, NewStringId)),
                                CurrencyCode = "RMB",
                                TimeStamp = DateTimeOffset.UtcNow,
                                Amount = amount
                            }
                        }
                    });
            Set(SR.response, response);
        }
        [Then(@"requested bet (.*) recorded")]
        public void ThenRequestedBetillNotBeRecorded(bool willBeRecorded)
        {
            ThenRequestedBetsWillNotBeRecorded(willBeRecorded);
        }

        [When(@"I win multiple amounts:")]
        public async Task WhenIWinMultipleAmounts(Table table)
        {
            var roundId = Get<string>(SR.roundId);
            var placeBetTxId = Get<string>(SR.transactionId);

            var transactions = new List<BetCommandTransactionRequest>();
            var betAmounts = table.CreateSet(tr => Decimal.Parse(tr.Values.First()));

            foreach (var betAmount in betAmounts)
            {
                transactions.Add(new BetCommandTransactionRequest
                {
                    Id = Set(SR.transactionId, NewStringId),
                    ReferenceId = placeBetTxId,
                    RoundId = roundId,
                    CurrencyCode = "RMB",
                    TimeStamp = DateTimeOffset.UtcNow,
                    Amount = betAmount
                });
            }
            await SendWinBetTransactions(transactions);
        }

        private async Task SendWinBetTransactions(List<BetCommandTransactionRequest> transactions)
        {
            var token = Get<string>(SR.token);

            var response =
                await JsonPostSecure<WinBetResponse>(Config.GameApiUrl + "api/mock/bets/win",
                    Get<string>(SR.accesstoken),
                    new WinBet
                    {
                        AuthToken = token,
                        Transactions = transactions
                    });
            Set(SR.response, response);
            
        }

        [When(@"I place bets for game ""(.*)"" for amount:")]
        public async Task WhenIPlaceBetsForAmount(string gameId, Table table)
        {
            var bets = table.CreateSet(tr => Decimal.Parse(tr.Values.First()));

            foreach (var amount in bets)
            {
                await WhenIBet(amount, gameId);
            }
        }
        [When(@"I settle the following bets with ""(.*)"":")]
        public async Task WhenISettleTheFollowingBets(string securityKey, Table table)
        {
            var bets = table.CreateSet<TypeAndAmount>();

            var betIdsList = GetOrCreate(SR.roundIdsList, () => new List<string>());
            var txIdsList = GetOrCreate(SR.txIdsList, () => new List<string>());
            var i = 0;
            var previousTransactionId = Get<string>(SR.transactionId);
            var response =
                await JsonPostSecure<BatchSettleResponse>(Config.GameApiUrl + "api/mock/batch/bets/settle",
                    Get<string>(SR.accesstoken),
                    new BatchSettle
                    {
                        BatchId = NewStringId,
                        SecurityKey = securityKey,
                        Transactions =
                            bets.Select(taa => new BatchCommandTransactionRequest
                            {
                                Id = Set(SR.transactionId, NewStringId),
                                ReferenceId = txIdsList[i],
                                RoundId = betIdsList[i++],
                                CurrencyCode = "RMB",
                                Amount = taa.Amount,
                                BrandCode = "138",
                                TransactionType = 
                                    taa.Type == "WIN" 
                                        ? BatchSettleBetTransactionType.Win 
                                        : BatchSettleBetTransactionType.Lose
                            })
                            .ToList()
                    });
            Set(SR.response, response);
        }

        [When(@"I batch settle the following bets with ""(.*)"":")]
        public async Task WhenIBatchSettleTheFollowingBets(string securityKey, Table table)
        {
            var bets = table.CreateSet<PlayerTypeAndAmount>();

            var playerData = Get<Dictionary<string, PlayerData>>(SR.PlayerData);

            var response =
                await JsonPostSecure<BatchSettleResponse>(Config.GameApiUrl + "api/mock/batch/bets/settle",
                    Get<string>(SR.accesstoken),
                    new BatchSettle
                    {
                        BatchId = NewStringId,
                        SecurityKey = securityKey,
                        Transactions =
                            bets.Select(taa => new BatchCommandTransactionRequest
                            {
                                Id = Set(SR.transactionId, NewStringId),
                                UserId = playerData[taa.PlayerName].PlayerId,
                                ReferenceId = playerData[taa.PlayerName].LastPlaceBetTransactionId,
                                RoundId = playerData[taa.PlayerName].RoundId,
                                CurrencyCode = "RMB",
                                Amount = taa.Amount,
                                BrandCode = "138",
                                TransactionType =
                                    taa.Type == "WIN"
                                        ? BatchSettleBetTransactionType.Win
                                        : BatchSettleBetTransactionType.Lose
                            })
                            .ToList()
                    });
            Set(SR.response, response);
        }

        [Then(@"requested bets (.*) recorded")]
        public void ThenRequestedBetsWillNotBeRecorded(bool willBeRecorded)
        {
            var gsiDb = GetOrCreateGamesDb();
            var betIdsList = Get<List<string>>(SR.roundIdsList);
            if(betIdsList == null) Assert.Fail("Expected to have placed bet(s)");
            foreach(var roundId in betIdsList)
            {
                var exists = gsiDb.Rounds.Any(b => b.ExternalRoundId == roundId);
                if (willBeRecorded != exists)
                {
                    Assert.Fail("Expected round with ID={0} was {1}found".Args(roundId, willBeRecorded ? "not " : ""));
                }
            };
        }
        [Then(@"place bet response balance will be \$(.*)")]
        public void ThenPlaceBetResponseBalanceWillEqualRequestedBalance(decimal amount)
        {
            var response = Get<PlaceBetResponse>(SR.response);

            response.Balance.Should().Be(amount);
        }

        [Then(@"win bet response balance will be \$(.*)")]
        public void ThenWinBetResponseBalanceWillBe(decimal balance)
        {
            var response = Get<WinBetResponse>(SR.response);

            response.Balance.Should().Be(balance);
        }


        [Then(@"I will get error code ""(.*)""")]
        public void ThenIWillGetErrorCode(GameApiErrorCode error)
        {
            var response = Get<GameApiResponseBase>(SR.response);

            response.ErrorCode.Should().Be(error);
        }

        [Then(@"the response's playable balance for ""(.*)"" will be \$(.*)")]
        public void ThenTheResponsesPlayableBalanceForPlayerWillBe(string playerName, decimal expectedBalance)
        {
            var response = Get<BatchCommandResponse>(SR.response);

            var playerData = Get<Dictionary<string, PlayerData>>(SR.PlayerData);
            
            response.PlayerBalances[playerData[playerName].PlayerId].Should().Be(expectedBalance);
        }

        [Then(@"the player's playable balance will be \$(.*)")]
        public void ThenThePlayersPlayableBalanceWillBe(decimal playableBalance)
        {
            var balance = Get<GetBalanceResponse>(SR.balance);

            balance.Balance.Should().Be(playableBalance);
        }
        [When(@"I get balance")]
        public async Task WhenIGetBalance()
        {
            var token = Get<string>(SR.token);
            var balance =
                await JsonGetSecure<GetBalanceResponse>(Config.GameApiUrl + "api/mock/players/balance?authtoken=" + token,
                                   Get<string>(SR.accesstoken));

            Set(SR.balance, balance);
        }
        [When(@"I get history for game ""(.*)""")]
        public async Task WhenIGetHistory(string gameId)
        {
            var token = Get<string>(SR.token);
            var history =
                await JsonGetSecure<BetsHistoryResponse>(Config.GameApiUrl + "api/mock/bets/history?gameid=" + gameId + "&authtoken=" + token,
                                   Get<string>(SR.accesstoken));

            Set(SR.response, history);
        }
        [Then(@"I will see the bet IDs in the history")]
        public void ThenIWillSeeTheBetIDsInTheHistory()
        {
            var history = Get<BetsHistoryResponse>(SR.response);
            var roundIdsList = Get<List<string>>(SR.roundIdsList);
            if(roundIdsList == null || roundIdsList.Count == 0) Assert.Fail("Expected to have placed one or more bet(s)");

            history.Should().NotBeNull();
            foreach (var roundId in roundIdsList)
            {
                if (history.Rounds.All(b => b.Id != roundId))
                {
                    Assert.Fail("Expected to see bet with ID=" + roundId);
                }
            }
        }
        [When(@"I adjust transaction with \$(.*)")]
        public async Task WhenIAdjustTransactionWith(decimal amount)
        {
            var roundId = Get<string>(SR.roundId);
            var token = Get<string>(SR.token);
            var response =
                await JsonPostSecure<AdjustTransactionResponse>(Config.GameApiUrl + "api/mock/transactions/adjust",
                    Get<string>(SR.accesstoken),
                    new AdjustTransaction
                    {
                        AuthToken = token,
                        Transactions =
                            new List<BetCommandTransactionRequest>
                            {
                                new BetCommandTransactionRequest
                                {
                                    Id = Set(SR.transactionId, NewStringId),
                                    RoundId = roundId,
                                    Amount = amount,
                                    CurrencyCode = "RMB",
                                    TimeStamp = DateTimeOffset.UtcNow
                                }
                            }
                    });
            Set(SR.response, response);
        }
        [When(@"I cancel the last transaction")]
        public async Task WhenICancelTheLastTransaction()
        {
            var roundId = Get<string>(SR.roundId);
            var token = Get<string>(SR.token);
            var transactionId = Get<string>(SR.transactionId);
            var response =
                await JsonPostSecure<CancelTransactionResponse>(Config.GameApiUrl + "api/mock/transactions/cancel",
                    Get<string>(SR.accesstoken),
                    new CancelTransaction
                    {
                        AuthToken = token,
                        Transactions =
                            new List<BetCommandTransactionRequest>
                            {
                                new BetCommandTransactionRequest
                                {
                                    Id = NewStringId,
                                    RoundId = roundId,
                                    ReferenceId = transactionId,
                                    TimeStamp = DateTimeOffset.UtcNow
                                }
                            }
                    });
            Set(SR.response, response);
        }
    }
}