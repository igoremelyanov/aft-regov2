using System;

using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Core.Game.Interfaces
{
    public interface IWalletOperations : IApplicationService
    {
        WalletOperationResult FundIn(Guid playerId, decimal amount, string transactionNumber);
        WalletOperationResult FundOut(Guid playerId, decimal amount, string transactionNumber);

        decimal GetBalance(Guid playerId);
    }
}
