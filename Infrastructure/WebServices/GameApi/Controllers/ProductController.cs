using System;
using System.Threading.Tasks;
using System.Web.Http;

using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.UGS.Core.Messages.Lobby;
using AFT.UGS.Core.Messages.OAuth;
using AFT.UGS.Core.Messages.Products;

namespace AFT.RegoV2.GameApi.Controllers
{
    public class ProductController : ApiController
    {
        private LocalProductApiClient _localProductApiClient;

        public ProductController(LocalProductApiClient localProductApiClient)
        {
            _localProductApiClient = localProductApiClient;
        }

        [Route("api/products/oauth/token")]
        public async Task<TokenResponse> PostAsync(ClientCredentialsTokenRequest request)
        {
            return await _localProductApiClient.GetTokenAsync(request);
        }

        [Route("api/products/for/player")]
        public async Task<ProductsForPlayerResponse> PostAsync(ProductsForPlayerRequest request)
        {
            return await _localProductApiClient.GetProductsForPlayerAsync(request, Request.Headers.Authorization.Parameter);
        }
    }
}
