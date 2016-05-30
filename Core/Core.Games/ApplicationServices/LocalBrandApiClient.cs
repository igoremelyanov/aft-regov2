using System;
using System.Data.Entity;
using System.Threading.Tasks;

using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Providers;
using AFT.UGS.Core.BaseModels;
using AFT.UGS.Core.BrandClient;
using AFT.UGS.Core.Messages.OAuth;
using AFT.UGS.Core.Messages.Players;
using AFT.UGS.Core.Messages.Wallets;

using Player = AFT.RegoV2.Core.Game.Interface.Data.Player;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class LocalBrandApiClient : IBrandApiClient
    {
        private readonly IGameRepository _gameRepository;
        private readonly IWalletOperations _walletOperations;
        private readonly ITokenProvider _tokenProvider;

        public LocalBrandApiClient(
            IWalletOperations walletOperations, 
            IGameRepository gameRepository, 
            IBrandCredentialsQueries brandCredentialsQueries,
            ITokenProvider tokenProvider)
        {
            _gameRepository = gameRepository;
            _walletOperations = walletOperations;
            _tokenProvider = tokenProvider;
        }
        
        public IApiClientLogger Logger { get; set; }

        public async Task<TokenResponse> GetTokenAsync(ClientCredentialsTokenRequest request)
        {
            var brand = await _gameRepository.Brands.SingleOrDefaultAsync(x => x.ClientId == request.client_id && x.ClientSecret == request.client_secret);
            if (brand == null)
            {
                throw new InvalidCredentialsException();
            }

            var expiresInSeconds = 3600;
            string rawToken = brand.Id.ToString();

            var tokenResponse = new TokenResponse
            {
                access_token = rawToken,
                expires_in = expiresInSeconds,
                token_type = "Bearer",
                state = request.state,
                scope = request.scope
            };

            return tokenResponse;
        }

        public async Task<TokenResponse> GetTokenWithAuthorizationHeaderAsync(ClientCredentialsTokenRequest request, string header)
        {
            return await GetTokenAsync(request);
        }

        public Task<PlayerBalanceResponse> GetPlayerBalanceAsync(PlayerBalanceRequest request, string brandToken)
        {
            var playerId = Guid.Parse(request.userid);

            var balance = _walletOperations.GetBalance(playerId);

            return Task.FromResult(new PlayerBalanceResponse
            {
                bal = balance,
                cur = request.cur
            });
        }

        public async Task<AuthorizePlayerResponse> AuthorizePlayerAsync(AuthorizePlayerRequest request, string brandToken)
        {
            Player player;
            if (string.IsNullOrEmpty(request.userid))
            {
                player = await _gameRepository.Players.SingleAsync(x => x.Name == request.username);
            }
            else
            {
                var playerId = Guid.Parse(request.userid);
                player = await _gameRepository.Players.SingleAsync(x => x.Id == playerId);
            }
            
            return new AuthorizePlayerResponse
            {
                authtoken = _tokenProvider.Encrypt(player.Id)
            };
        }

        public Task<DeauthorizePlayerResponse> DeauthorizePlayerAsync(DeauthorizePlayerRequest request, string brandToken)
        {
            throw new NotImplementedException();
        }

        public Task<WalletActionResponse> CreditWalletAsync(WalletCreditRequest request, string brandToken)
        {
            return PerformFundOperation(request, false);
        }

        public Task<WalletActionResponse> DebitWalletAsync(WalletDebitRequest request, string brandToken)
        {
            return PerformFundOperation(request, true);
        }

        private Task<WalletActionResponse> PerformFundOperation(WalletActionRequest request, bool isDebit)
        {
            var playerId = Guid.Parse(request.userid);

            var operationResult = isDebit
                ? _walletOperations.FundOut(playerId, request.amt, request.txid)
                : _walletOperations.FundIn(playerId, request.amt, request.txid);

            var result = new WalletActionResponse()
            {
                bal = operationResult.Balance,
                cur = operationResult.Currency,
                txid = operationResult.ExternalTransactionId,
                ptxid = operationResult.PlatformTransactionId,
                dup = operationResult.IsDuplicate
            };

            return Task.FromResult(result);
        }
    }
}
