using System;
using System.Linq;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;
using Wallet = AFT.RegoV2.Core.Game.Entities.Wallet;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class WalletOperations : IWalletOperations
    {
        private readonly IGameRepository _repository;
        private readonly IEventBus _eventBus;

        public WalletOperations(IGameRepository repository, IEventBus eventBus)
        {
            _repository = repository;
            _eventBus = eventBus;
        }

        public WalletOperationResult FundIn(Guid playerId, decimal amount, string transactionNumber)
        {
            return PerformFundOperation(playerId, amount, transactionNumber, true);
        }

        public WalletOperationResult FundOut(Guid playerId, decimal amount, string transactionNumber)
        {
            return PerformFundOperation(playerId, amount, transactionNumber, false);
        }

        public decimal GetBalance(Guid playerId)
        {
            var wallet = _repository.GetWalletWithUPDLock(playerId);

            return wallet.Data.Balance;
        }

        private WalletOperationResult PerformFundOperation(Guid playerId, decimal amount, string transactionNumber, bool isFundIn)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallet = _repository.GetWalletWithUPDLock(playerId);

                var transaction = wallet.Data.Transactions.FirstOrDefault(x => x.TransactionNumber == transactionNumber);
                if (transaction != null)
                {
                    return GetOperationResult(wallet, transaction, true);
                }

                transaction = isFundIn ? wallet.FundIn(amount, transactionNumber) : wallet.FundOut(amount, transactionNumber);

                _repository.SaveChanges();
                wallet.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();

                return GetOperationResult(wallet, transaction, false);
            }
        }

        private WalletOperationResult GetOperationResult(Wallet wallet, Transaction transaction, bool isDuplicate)
        {
            return new WalletOperationResult
            {
                Balance = wallet.Data.Balance,
                Currency = wallet.Data.CurrencyCode,
                ExternalTransactionId = transaction.TransactionNumber,
                PlatformTransactionId = transaction.Id.ToString(),
                IsDuplicate = isDuplicate
            };
        }
    }
}
