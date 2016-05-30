using System;
using System.Threading.Tasks;

using AFT.RegoV2.Core.Common.Interfaces;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.Interfaces
{
    public interface IGameCommands : IApplicationService
    {
        Task<Guid> PlaceBetAsync(GameActionData data, GameActionContext context, Guid playerId);
        Task<Guid> FreeBetAsync(GameActionData data, GameActionContext context, Guid? playerId = null);
        Task<Guid> WinBetAsync(GameActionData data, GameActionContext context);
        Task<Guid> LoseBetAsync(GameActionData data, GameActionContext context);
        Task<Guid> TieBetAsync(GameActionData data, GameActionContext context);
        Task<Guid> AdjustTransaction(GameActionData data, GameActionContext context);
        Task<Guid> CancelTransactionAsync(GameActionData data, GameActionContext context);
    }
}