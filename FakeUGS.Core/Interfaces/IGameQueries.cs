using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AFT.RegoV2.Core.Common.Interfaces;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.Interfaces
{
    public interface IGameQueries : IApplicationService
    {
        List<Entities.Round>        GetRoundHistory(Guid playerId, Guid gameId, int recordCount);
        Task<string>                GetPlayerBetLimitCodeOrNullAsync(Guid vipLevelId, string gameProviderCode, string currency);
        Task<Player>                GetPlayerDataAsync(Guid playerId);
        PlayableBalanceInfo         GetPlayableBalance(Guid playerId);
        Task<PlayableBalanceInfo>   GetPlayableBalanceAsync(Guid playerId);
        Dictionary<Guid, decimal>   GetPlayableBalances(IEnumerable<Guid> playerIds);
        Task<string>                GetBrandCodeAsync(Guid brandId);
        void                        ValidateBatchIsUnique(string batchId, string gameProviderCode);
        Game                        GetGameByExternalGameId(string externalGameId);
        Task<string>                GetMappedGameProviderCurrencyCodeAsync(string gameProviderCode, string playerCurrencyCode);
        Task<bool>                  ValidateSecurityKey(string gameProviderCode, string securityKey);
    }
}