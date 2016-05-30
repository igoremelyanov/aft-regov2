using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Providers;
using AFT.RegoV2.Infrastructure.GameIntegration;
using AFT.UGS.Core.BrandClient;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class LocalBrandApiClientFactory : IBrandApiClientFactory
    {
        private IGameRepository _gameRepository;
        private IWalletOperations _walletOperations;
        private ITokenProvider _tokenProvider;
        private IBrandCredentialsQueries _brandCredentialsQueries;

        public LocalBrandApiClientFactory(
            IWalletOperations walletOperations, 
            IGameRepository gameRepository, 
            IBrandCredentialsQueries brandCredentialsQueries,
            ITokenProvider tokenProvider)
        {
            _gameRepository = gameRepository;
            _walletOperations = walletOperations;
            _brandCredentialsQueries = brandCredentialsQueries;
            _tokenProvider = tokenProvider;
        }
        
        public IBrandApiClient GetApiClient()
        {
            return new LocalBrandApiClient(_walletOperations, _gameRepository, _brandCredentialsQueries, _tokenProvider);
        }
    }
}
