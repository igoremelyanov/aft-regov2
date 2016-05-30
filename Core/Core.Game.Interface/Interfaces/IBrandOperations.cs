using System;
using System.Threading.Tasks;

using AFT.UGS.Core.BaseModels.Enums;

namespace AFT.RegoV2.Core.Game.Interface.Interfaces
{
    public interface IBrandOperations
    {
        Task<decimal> GetPlayerBalanceAsync(Guid playerId, string currencyCode);

        Task<decimal> FundOutAsync(Guid playerId, decimal amount, string currencyCode, string transactionId);
        decimal FundOut(Guid playerId, decimal amount, string currencyCode, string transactionId);

        Task<decimal> FundInAsync(Guid playerId, decimal amount, string currencyCode, string transactionId);
        decimal FundIn(Guid playerId, decimal amount, string currencyCode, string transactionId);

        Task<string> GetPlayerAuthTokenAsync(Guid playerId, string playerIpAddress, PlatformType platformType);

        decimal GetPlayerBalance(Guid playerId, string currencyCode);

        string GetPlayerAuthToken(Guid playerId, string playerIpAddress, PlatformType platformType);
    }
}
