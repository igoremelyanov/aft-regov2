using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.MemberApi.Interface.Common;
using AFT.RegoV2.MemberApi.Interface.GameProvider;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Controllers
{
    public class GamesController : BaseApiController
    {
        private readonly IGameQueries _gameQueries;
        private readonly IProductOperations _productOperations;

        public GamesController(IGameQueries gameQueries, IProductOperations productOperations)
        {
            _gameQueries = gameQueries;
            _productOperations = productOperations;
        }

        [HttpPost]
        public async Task<GameListResponse> GameList(GamesRequest request)
        {
            var player = await _gameQueries.GetPlayerByUsernameAsync(request.PlayerUsername);
            if (player == null)
                throw new RegoValidationException(ErrorMessagesEnum.PlayerWithRequestedIdDoesntExist.ToString());

            var brandId = _gameQueries.GetPlayerData(player.Id).BrandId;
            if (brandId == null)
                throw new RegoValidationException(ErrorMessagesEnum.NoBrandRelatedToThisPlayer.ToString());

            var lobby = _gameQueries.GetLobby(brandId, request.IsForMobile);
            if (lobby == null)
                throw new RegoValidationException(ErrorMessagesEnum.LobbyIsMissingForThisBrand.ToString());

            var data = await _productOperations.GetProductsForPlayerAsync(lobby.Id, player.Id, request.LobbyUrl, request.PlayerIpAddress, request.UserAgent);

            return new GameListResponse
            {
                GameProviders = data.products.groups.SelectMany(x => x.games).GroupBy(x => x.providercode, x => x)
                    .Select(gameProvider => new GameProviderData
                    {
                        Code = gameProvider.Key,
                        Games = data.products.groups
                            .SelectMany(group => group.games)
                            .Where(game => game.providercode == gameProvider.Key)
                            .Select(game => new GameData
                            {
                                Id = game.id,
                                Name = game.name,
                                Url = game.url
                            }).ToList()
                    }).ToList()
            };
        }

		[HttpGet]
	    public IEnumerable<GameDTO> GameDtos()
		{
			return _gameQueries.GetGameDtos();
		}

        [HttpPost]
        public async Task<GamesResponse> GamesData(GamesRequest request)
        {
            var player = await _gameQueries.GetPlayerByUsernameAsync(request.PlayerUsername);

            var brandId = _gameQueries.GetPlayerData(player.Id).BrandId;
            var lobby = _gameQueries.GetLobby(brandId, request.IsForMobile);

            var data = await _productOperations.GetProductsForPlayerAsync(lobby.Id, player.Id, request.LobbyUrl, request.PlayerIpAddress, request.UserAgent);
            
            return new GamesResponse()
            {
                CdnRoot = data.products.cdnroot,
                Iconres = data.products.iconres,
                GameGroups = data.products.groups.Select(gamegroup => new GameGroupDto()
                {
                    Name = gamegroup.name,
                    Code = gamegroup.code,
                    Order = gamegroup.order,
                    Games = gamegroup.games.Select(game => new GameDto()
                    {
                        Id = game.id,
                        IconPath = game.iconpath,
                        Name = game.name,
                        Code = game.code,
                        Url = game.url,
                        Order = game.order,
                        ProviderCode = game.providercode,
                        IsActive = game.isactive
                    }).ToArray()
                }).ToArray()
            };
        }
    }
}