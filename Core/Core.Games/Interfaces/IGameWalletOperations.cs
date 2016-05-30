using System;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Game.Interfaces
{
    public interface IGameWalletOperations : IApplicationService
    {
        Task<Guid> PlaceBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount, string externalTransactionId);

        Task<Guid> CancelBetAsync(Guid playerId, Guid gameId, Guid roundId, string externalTransactionId, string externalTransactionReferenceId);

        Task<Guid> WinBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount, string externalTransactionId);

        Task<Guid> LoseBetAsync(Guid playerId, Guid gameId, Guid roundId, string externalTransactionId);

        Task<Guid> TieBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount, string externalTransactionId);

        Task<Guid> FreeBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount, string externalTransactionId);

        Guid AdjustBetTransaction(Guid playerId, Guid gameId, decimal newAmount, string externalTransactionId, string externalTransactionReferenceId);
    }
}
