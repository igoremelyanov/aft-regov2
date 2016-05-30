using System;
using System.Linq;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;
using FakeUGS.Core.Data;
using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.Services
{
    public class WalletOperations : IWalletOperations
    {
        private readonly IRepository _repository;
        private readonly IUgsServiceBus _serviceBus;

        public WalletOperations(IRepository repository, IUgsServiceBus serviceBus)
        {
            _repository = repository;
            _serviceBus = serviceBus;
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

                // PublishMessage has to know the exact event type!
                wallet.Events.ForEach(@event =>_serviceBus.PublishExternalMessage(@event));

                scope.Complete();

                return GetOperationResult(wallet, transaction, false);
            }
        }

        private WalletOperationResult GetOperationResult(Entities.Wallet wallet, Transaction transaction, bool isDuplicate)
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
