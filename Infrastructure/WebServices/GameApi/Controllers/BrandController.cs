using System.Threading.Tasks;
using System.Web.Http;

using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.UGS.Core.Messages.OAuth;
using AFT.UGS.Core.Messages.Players;
using AFT.UGS.Core.Messages.Wallets;

namespace AFT.RegoV2.GameApi.Controllers
{
    public class BrandController : ApiController
    {
        private LocalBrandApiClient _localBrandApiClient;

        public BrandController(LocalBrandApiClient localBrandApiClient)
        {
            _localBrandApiClient = localBrandApiClient;
        }

        [Route("api/oauth/token")]
        public async Task<TokenResponse> PostAsync(ClientCredentialsTokenRequest request)
        {
            return await _localBrandApiClient.GetTokenAsync(request);
        }

        [Route("api/player/authorize")]
        public async Task<AuthorizePlayerResponse> PostAsync(AuthorizePlayerRequest request)
        {
            return await _localBrandApiClient.AuthorizePlayerAsync(request, string.Empty);
        }

        [Route("api/player/balance")]
        public async Task<PlayerBalanceResponse> GetAsync([FromUri]PlayerBalanceRequest request)
        {
            return await _localBrandApiClient.GetPlayerBalanceAsync(request, string.Empty);
        }

        [Route("api/wallet/credit")]
        public async Task<WalletActionResponse> PostAsync(WalletCreditRequest request)
        {
            return await _localBrandApiClient.CreditWalletAsync(request, string.Empty);
        }

        [Route("api/wallet/debit")]
        public async Task<WalletActionResponse> PostAsync(WalletDebitRequest request)
        {
            return await _localBrandApiClient.DebitWalletAsync(request, string.Empty);
        }
    }
}
