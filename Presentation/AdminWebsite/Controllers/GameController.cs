using System;
using System.Linq;
using AFT.RegoV2.Core.Game.Interfaces;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class GameController : BaseController
    {
        private readonly IGameQueries _gameQueries;

        public GameController(IGameQueries gameQueries)
        {
            _gameQueries = gameQueries;
        }

        public string BetLimits(Guid gameProviderId, Guid brandId)
        {
            var limits = _gameQueries.GetBetLimits(gameProviderId, brandId).OrderBy(x => x.Name);
            return SerializeJson(new
            {
                BetLimits = limits
            });
        }

        public string GameProviders()
        {
            var gameProviders = _gameQueries.GetGameProviderDtos();
            return SerializeJson(new
            {
                GameProviders = gameProviders
            });
        }
    }
}