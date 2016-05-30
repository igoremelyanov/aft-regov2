using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

using AFT.UGS.Core.Messages.OAuth;
using AFT.UGS.Core.Messages.Players;
using AFT.UGS.Core.Messages.Wallets;

using FakeUGS.Core.Data;
using FakeUGS.Core.Exceptions;
using FakeUGS.Core.Interfaces;
using FakeUGS.Extensions;

namespace FakeUGS.Controllers
{
    public class BrandController : ApiController
    {
        private readonly IRepository _repository;
        private readonly IWalletOperations _walletOperations;
        private readonly ITokenProvider _tokenProvider;
        private readonly IPlayerCommands _playerCommands;

        public BrandController(
            IWalletOperations walletOperations,
            IRepository repository,
            ITokenProvider tokenProvider,
            IPlayerCommands playerCommands)
        {
            _repository = repository;
            _walletOperations = walletOperations;
            _tokenProvider = tokenProvider;
            _playerCommands = playerCommands;
        }

        [Route("api/oauth/token")]
        public async Task<TokenResponse> PostAsync(ClientCredentialsTokenRequest request)
        {
            if (request == null)
                throw new InvalidCredentialsException();

            var brand = await _repository.Brands.SingleOrDefaultAsync(x => x.ClientId == request.client_id && x.ClientSecret == request.client_secret);
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

        [Route("api/player/authorize")]
        public async Task<AuthorizePlayerResponse> PostAsync(AuthorizePlayerRequest request)
        {
            if (string.IsNullOrEmpty(request.ipaddress))
            {
                throw new ArgumentException("IpAddress is missing");
            }

            Player player;
            if (string.IsNullOrEmpty(request.userid))
            {
                player = await _repository.Players.SingleOrDefaultAsync(x => x.Name == request.username);
            }
            else
            {
                var playerId = Guid.Parse(request.userid);
                player = await _repository.Players.SingleOrDefaultAsync(x => x.Id == playerId);
            }

            if (player == null)
            {
                var brandId = GetBrandId(ActionContext);
                player = _playerCommands.AddPlayerFromRequest(request, brandId);
            }

            return new AuthorizePlayerResponse
            {
                authtoken = _tokenProvider.Encrypt(player.Id)
            };
        }

        private Guid GetBrandId(HttpActionContext context)
        {
            return context.GetIdFromAuthToken(_tokenProvider);
        }

        [Route("api/player/balance")]
        public Task<PlayerBalanceResponse> GetAsync([FromUri]PlayerBalanceRequest request)
        {
            var playerId = Guid.Parse(request.userid);

            var balance = _walletOperations.GetBalance(playerId);

            return Task.FromResult(new PlayerBalanceResponse
            {
                bal = balance,
                cur = request.cur
            });
        }

        [Route("api/wallet/credit")]
        public Task<WalletActionResponse> PostAsync(WalletCreditRequest request)
        {
            return PerformFundOperation(request, false);
        }

        [Route("api/wallet/debit")]
        public Task<WalletActionResponse> PostAsync(WalletDebitRequest request)
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
