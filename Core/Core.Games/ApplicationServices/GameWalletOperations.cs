using System;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class GameWalletOperations : IGameWalletOperations
    {
        private readonly IGameRepository _repository;
        private readonly IEventBus _eventBus;

        public GameWalletOperations(IEventBus eventBus, IGameRepository repository)
        {
            _repository = repository;
            _eventBus = eventBus;
        }

        public async Task<Guid> PlaceBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount, string externalTransactionId)
        {
            return await DoGameWalletOperation(playerId, wallet => wallet.PlaceBet(amount, roundId, gameId, externalTransactionId));
        }

        public async Task<Guid> CancelBetAsync(Guid playerId, Guid gameId, Guid roundId, string externalTransactionId, string externalTransactionReferenceId)
        {
            return await DoGameWalletOperation(playerId, wallet => wallet.CancelBet(roundId, externalTransactionId, externalTransactionReferenceId));
        }

        public async Task<Guid> WinBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount, string externalTransactionId)
        {
            return await DoGameWalletOperation(playerId, wallet => wallet.WinBet(roundId, amount, externalTransactionId));
        }

        public async Task<Guid> LoseBetAsync(Guid playerId, Guid gameId, Guid roundId, string externalTransactionId)
        {
            return await DoGameWalletOperation(playerId, wallet => wallet.LoseBet(roundId, externalTransactionId));
        }

        public async Task<Guid> TieBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount, string externalTransactionId)
        {
            return await DoGameWalletOperation(playerId, wallet => wallet.TieBet(roundId, amount, externalTransactionId));
        }

        public async Task<Guid> FreeBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount, string externalTransactionId)
        {
            return await DoGameWalletOperation(playerId, wallet => wallet.FreeBet(amount, roundId, gameId, externalTransactionId)); 
        }

        public Guid AdjustBetTransaction(Guid playerId, Guid gameId, decimal newAmount, string externalTransactionId, string externalTransactionReferenceId)
        {
            //TODO: Adjust bet call is not implemented in the wallet.
            return Guid.Empty;
        }


        private Task<Guid> DoGameWalletOperation(Guid playerId, Func<Entities.Wallet, Transaction> walletAction)
        {
            var wallet = _repository.GetWalletWithUPDLock(playerId);

            var transaction = walletAction.Invoke(wallet);
            wallet.Events.ForEach(ev => _eventBus.Publish(ev));

            return Task.FromResult(transaction.Id);
        }

    }
}
