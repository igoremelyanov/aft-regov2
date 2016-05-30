using System;
using System.Threading.Tasks;

using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using AFT.UGS.Core.BaseModels.Enums;
using AFT.UGS.Core.BrandClient;
using AFT.UGS.Core.Messages.OAuth;
using AFT.UGS.Core.Messages.Players;
using AFT.UGS.Core.Messages.Wallets;

using Player = AFT.RegoV2.Core.Common.Data.Player.Player;

namespace AFT.RegoV2.Infrastructure.GameIntegration
{
    public class BrandOperations : IBrandOperations
    {
        private readonly IBrandApiClientFactory _brandApiClientFactory;
        private readonly IBrandCredentialsQueries _brandCredentialsQueries;
        private readonly IPlayerQueries _playerQueries;
        private readonly IGameQueries _gameQueries;

        public BrandOperations(IBrandApiClientFactory brandApiClientFactory, IBrandCredentialsQueries brandCredentialsQueries, IPlayerQueries playerQueries, IGameQueries gameQueries)
        {
            _brandApiClientFactory = brandApiClientFactory;
            _brandCredentialsQueries = brandCredentialsQueries;
            _playerQueries = playerQueries;
            _gameQueries = gameQueries;
        }

        public async Task<string> GetPlayerAuthTokenAsync(Guid playerId, string playerIpAddress, PlatformType platformType)
        {
            var player = _playerQueries.GetPlayer(playerId);

            var brandToken = await GetBrandTokenAsync(player.BrandId);
            var betLimitGroup = _gameQueries.GetBetLimitGroupByVipLevel(player.VipLevelId);

            var token = await GetApiClient().AuthorizePlayerAsync(new AuthorizePlayerRequest
            {
                userid = player.Id.ToString(),
                ipaddress = playerIpAddress,
                username = player.Username,
                //tag = , // need to know more to implement 
                lang = player.CultureCode,
                cur = player.CurrencyCode,
                betlimitid = betLimitGroup == null ? 0 : betLimitGroup.ExternalId, // this need to be refactored after UGS will implement separation of betlimitgroups by gameproviders
                platformtype = platformType,
                istestplayer = player.InternalAccount,
            }, brandToken);

            return token.authtoken;
        }

        [Obsolete("Need to use GetPlayerAuthTokenAsync instead")]
        public string GetPlayerAuthToken(Guid playerId, string playerIpAddress, PlatformType platformType)
        {
            return Task.Run(() => GetPlayerAuthTokenAsync(playerId, playerIpAddress, platformType)).Result;
        }

        public async Task<decimal> GetPlayerBalanceAsync(Guid playerId, string currencyCode)
        {
            var player = _playerQueries.GetPlayer(playerId);
            await EnsureUgsPlayerAsync(player);

            var brandToken = await GetBrandTokenAsync(player.BrandId);

            var response = await GetApiClient().GetPlayerBalanceAsync(new PlayerBalanceRequest
            {
                userid = playerId.ToString(),
                cur = currencyCode
            }, brandToken);

            return response.bal;
        }

        [Obsolete("Need to use GetPlayerBalanceAsync instead")]
        public decimal GetPlayerBalance(Guid playerId, string currencyCode)
        {
            return Task.Run(() => GetPlayerBalanceAsync(playerId, currencyCode)).Result;
        }

        public async Task<decimal> FundOutAsync(Guid playerId, decimal amount, string currencyCode, string transactionId)
        {
            var player = _playerQueries.GetPlayer(playerId);
            await EnsureUgsPlayerAsync(player);

            var credentials = _brandCredentialsQueries.Get(player.BrandId);
            var brandToken = await GetBrandTokenAsync(credentials.BrandId);

            var response = await GetApiClient().DebitWalletAsync(new WalletDebitRequest
            {
                userid = playerId.ToString(),
                amt = Math.Abs(amount),
                cur = currencyCode,
                txid = transactionId,
                timestamp = DateTimeOffset.Now.ToBrandOffset(credentials.TimezoneId) // OUR ?? timezoneId
            }, brandToken);

            return response.bal;
        }

        [Obsolete("Need to use FundOutAsync instead")]
        public decimal FundOut(Guid playerId, decimal amount, string currencyCode, string transactionId)
        {
            return Task.Run(() => FundOutAsync(playerId, amount, currencyCode, transactionId)).Result;
        }

        public async Task<decimal> FundInAsync(Guid playerId, decimal amount, string currencyCode, string transactionId)
        {
            var player = _playerQueries.GetPlayer(playerId);
            await EnsureUgsPlayerAsync(player);

            var credentials = _brandCredentialsQueries.Get(player.BrandId);
            var brandToken = await GetBrandTokenAsync(credentials.BrandId);

            var response = await GetApiClient().CreditWalletAsync(new WalletCreditRequest
            {
                userid = playerId.ToString(),
                amt = amount,
                cur = currencyCode,
                txid = transactionId,
                timestamp = DateTimeOffset.Now.ToBrandOffset(credentials.TimezoneId) // OUR ?? timezoneId
            }, brandToken);

            return response.bal;
        }

        [Obsolete("Need to use FundInAsync instead")]
        public decimal FundIn(Guid playerId, decimal amount, string currencyCode, string transactionId)
        {
            return Task.Run(() => FundInAsync(playerId, amount, currencyCode, transactionId)).Result;
        }

        public async Task<string> GetBrandTokenAsync(Guid brandId)
        {
            var credentials = _brandCredentialsQueries.Get(brandId);

            var token = await GetApiClient().GetTokenAsync(new ClientCredentialsTokenRequest()
            {
                client_id = credentials.ClientId,
                client_secret = credentials.ClientSecret,
                grant_type = "client_credentials",
                scope = "playerapi"
            });

            return token.access_token;
        }

        protected IBrandApiClient GetApiClient()
        {
            return _brandApiClientFactory.GetApiClient();
        }

        private async Task EnsureUgsPlayerAsync(Player player)
        {
            await GetPlayerAuthTokenAsync(player.Id, player.IpAddress, PlatformType.Desktop);
        }
    }

}
