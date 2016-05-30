using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using AFT.RegoV2.Shared;

using FakeUGS.Core.Data;
using FakeUGS.Core.Exceptions;
using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.ApplicationServices
{
    public sealed class GameQueries : MarshalByRefObject, IGameQueries
    {
        private readonly IRepository _gameRepository;
        private readonly IGameWalletQueries _walletQueries;

        public GameQueries(
            IRepository gameRepository,
            IGameWalletQueries walletQueries)
        {
            _gameRepository = gameRepository;
            _walletQueries = walletQueries;
        }

        PlayableBalanceInfo IGameQueries.GetPlayableBalance(Guid playerId)
        {
            var playerBalance = _walletQueries.GetPlayerBalance(playerId);

            return new PlayableBalanceInfo
            {
                Balance = playerBalance.Amount,
                CurrencyCode = playerBalance.CurrencyCode
            };
        }

        async Task<PlayableBalanceInfo> IGameQueries.GetPlayableBalanceAsync(Guid playerId)
        {
            var playerBalance = await _walletQueries.GetPlayerBalanceAsync(playerId);

            return new PlayableBalanceInfo
            {
                Balance = playerBalance.Amount,
                CurrencyCode = playerBalance.CurrencyCode
            };
        }

        Dictionary<Guid, decimal> IGameQueries.GetPlayableBalances(IEnumerable<Guid> playerIds)
        {
            var players = playerIds.ToList();

            return _walletQueries.GetPlayersBalanceAsync(players);
        }


        List<Entities.Round> IGameQueries.GetRoundHistory(Guid playerId, Guid gameId, int recordCount)
        {
            return _gameRepository
                .GetPlayerRounds(playerId)
                .Where(round => round.Data.GameId == gameId)
                .OrderByDescending(x => x.Data.CreatedOn)
                .Take(recordCount)
                .ToList();
        }

        void IGameQueries.ValidateBatchIsUnique(string batchId, string gameProviderCode)
        {
            var gameProviderId =
                _gameRepository.GameProviders.Where(x => x.Code == gameProviderCode).Select(x => x.Id).FirstOrDefault();
            if (_gameRepository.DoesBatchIdExist(batchId, gameProviderId))
            {
                throw new DuplicateBatchException();
            }
        }

        async Task<bool> IGameQueries.ValidateSecurityKey(string gameProviderCode, string securityKey)
        {
            var gameProviderId =
                await _gameRepository.GameProviders.Where(x => x.Code == gameProviderCode).Select(x => x.Id).FirstOrDefaultAsync();
            return
                await
                    _gameRepository.GameProviderConfigurations.AnyAsync(
                        x => x.GameProviderId == gameProviderId && 
                            x.SecurityKey == securityKey);
        }

        public Game GetGameByExternalGameId(string externalGameId)
        {
            return _gameRepository.Games.FirstOrDefault(x => x.ExternalId == externalGameId);
        }


        async Task<Player> IGameQueries.GetPlayerDataAsync(Guid playerId)
        {
            return await _gameRepository.Players.FirstOrDefaultAsync(x => x.Id == playerId);
        }
        async Task<string> IGameQueries.GetPlayerBetLimitCodeOrNullAsync(Guid vipLevelId, string gameProviderCode, string currency)
        {
            return await GetPlayerBetLimitCodeQuery(vipLevelId, gameProviderCode, currency).SingleOrDefaultAsync();
        }

        private IQueryable<string> GetPlayerBetLimitCodeQuery(Guid vipLevelId, string gameProviderCode, string currency)
        {
            var gameProviderId =
                _gameRepository.GameProviders.Where(x => x.Code == gameProviderCode)
                    .Select(x => x.Id)
                    .FirstOrDefault();

            return _gameRepository.VipLevelBetLimits
                .Where(x => x.VipLevelId == vipLevelId 
                            &&  x.GameProviderId == gameProviderId 
                            &&  x.CurrencyCode == currency)
                .Select(x =>x.BetLimit.Code);
        }

        async Task<string> IGameQueries.GetBrandCodeAsync(Guid brandId)
        {
            return await _gameRepository.Brands.Where(x => x.Id == brandId).Select(b => b.Code).SingleOrDefaultAsync();
        }

       
        async Task<string> IGameQueries.GetMappedGameProviderCurrencyCodeAsync(string gameProviderCode, string innerCurrencyCode)
        {
            var mappedCurrencyCode = await (from gpc in _gameRepository.GameProviderCurrencies
                                           join gp in _gameRepository.GameProviders on gpc.GameProviderId equals gp.Id
                                           where gp.Code == gameProviderCode && gpc.CurrencyCode == innerCurrencyCode
                                           select gpc.GameProviderCurrencyCode).SingleOrDefaultAsync();

            return mappedCurrencyCode ?? innerCurrencyCode;
        }

    }

    public class PlayerWalletNotFoundException : RegoException
    {
        public PlayerWalletNotFoundException(string message) : base(message) { }
    }
}