using System;

using AFT.RegoV2.Core.Common.Interfaces;
using AFT.UGS.Core.ProductConsumerClient;

namespace AFT.RegoV2.Infrastructure.GameIntegration
{
    public class ProductApiClientFactory : IProductApiClientFactory
    {
        private ICommonSettingsProvider _settingsProvider;

        public ProductApiClientFactory(ICommonSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public IProductApiClient GetApiClient()
        {
            var operatorApiUrl = _settingsProvider.GetOperatorApiUrl();
            return new ProductApiClient(operatorApiUrl);
        }
    }
}
