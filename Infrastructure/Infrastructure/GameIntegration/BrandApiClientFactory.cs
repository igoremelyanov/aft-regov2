using AFT.RegoV2.Core.Common.Interfaces;
using AFT.UGS.Core.BrandClient;

namespace AFT.RegoV2.Infrastructure.GameIntegration
{
    public class BrandApiClientFactory : IBrandApiClientFactory
    {
        private ICommonSettingsProvider _settingsProvider;

        public BrandApiClientFactory(ICommonSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public IBrandApiClient GetApiClient()
        {
            var operatorApiUrl = _settingsProvider.GetOperatorApiUrl();
            return new BrandApiClient(operatorApiUrl);
        }
    }
}
