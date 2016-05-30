using System;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.Interfaces
{
    public interface IWalletOperations
    {
        WalletOperationResult FundIn(Guid playerId, decimal amount, string transactionNumber);
        WalletOperationResult FundOut(Guid playerId, decimal amount, string transactionNumber);

        decimal GetBalance(Guid playerId);
    }
}
