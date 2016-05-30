using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

using AFT.RegoV2.Core.Common.Interfaces;
using AFT.UGS.Core.Messages.Lobby;
using AFT.UGS.Core.Messages.OAuth;
using AFT.UGS.Core.Messages.Products;
using AFT.UGS.Core.Messages.Products.External;
using AFT.UGS.Core.TokenData;

using FakeUGS.Core.Data;
using FakeUGS.Core.Exceptions;
using FakeUGS.Extensions;
using FakeUGS.Core.Interfaces;
using FakeUGS.Core.Services;

namespace FakeUGS.Controllers
{
    public class ProductController : ApiController
    {
        private readonly IRepository _repository;
        private readonly ITokenProvider _tokenProvider;
        private readonly ICommonSettingsProvider _commonSettingsProvider;

        public ProductController(
            IRepository repository,
            ITokenProvider tokenProvider,
            ICommonSettingsProvider commonSettingsProvider)
        {
            _repository = repository;
            _tokenProvider = tokenProvider;
            _commonSettingsProvider = commonSettingsProvider;
        }

        [Route("api/products/oauth/token")]
        public async Task<TokenResponse> PostAsync(ClientCredentialsTokenRequest request)
        {
            var lobby = await _repository.Lobbies.SingleOrDefaultAsync(x => x.ClientId == request.client_id && x.ClientSecret == request.client_secret);
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

        [Route("api/products/for/player")]
        public async Task<ProductsForPlayerResponse> PostAsync(ProductsForPlayerRequest request)
        {
            var lobbyId = GetLobbyId(ActionContext);
            var playerId = _tokenProvider.Decrypt(request.authtoken);

            var playerInfo = await GetLobbyPlayerAsync(playerId);
            var productsInfo = await GetProductsDataAsync(lobbyId, playerInfo);

            return new ProductsForPlayerResponse()
            {
                products = productsInfo,
                player = playerInfo
            };
        }

        private Guid GetLobbyId(HttpActionContext context)
        {
            return context.GetIdFromAuthToken(_tokenProvider);
        }

        private async Task<ProductsDataResponse> GetProductsDataAsync(Guid lobbyId, PlayerDataResponse playerInfo)
        {
            var player = _repository.Players.Single(x => x.Id == playerInfo.id.Value);
            var lobby = await _repository.Lobbies.Include(x => x.GameGroups).SingleAsync(x => x.Id == lobbyId);
            var brand = await _repository.Brands.Include(x => x.BrandGameProviderConfigurations).SingleAsync(x => x.Id == player.BrandId);

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
                    code = gameGroup.Code,
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
                        iconpath = "Content/images/game1.png",
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

            var membersiteUrl = _commonSettingsProvider.GetMemberWebsiteUrl();

            return new ProductsDataResponse
            {
                lobbyid = lobby.Id,
                cdnroot = membersiteUrl,
                iconres = new string[] {},
                groups = gameGroupsData.ToArray()
            };
        }

        private async Task<PlayerDataResponse> GetLobbyPlayerAsync(Guid playerId)
        {
            var player = await _repository.Players.SingleAsync(x => x.Id == playerId);
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
                //game_providers = providers.Select(x => new GameBaseUrl()
                //{
                //    provider_code = x.Code,
                //    url = brand.BrandGameProviderConfigurations.First(bc => bc.GameProviderId == x.Id).GameProviderConfiguration.Endpoint
                //}).ToArray(),
                //}).ToArray(),
                token = newRawToken
            };
        }

        private string GetGameUrl(Game game, PlayerDataResponse playerInfo)
        {
            var player = _repository.Players.Single(x => x.Id == playerInfo.id.Value);
            var brand = _repository.Brands
                .Include(x => x.BrandGameProviderConfigurations)
                .Include(x => x.BrandGameProviderConfigurations.Select(c => c.GameProviderConfiguration))
                .Single(x => x.Id == player.BrandId);
            var providers = _repository.GameProviders.Where(x => x.IsActive).ToList();
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
