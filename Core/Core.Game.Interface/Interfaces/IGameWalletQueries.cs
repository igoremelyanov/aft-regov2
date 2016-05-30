using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Game.Interface.Interfaces
{
    public interface IGameWalletQueries : IApplicationService
    {
        GameWalletBalance GetPlayerBalance(Guid playerId);
        Task<GameWalletBalance> GetPlayerBalanceAsync(Guid playerId);

        Dictionary<Guid, decimal> GetPlayersBalanceAsync(IEnumerable<Guid> playerIds);
    }

    public class GameWalletBalance
    {
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
    }
}
