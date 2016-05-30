using System;
using System.Threading.Tasks;

namespace FakeUGS.Core.Interfaces
{
    public interface IGameWalletOperations 
    {
        Task<Guid> PlaceBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount);

        Task<Guid> CancelBetAsync(Guid playerId, Guid gameId, Guid transactionId);

        Task<Guid> WinBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount);

        Task<Guid> LoseBetAsync(Guid playerId, Guid gameId, Guid roundId);

        Task<Guid> TieBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount);

        Task<Guid> FreeBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount);

        Guid AdjustBetTransaction(Guid playerId, Guid gameId, Guid transactionId, decimal newAmount);
    }
}
