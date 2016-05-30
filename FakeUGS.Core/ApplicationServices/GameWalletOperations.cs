using System;
using System.Threading.Tasks;

using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using FakeUGS.Core.Data;
using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.ApplicationServices
{
    public class GameWalletOperations : IGameWalletOperations
    {
        private readonly IRepository _repository;
        private readonly IUgsServiceBus _serviceBus;

        public GameWalletOperations(IUgsServiceBus serviceBus, IRepository repository)
        {
            _repository = repository;
            _serviceBus = serviceBus;
        }

        public async Task<Guid> PlaceBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount)
        {
            return await DoGameWalletOperation(playerId, wallet => wallet.PlaceBet(amount, roundId, gameId));
        }

        public async Task<Guid> CancelBetAsync(Guid playerId, Guid gameId, Guid transactionId)
        {
            return await DoGameWalletOperation(playerId, wallet => wallet.CancelBet(transactionId));
        }

        public async Task<Guid> WinBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount)
        {
            return await DoGameWalletOperation(playerId, wallet => wallet.WinBet(roundId, amount));
        }

        public async Task<Guid> LoseBetAsync(Guid playerId, Guid gameId, Guid roundId)
        {
            return await DoGameWalletOperation(playerId, wallet => wallet.LoseBet(roundId));
        }

        public async Task<Guid> TieBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount)
        {
            return await DoGameWalletOperation(playerId, wallet => wallet.TieBet(roundId, amount));
        }

        public async Task<Guid> FreeBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount)
        {
            return await DoGameWalletOperation(playerId, wallet => wallet.FreeBet(amount, roundId, gameId)); 
        }

        public Guid AdjustBetTransaction(Guid playerId, Guid gameId, Guid transactionId, decimal newAmount)
        {
            //TODO: Adjust bet call is not implemented in the wallet.
            return Guid.Empty;
        }


        private Task<Guid> DoGameWalletOperation(Guid playerId, Func<Entities.Wallet, Transaction> walletAction)
        {
            var wallet = _repository.GetWalletWithUPDLock(playerId);

            var transaction = walletAction.Invoke(wallet);
            wallet.Events.ForEach(ev =>
                _serviceBus.PublishExternalMessage(ev)
                );

            return Task.FromResult(transaction.Id);
        }

    }
}
