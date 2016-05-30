using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AFT.RegoV2.Infrastructure.Providers;
using AFT.RegoV2.Shared;
using AFT.UGS.Endpoints.Games.FlyCow.Models;

using FakeUGS.Core.Classes;
using FakeUGS.Core.Data;
using FakeUGS.Core.Exceptions;
using FakeUGS.Core.Interfaces;
using FakeUGS.Core.ServiceContracts;
using FakeUGS.Core.ServiceContracts.Base;

using Microsoft.Practices.Unity;

using GetBalanceResponse = FakeUGS.Core.ServiceContracts.GetBalanceResponse;
using ValidateTokenResponse = FakeUGS.Core.ServiceContracts.ValidateTokenResponse;
using AFT.UGS.Core.BaseModels.Enums;

namespace FakeUGS.Core.Services
{
    public interface ICommonGameActionsProvider
    {
        void SetGameActionContext(GameActionContextDetails context);

        Task<ValidateTokenResponse> ValidateTokenAsync(IAuthTokenHolder validateToken);
        Task<GetBalanceResponse> GetBalanceAsync(GetBalance request);

        Task<PlaceBetResponse> PlaceBetAsync(PlaceBet request);
        Task<WinBetResponse> WinBetAsync(WinBet request);
        Task<LoseBetResponse> LoseBetAsync(LoseBet request);
        Task<FreeBetResponse> FreeBetAsync(FreeBet request);
        Task<TieBetResponse> TieBetAsync(TieBet request);
        Task<AdjustTransactionResponse> AdjustTransaction(AdjustTransaction request);
        Task<CancelTransactionResponse> CancelTransaction(CancelTransaction request);

        Task<BatchSettleResponse> BatchSettle(BatchSettle request);
        BatchAdjustResponse BatchAdjust(BatchAdjust request);
        CancelTransactionsResponse BatchCancel(BatchCancel request);

        BetsHistoryResponse GetBetHistory(RoundsHistory request);
    }

    public class GameActionContextDetails
    {
        public string GameProviderCode { get; set; }
        public bool OptionalTxRefId { get; set; }
        public string PlayerToken { get; set; }
    }

    public sealed class CommonGameActionsProvider : ICommonGameActionsProvider
    {
        [Dependency]
        internal ITokenProvider TokenProvider { get; set; }
        private readonly IGameQueries _gameQueries;
        private readonly IGameCommands _gameCommands;
        private readonly IErrorManager _errors;
        private readonly IUnityContainer _container;
        private readonly ITransactionScopeProvider _transactionScope;
        private readonly IFlycowApiClientProvider _flycowApiClientProvider;

        private GameActionContext Context { get; set; }
        private readonly bool _useRealUgs;

        void ICommonGameActionsProvider.SetGameActionContext(GameActionContextDetails context)
        {
            Context = new GameActionContext
            {
                GameProviderCode = context.GameProviderCode,
                OptionalTxRefId = context.OptionalTxRefId,
                PlayerToken = context.PlayerToken
            };
        }

        public CommonGameActionsProvider(
            IUnityContainer container,
            IGameCommands gameCommands,
            IGameQueries gameQueries,
            ITransactionScopeProvider transactionScope,
            IErrorManager errors,
            IModeSwitch modeSwitch,
            IFlycowApiClientProvider flycowApiClientProvider
            )
        {
            _container = container;
            _gameCommands = gameCommands;
            _gameQueries = gameQueries;
            _transactionScope = transactionScope;
            _errors = errors;
            _flycowApiClientProvider = flycowApiClientProvider;

            _useRealUgs = modeSwitch.IsUsingRealUgs();
        }

        async Task<ValidateTokenResponse> ICommonGameActionsProvider.ValidateTokenAsync(IAuthTokenHolder request)
        {
            if (_useRealUgs)
            {
                var client = _flycowApiClientProvider.GetApiClient();
                var token = await _flycowApiClientProvider.GetApiToken(client);

                var validationResponse = await client.ValidateTokenAsync(new ValidateTokenRequest
                {
                    authtoken = request.AuthToken,
                    ipaddress = request.PlayerIpAddress
                }, token);

                if (validationResponse.err != (int)UgsApiError.None)
                {
                    throw new InvalidTokenException("Token validation failed with description: " + validationResponse.errdesc);
                }

                return new ValidateTokenResponse
                {
                    Balance = validationResponse.bal,
                    PlayerId = validationResponse.userid,
                    PlayerDisplayName = validationResponse.username,
                    BrandCode = validationResponse.brand,
                    Language = validationResponse.lang,
                    CurrencyCode = validationResponse.cur,
                    BetLimitCode = validationResponse.betlimitid
                };
            }

            var playerId = GetPlayerIdFromToken(request);
            var playerData = await _gameQueries.GetPlayerDataAsync(playerId); // Get player

            if (playerData == null)
                throw new PlayerNotFoundException("Cannot find player with id=" + playerId);

            var balance = await _gameQueries.GetPlayableBalanceAsync(playerId); // Get balance

            var brandCode = await _gameQueries.GetBrandCodeAsync(playerData.BrandId); // Get brand code

            var playerBetLimitCode = await _gameQueries.GetPlayerBetLimitCodeOrNullAsync(playerData.VipLevelId, Context.GameProviderCode, playerData.CurrencyCode);

            var balanceCurrencyCode =
                await
                    _gameQueries.GetMappedGameProviderCurrencyCodeAsync(Context.GameProviderCode,
                        balance.CurrencyCode);

            return new ValidateTokenResponse
            {
                Balance = balance.Balance,
                PlayerId = playerData.Id,
                PlayerDisplayName = playerData.Name,
                BrandCode = brandCode ?? string.Empty,
                Language = playerData.CultureCode ?? string.Empty,
                CurrencyCode = balanceCurrencyCode,
                BetLimitCode = playerBetLimitCode ?? ""
            };
        }

        #region Transactional bet methods

        async Task<PlaceBetResponse> ICommonGameActionsProvider.PlaceBetAsync(PlaceBet request)
        {
            return await DoBetCommandTransactions<PlaceBet, PlaceBetResponse>(request,
                async (playerId, transaction) =>
                    await PlaceBet(playerId, transaction));
        }

        async Task<WinBetResponse> ICommonGameActionsProvider.WinBetAsync(WinBet request)
        {
            return await DoBetCommandTransactions<WinBet, WinBetResponse>(request,
                async (playerId, transaction) =>
                    await SettleBet(transaction,
                        (data, context) => _gameCommands.WinBetAsync(data, context)));
        }
        async Task<LoseBetResponse> ICommonGameActionsProvider.LoseBetAsync(LoseBet request)
        {
            return await DoBetCommandTransactions<LoseBet, LoseBetResponse>(request,
                async (playerId, transaction) =>
                    await SettleBet(transaction,
                        (data, context) => _gameCommands.LoseBetAsync(data, context)));
        }
        async Task<FreeBetResponse> ICommonGameActionsProvider.FreeBetAsync(FreeBet request)
        {
            return await DoBetCommandTransactions<FreeBet, FreeBetResponse>(request,
                async (playerId, transaction) =>
                    await SettleBet(transaction,
                        (data, context) => _gameCommands.FreeBetAsync(data, context, playerId)));
        }

        async Task<TieBetResponse> ICommonGameActionsProvider.TieBetAsync(TieBet request)
        {
            return await DoBetCommandTransactions<TieBet, TieBetResponse>(request,
                async (playerId, transaction) =>
                    await SettleBet(transaction,
                        (data, context) => _gameCommands.TieBetAsync(data, context)));
        }

        private async Task<TResponse> DoBetCommandTransactions<TRequest, TResponse>(TRequest request,
            Func<Guid, BetCommandTransactionRequest, Task<BetCommandResponseTransaction>> betCommandPerTransaction)
            where TResponse : BetCommandResponse, new()
            where TRequest : BetCommandRequest
        {
            using (var scope = _transactionScope.GetTransactionScopeAsync())
            {
                var playerId = GetPlayerIdFromToken(request);

                var resultTransactions = new List<BetCommandResponseTransaction>();
                foreach (var betCommandTransactionRequest in request.Transactions)
                {
                    var betCommandResult = await betCommandPerTransaction.Invoke(playerId, betCommandTransactionRequest);
                    resultTransactions.Add(betCommandResult);
                }

                var balance = await _gameQueries.GetPlayableBalanceAsync(playerId);

                scope.Complete();

                return new TResponse
                {
                    Balance = balance.Balance,
                    CurrencyCode = balance.CurrencyCode,
                    Transactions = resultTransactions
                };
            }
        }
        async Task<AdjustTransactionResponse> ICommonGameActionsProvider.AdjustTransaction(AdjustTransaction request)
        {
            using (var scope = _transactionScope.GetTransactionScope())
            {
                var playerId = GetPlayerIdFromToken(request);
                
                var tasks = request.Transactions.Select(async tx => await AdjustTransaction(tx)).ToList();

                var result = await Task.WhenAll(tasks);

                var balance = _gameQueries.GetPlayableBalance(playerId);

                scope.Complete();

                return new AdjustTransactionResponse
                {
                    Balance = balance.Balance,
                    CurrencyCode = balance.CurrencyCode,
                    Transactions = result.ToList()
                };
            }
        }
        async Task<CancelTransactionResponse> ICommonGameActionsProvider.CancelTransaction(CancelTransaction request)
        {
            using (var scope = _transactionScope.GetTransactionScopeAsync())
            {
                var playerId = GetPlayerIdFromToken(request);

                var tasks = request.Transactions.Select(async tx => await CancelTransaction(tx)).ToList();

                var result = await Task.WhenAll(tasks);

                var balance = _gameQueries.GetPlayableBalance(playerId);

                scope.Complete();

                return new CancelTransactionResponse
                {
                    Balance = balance.Balance,
                    CurrencyCode = balance.CurrencyCode,
                    Transactions = result.ToList()
                };
            }
        }
        #endregion

        #region Batch methods
        async Task<BatchSettleResponse> ICommonGameActionsProvider.BatchSettle(BatchSettle request)
        {
            var errorCode = GameApiErrorCode.NoError;
            var errorDescription = (string)null;
            var errorsList = new ConcurrentBag<BatchCommandTransactionError>();
            var txCount = 0;
            var isDuplicateBatch = 0;
            var timer = new Stopwatch();
            var playerBalances = new Dictionary<Guid, decimal>();

            timer.Start();
            try
            {
                _gameQueries.ValidateBatchIsUnique(request.BatchId, Context.GameProviderCode);

                if (false == await _gameQueries.ValidateSecurityKey(Context.GameProviderCode, request.SecurityKey))
                    throw new ArgumentException("Invalid security key.");

                txCount = await ProcessBatchSettleTransactionsAsync(request, errorsList);

                playerBalances = _gameQueries.GetPlayableBalances(request.Transactions.Select(x => x.UserId));
            }
            catch (DuplicateBatchException ex)
            {
                isDuplicateBatch = 1;
                errorCode = _errors.GetErrorCodeByException(ex, out errorDescription);
            }
            catch (Exception ex)
            {
                errorCode = _errors.GetErrorCodeByException(ex, out errorDescription);
            }

            timer.Stop();

            return new BatchSettleResponse
            {
                BatchId = request.BatchId,
                TransactionCount = txCount,
                BatchTimestamp = DateTimeOffset.UtcNow.ToString("O"),
                Elapsed = timer.ElapsedMilliseconds,
                Errors = errorsList.ToList(),
                ErrorCode = errorCode,
                ErrorDescription = errorDescription,
                IsDuplicate = isDuplicateBatch,
                PlayerBalances = new Dictionary<Guid, decimal>(playerBalances)
            };
        }

        BatchAdjustResponse ICommonGameActionsProvider.BatchAdjust(BatchAdjust request)
        {
            var errorCode = GameApiErrorCode.NoError;
            var errorDescription = (string)null;
            var errorsList = new ConcurrentBag<BatchCommandTransactionError>();
            var txCount = 0;

            try
            {
                txCount = request.Transactions.Count;

                request.Transactions.AsParallel().ForAll(async transaction =>
                {
                    try
                    {
                        await AdjustTransaction(transaction, request.BatchId);
                    }
                    catch (Exception ex)
                    {
                        errorsList.Add(CreateBatchTransactionError(ex, transaction.Id, transaction.UserId));
                    }
                });
            }
            catch (Exception ex)
            {
                errorCode = _errors.GetErrorCodeByException(ex, out errorDescription);
            }

            return new BatchAdjustResponse
            {
                BatchId = request.BatchId,
                TransactionCount = txCount,
                BatchTimestamp = DateTimeOffset.UtcNow.ToString("O"),
                Errors = errorsList.ToList(),
                ErrorCode = errorCode,
                ErrorDescription = errorDescription
            };
        }
        CancelTransactionsResponse ICommonGameActionsProvider.BatchCancel(BatchCancel request)
        {
            var errorCode = GameApiErrorCode.NoError;
            var errorDescription = (string)null;
            var errorsList = new ConcurrentBag<BatchCommandTransactionError>();
            var txCount = 0;

            try
            {
                txCount = request.Transactions.Count;

                request.Transactions.AsParallel().ForAll(async transaction =>
                {
                    try
                    {
                        await CancelTransaction(transaction, request.BatchId);
                    }
                    catch (Exception ex)
                    {
                        errorsList.Add(CreateBatchTransactionError(ex, transaction.Id, transaction.UserId));
                    }
                });
            }
            catch (Exception ex)
            {
                errorCode = _errors.GetErrorCodeByException(ex, out errorDescription);
            }

            return new CancelTransactionsResponse
            {
                BatchId = request.BatchId,
                TransactionCount = txCount,
                BatchTimestamp = DateTimeOffset.UtcNow.ToString("O"),
                Errors = errorsList.ToList(),
                ErrorCode = errorCode,
                ErrorDescription = errorDescription
            };
        }
        #endregion

        BetsHistoryResponse ICommonGameActionsProvider.GetBetHistory(RoundsHistory request)
        {
            var playerId = GetPlayerIdFromToken(request);

            var game = _gameQueries.GetGameByExternalGameId(request.gameid);

            if (game == null)
                throw new RegoException("Game not found");

            var rounds = _gameQueries.GetRoundHistory(playerId, game.Id, request.count);

            var convertedRounds = rounds.Select(round => new RoundHistoryData
            {
                Id = round.Data.ExternalRoundId,
                Status = round.Data.Status.ToString(),
                Amount = round.Amount,
                WonAmount = round.WonAmount,
                AdjustedAmount = round.AdjustedAmount,
                CreatedOn = round.Data.CreatedOn,
                ClosedOn = round.Data.ClosedOn,
                GameActions = round.Data.GameActions.Select(x => new GameActionHistoryData
                {
                    PlatformTxId = x.Id,
                    Amount = x.Amount,
                    Description = x.Description,
                    TransactionType = x.GameActionType.ToString(),
                    CreatedOn = x.Timestamp,
                    Id = x.ExternalTransactionId,
                }).ToList()
            }).ToList();

            return new BetsHistoryResponse
            {
                Rounds = convertedRounds
            };
        }

        async Task<GetBalanceResponse> ICommonGameActionsProvider.GetBalanceAsync(GetBalance request)
        {
            if (_useRealUgs)
            {
                var client = _flycowApiClientProvider.GetApiClient();
                var token = await _flycowApiClientProvider.GetApiToken(client);

                var balanceResponse = await client.GetBalanceAsync(new GetBalanceRequest
                {
                    authtoken = request.AuthToken
                }, token);

                return new GetBalanceResponse
                {
                    Balance = balanceResponse.bal,
                    CurrencyCode = balanceResponse.cur,
                };
            }

            var playerId = GetPlayerIdFromToken(request);
            var balance = await _gameQueries.GetPlayableBalanceAsync(playerId);

            return new GetBalanceResponse
            {
                Balance = balance.Balance,
                CurrencyCode = balance.CurrencyCode,
            };
        }

        private async Task<int> ProcessBatchSettleTransactionsAsync(BatchSettle request, ConcurrentBag<BatchCommandTransactionError> errorsList)
        {
            var txCount = 0;
            var brands = new ConcurrentDictionary<string, Brand>();
            var userGroups = request.Transactions.GroupBy(x => x.UserId);

            await Task.WhenAll(
                userGroups.Select(userTransactions =>
                    Task.Run(async () =>
                    {
                        var commands = _container.Resolve<IGameCommands>();

                        foreach (var transaction in userTransactions)
                        {
                            try
                            {
                                var betData = new GameActionData
                                {
                                    RoundId = transaction.RoundId,
                                    TransactionReferenceId = transaction.ReferenceId,
                                    ExternalTransactionId = transaction.Id,
                                    Amount = transaction.Amount,
                                    CurrencyCode = transaction.CurrencyCode,
                                    Description = transaction.Description,
                                    BatchId = request.BatchId,
                                };

                                switch (transaction.TransactionType)
                                {
                                    case BatchSettleBetTransactionType.Win:
                                        await commands.WinBetAsync(betData, Context);
                                        break;
                                    case BatchSettleBetTransactionType.Lose:
                                        await commands.LoseBetAsync(betData, Context);
                                        break;
                                    case BatchSettleBetTransactionType.Free:
                                        await commands.FreeBetAsync(betData, Context);
                                        break;
                                    case BatchSettleBetTransactionType.Tie:
                                        await commands.TieBetAsync(betData, Context);
                                        break;

                                }

                                Interlocked.Increment(ref txCount);
                            }
                            catch (Exception ex)
                            {
                                errorsList.Add(CreateBatchTransactionError(ex, transaction.Id, transaction.UserId));
                            }
                        }
                    })));
            return txCount;
        }

        private BatchCommandTransactionError CreateBatchTransactionError(Exception exception, string transactionId, Guid userId)
        {
            var betTxId = Guid.Empty;
            var duplicate = exception as DuplicateGameActionException;
            var isDuplicate = duplicate != null;

            if (isDuplicate)
                betTxId = duplicate.GameActionId;

            string errorDescription;
            var errorCode = _errors.GetErrorCodeByException(exception, out errorDescription);

            var error = new BatchCommandTransactionError
            {
                ErrorCode = errorCode,
                ErrorDescription = errorDescription,
                GameActionId = betTxId,
                Id = transactionId,
                IsDuplicate = isDuplicate ? 1 : 0,
                UserId = userId
            };

            return error;
        }

        private Guid GetPlayerIdFromToken(IAuthTokenHolder request)
        {
            return TokenProvider.Decrypt(request.AuthToken);
        }
        
        #region Bet methods

        private async Task<BetCommandResponseTransaction> PlaceBet(Guid playerId, BetCommandTransactionRequest transaction)
        {
            var isDuplicate = 0;
            Guid gameActionId;

            try
            {
                gameActionId = await _gameCommands.PlaceBetAsync(
                    new GameActionData
                    {
                        RoundId = transaction.RoundId,
                        ExternalGameId = transaction.GameId,
                        ExternalTransactionId = transaction.Id,
                        Amount = transaction.Amount,
                        CurrencyCode = transaction.CurrencyCode,
                        Description = transaction.Description,
                    }, Context, playerId);
            }
            catch (DuplicateGameActionException ex)
            {
                gameActionId = ex.GameActionId;
                isDuplicate = 1;
            }

            return new BetCommandResponseTransaction
            {
                GameActionId = gameActionId,
                Id = transaction.Id,
                IsDuplicate = isDuplicate
            };
        }

        private async Task<BetCommandResponseTransaction> SettleBet(BetCommandTransactionRequest transaction, Func<GameActionData, GameActionContext, Task<Guid>> settleMethod, string batchId = null)
        {
            var isDuplicate = 0;
            Guid gameActionId;

            try
            {
                var actionData = new GameActionData
                {
                    RoundId = transaction.RoundId,
                    ExternalTransactionId = transaction.Id,
                    TransactionReferenceId = transaction.ReferenceId,
                    Amount = transaction.Amount,
                    CurrencyCode = transaction.CurrencyCode,
                    Description = transaction.Description,
                    BatchId = batchId,
                    ExternalGameId = transaction.GameId
                };
                gameActionId = await settleMethod(actionData, Context);
            }
            catch (DuplicateGameActionException ex)
            {
                gameActionId = ex.GameActionId;
                isDuplicate = 1;
            }

            return new BetCommandResponseTransaction
            {
                GameActionId = gameActionId,
                Id = transaction.Id,
                IsDuplicate = isDuplicate
            };
        }

        private async Task<BetCommandResponseTransaction> AdjustTransaction(IBetCommandTransactionRequest transaction, string batchId = null)
        {
            var isDuplicate = 0;
            Guid gameActionId;

            try
            {
                gameActionId = await _gameCommands.AdjustTransaction(
                    new GameActionData
                    {
                        RoundId = transaction.RoundId,
                        ExternalTransactionId = transaction.Id,
                        TransactionReferenceId = transaction.ReferenceId,
                        Amount = transaction.Amount,
                        CurrencyCode = transaction.CurrencyCode,
                        Description = transaction.Description,
                        BatchId = batchId,
                    },
                    Context);
            }
            catch (DuplicateGameActionException ex)
            {
                gameActionId = ex.GameActionId;
                isDuplicate = 1;
            }

            return new BetCommandResponseTransaction
            {
                GameActionId = gameActionId,
                Id = transaction.Id,
                IsDuplicate = isDuplicate
            };
        }

        private async Task<BetCommandResponseTransaction> CancelTransaction(IBetCommandTransactionRequest transaction, string batchId = null)
        {
            var isDuplicate = 0;
            Guid gameActionId;

            try
            {
                gameActionId = await _gameCommands.CancelTransactionAsync(
                    new GameActionData
                    {
                        RoundId = transaction.RoundId,
                        ExternalTransactionId = transaction.Id,
                        Amount = 0,
                        TransactionReferenceId = transaction.ReferenceId,
                        Description = transaction.Description,
                        BatchId = batchId,
                    },
                    Context);
            }
            catch (DuplicateGameActionException ex)
            {
                gameActionId = ex.GameActionId;
                isDuplicate = 1;
            }

            return new BetCommandResponseTransaction
            {
                GameActionId = gameActionId,
                Id = transaction.Id,
                IsDuplicate = isDuplicate
            };
        }

        #endregion
    }
}