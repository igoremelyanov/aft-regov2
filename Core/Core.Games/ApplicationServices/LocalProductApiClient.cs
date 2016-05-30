using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Providers;
using AFT.UGS.Core.BaseModels;
using AFT.UGS.Core.Messages.Lobby;
using AFT.UGS.Core.Messages.OAuth;
using AFT.UGS.Core.Messages.Products;
using AFT.UGS.Core.Messages.Products.External;
using AFT.UGS.Core.ProductConsumerClient;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class LocalProductApiClient : IProductApiClient
    {
        private IGameRepository _gameRepository;
        private ITokenProvider _tokenProvider;

        public LocalProductApiClient(
            IGameRepository gameRepository,
            ITokenProvider tokenProvider)
        {
            _gameRepository = gameRepository;
            _tokenProvider = tokenProvider;
        }

        public IApiClientLogger Logger { get; set; }

        public async Task<TokenResponse> GetTokenAsync(ClientCredentialsTokenRequest request)
        {
            var lobby = await _gameRepository.Lobbies.SingleOrDefaultAsync(x => x.ClientId == request.client_id && x.ClientSecret == request.client_secret);
            if (lobby == null)
            {
                throw new InvalidCredentialsException();
            }

            var expiresInSeconds = 3600;
            string rawToken = lobby.Id.ToString();

            return new TokenResponse
            {
                access_token = rawToken,
                expires_in = expiresInSeconds,
                token_type = "Bearer",
                state = request.state,
                scope = request.scope
            };
        }

        public Task<ProductLocalizationResponse> GetLocalizationsAsync(string lobbyToken, string key)
        {
            throw new NotImplementedException();
        }

        public async Task<ProductsForPlayerResponse> GetProductsForPlayerAsync(ProductsForPlayerRequest request, string lobbyToken)
        {
            var lobbyId = _tokenProvider.Decrypt(lobbyToken);
            var playerId = _tokenProvider.Decrypt(request.authtoken);

            var playerInfo = await GetLobbyPlayerAsync(playerId);
            var productsInfo = await GetProductsDataAsync(lobbyId, playerInfo);

            return new ProductsForPlayerResponse()
            {
                products = productsInfo,
                player = playerInfo
            };
        }

        private async Task<ProductsDataResponse> GetProductsDataAsync(Guid lobbyId, PlayerDataResponse playerInfo)
        {
            var player = _gameRepository.Players.Single(x => x.Id == playerInfo.id.Value);

            var lobby = await _gameRepository.Lobbies.Include(x => x.GameGroups).SingleAsync(x => x.Id == lobbyId);
            var brand = await _gameRepository.Brands.Include(x => x.BrandGameProviderConfigurations).SingleAsync(x => x.Id == player.BrandId);

            var gameGroups = lobby.GameGroups.ToList();
            var gameGroupsData = new List<GameGroupSummary>();
            var gameProviders = new List<GameProviderSummary>();

            foreach (var gameGroup in gameGroups)
            {
                var games = gameGroup.GameGroupGames.Select(x => x.Game).ToList();
                if (games.Count == 0)
                {
                    continue;
                }

                var providers = games.Select(x => x.GameProvider).Distinct().ToList();

                var lobbyGameGroup = new GameGroupSummary()
                {
                    code = "",
                    name = gameGroup.Name,
                    order = 0,
                    games = games.Select(x => new GameSummary
                    {
                        id = x.ExternalId,
                        name = x.Name,
                        code = x.Code,
                        url = GetGameUrl(x, playerInfo),
                        isactive = x.IsActive && lobby.IsActive && x.GameProvider.IsActive &&
                            x.GameProvider.GameProviderConfigurations.First(c => c.Id == brand.BrandGameProviderConfigurations
                                .First(bc => bc.GameProviderId == x.GameProviderId).GameProviderConfigurationId).IsActive,
                        //popup_width = ,
                        //popup_height = ,
                        order = 0,
                        iconpath = string.Empty,
                        providercode = x.GameProvider.Code,
                        tags = new string[] { }
                    }).ToArray()
                };

                gameProviders.AddRange(providers.Where(x => gameProviders.All(gp => gp.code != x.Code)).Select(x => new GameProviderSummary
                {
                    code = x.Code,
                    name = x.Name,
                    //betlimitid = 
                }));

                gameGroupsData.Add(lobbyGameGroup);
            }

            return new ProductsDataResponse
            {
                lobbyid = lobby.Id,
                cdnroot = string.Empty,
                iconres = new string[] { },
                groups = gameGroupsData.ToArray()
            };
        }

        private async Task<PlayerDataResponse> GetLobbyPlayerAsync(Guid playerId)
        {
            var player = await _gameRepository.Players.SingleAsync(x => x.Id == playerId);
            var newRawToken = _tokenProvider.Encrypt(playerId);

            return new PlayerDataResponse
            {
                id = player.Id,
                username = player.Name,
                //status = , // ask about it. This is looks like active/inactive
                lang = player.CultureCode,
                cur = player.CurrencyCode,
                //level =  , // need to know more to convert it to ugs BetLimitGroupId (ProductionDataInitializer.CreateBetLimits)
                //wallets = , // ask about do we need it?
                //gameproviders = providers.Select(x=>new GameBaseUrl()
                //{
                //    provider_code = x.Code,
                //    url = brand.BrandGameProviderConfigurations.First(bc => bc.GameProviderId == x.Id).GameProviderConfiguration.Endpoint
                //}).ToArray(),
                token = newRawToken
            };
        }

        private string GetGameUrl(Interface.Data.Game game, PlayerDataResponse playerInfo)
        {
            var player = _gameRepository.Players.Single(x => x.Id == playerInfo.id.Value);
            var brand = _gameRepository.Brands
                .Include(x => x.BrandGameProviderConfigurations)
                .Include(x => x.BrandGameProviderConfigurations.Select(c => c.GameProviderConfiguration))
                .Single(x => x.Id == player.BrandId);
            var providers = _gameRepository.GameProviders.Where(x => x.IsActive).ToList();
            var gameProvider = providers.Single(x => x.Id == game.GameProviderId);

            var url =
                brand.BrandGameProviderConfigurations.First(bc => bc.GameProviderId == gameProvider.Id)
                    .GameProviderConfiguration.Endpoint;

            var token = playerInfo.token;

            if (string.IsNullOrEmpty(url)) throw new ArgumentException();

            url = url + game.EndpointPath;
            url = url.Replace("{GameName}", game.Name);
            url = url.Replace("{GameId}", game.ExternalId);
            url = url.Replace("{GameProviderId}", gameProvider.Id.ToString());

            var limiter = url.IndexOf('?') > 0 ? '&' : '?';
            return url + limiter + "token=" + token;
        }
    }
}
