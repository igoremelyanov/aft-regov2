using System;
using System.Linq;
using AFT.RegoV2.Core.Game.Interface.Data;
using AFT.RegoV2.Core.Game.Interfaces;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class BrandCredentialsQueries : IBrandCredentialsQueries
    {
        private IGameRepository _gameRepository;
        
        public BrandCredentialsQueries(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public BrandCredentials Get(Guid brandId)
        {
            var brand = _gameRepository.Brands.Single(x => x.Id == brandId);
            return new BrandCredentials(brand.Id, brand.ClientId, brand.ClientSecret, brand.TimezoneId);
        }
    }
}
