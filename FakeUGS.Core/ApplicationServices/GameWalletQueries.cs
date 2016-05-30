using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FakeUGS.Core.Data;
using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.ApplicationServices
{
    public class GameWalletQueries : IGameWalletQueries
    {
        private readonly IRepository _repository;

        public GameWalletQueries(IRepository repository)
        {
            _repository = repository;
        }

        public GameWalletBalance GetPlayerBalance(Guid playerId)
        {
            var wallet = _repository.GetWalletWithUPDLock(playerId);
            var currencyCode = _repository.Players.Single(x => x.Id == playerId).CurrencyCode;

            return new GameWalletBalance
            {
                CurrencyCode = currencyCode,
                Amount = wallet.Data.Balance
            };
        }

        public async Task<GameWalletBalance> GetPlayerBalanceAsync(Guid playerId)
        {
            var wallet = await _repository.GetWalletWithUPDLockAsync(playerId);
            var currencyCode = _repository.Players.Single(x => x.Id == playerId).CurrencyCode;

            return new GameWalletBalance
            {
                CurrencyCode = currencyCode,
                Amount = wallet.Data.Balance
            };
        }

        public Dictionary<Guid, decimal> GetPlayersBalanceAsync(IEnumerable<Guid> playerIds)
        {
            return _repository
                .Wallets
                .Where(x => playerIds.Contains(x.PlayerId) )
                .Select(x => new { x.PlayerId, Data = x})
                .ToDictionary(x => x.PlayerId, x => x.Data.Balance);
        }

        public IQueryable<Wallet> GetProductWalletsOfPlayer(Guid playerId)
        {
            return _repository.Wallets.Where(x => x.PlayerId == playerId);
        }
    }
}