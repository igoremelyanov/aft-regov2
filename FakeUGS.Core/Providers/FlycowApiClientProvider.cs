using System.Threading.Tasks;

using AFT.RegoV2.Core.Common.Interfaces;
using AFT.UGS.Core.FlyCowClient;
using AFT.UGS.Endpoints.Games.FlyCow.Models;

using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.Providers
{
    public class FlycowApiClientProvider : IFlycowApiClientProvider
    {
        private readonly ICommonSettingsProvider _commonSettingsProvider;
        private readonly IFlycowApiClientSettingsProvider _flycowApiClientSettingsProvider;

        public FlycowApiClientProvider(
            ICommonSettingsProvider commonSettingsProvider,
            IFlycowApiClientSettingsProvider flycowApiClientSettingsProvider)
        {
            _commonSettingsProvider = commonSettingsProvider;
            _flycowApiClientSettingsProvider = flycowApiClientSettingsProvider;
        }

        public IFlyCowApiClient GetApiClient()
        {
            var operatorApiUrl = _commonSettingsProvider.GetOperatorApiUrl();
            return new FlyCowApiClient(operatorApiUrl);
        }

        public async Task<string> GetApiToken(IFlyCowApiClient client)
        {
            var clientId = _flycowApiClientSettingsProvider.GetClientId();
            var clientSecret = _flycowApiClientSettingsProvider.GetClientSecret();

            var token = await client.GetTokenAsync(new OAuthTokenRequest()
            {
                client_id = clientId,
                client_secret = clientSecret,
                grant_type = "client_credentials",
                scope = "bets"
            });

            return token.access_token;
        }
    }
}
