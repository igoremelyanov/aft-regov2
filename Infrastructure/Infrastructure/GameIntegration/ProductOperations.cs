using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.UGS.Core.BaseModels.Enums;
using AFT.UGS.Core.Messages.Lobby;
using AFT.UGS.Core.Messages.OAuth;
using AFT.UGS.Core.Messages.Products;
using AFT.UGS.Core.Messages.Products.External;
using AFT.UGS.Core.ProductConsumerClient;

namespace AFT.RegoV2.Infrastructure.GameIntegration
{
    public class ProductOperations : IProductOperations
    {
        private readonly IProductApiClientFactory _productApiClientFactory;
        private readonly IBrandOperations _brandOperations;
        private readonly IGameRepository _gameRepository;

        public ProductOperations(
            IProductApiClientFactory productApiClientFactory,
            IBrandOperations brandOperations,
            IGameRepository gameRepository)
        {
            _productApiClientFactory = productApiClientFactory;
            _brandOperations = brandOperations;
            _gameRepository = gameRepository;
        }

        public async Task<string> GetLobbyTokenAsync(Guid lobbyId)
        {
            var lobby = _gameRepository.Lobbies.Single(x => x.Id == lobbyId);
            var token = await GetApiClient().GetTokenAsync(new ClientCredentialsTokenRequest()
                {
                    client_id = lobby.ClientId,
                    client_secret = lobby.ClientSecret,
                    grant_type = "client_credentials",
                    scope = "playerapi",
                });
            return token.access_token;
        }

        public async Task<ProductsForPlayerResponse> GetProductsForPlayerAsync(Guid lobbyId, Guid playerId, string lobbyUrl, string playerIpAddress, string userAgent)
        {
            var lobby = _gameRepository.Lobbies.Single(x => x.Id == lobbyId);
            var playerToken = _brandOperations.GetPlayerAuthToken(playerId, playerIpAddress, lobby.PlatformType);

            var request = new ProductsForPlayerRequest()
            {
                authtoken = playerToken,
                useragent = userAgent,
                lobbyurl = lobbyUrl.ToLower(),
                ipaddress = playerIpAddress,
            };
            var lobbyToken = await GetLobbyTokenAsync(lobbyId);
            var response = await GetApiClient().GetProductsForPlayerAsync(request, lobbyToken);

            return response;
        }

        protected IProductApiClient GetApiClient()
        {
            return _productApiClientFactory.GetApiClient();
        }
    }
}
