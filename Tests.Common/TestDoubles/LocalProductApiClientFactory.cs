using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Providers;
using AFT.RegoV2.Infrastructure.GameIntegration;
using AFT.UGS.Core.ProductConsumerClient;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class LocalProductApiClientFactory : IProductApiClientFactory
    {
        private IGameRepository _gameRepository;
        private ITokenProvider _tokenProvider;

        public LocalProductApiClientFactory(
            IGameRepository gameRepository,
            ITokenProvider tokenProvider)
        {
            _gameRepository = gameRepository;
            _tokenProvider = tokenProvider;
        }

        public IProductApiClient GetApiClient()
        {
            return new LocalProductApiClient(_gameRepository, _tokenProvider);
        }
    }
}
