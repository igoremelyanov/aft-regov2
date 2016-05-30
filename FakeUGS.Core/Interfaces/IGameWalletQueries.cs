using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FakeUGS.Core.Interfaces
{
    public interface IGameWalletQueries
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
