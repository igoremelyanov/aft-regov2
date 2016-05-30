using System.Web.Http;
using FakeUGS.Attributes;
using FakeUGS.Classes;
using FakeUGS.Core.Exceptions;
using FakeUGS.Core.Interfaces;
using FakeUGS.Core.Services;

using AFT.UGS.Endpoints.Games.FlyCow.Models;

namespace FakeUGS.Controllers
{
    [RoutePrefix("api/flycow")]
    public sealed class FlycowApiClientController : CommonGameProviderController
    {
        public FlycowApiClientController(
            ICommonGameActionsProvider gameActions, 
            IFlycowApiClientSettingsProvider flycowApiClientSettingsProvider) 
            : base(gameActions, flycowApiClientSettingsProvider)
        {
        }

        [Route("oauth/token"), ProcessError]
        public OAuthTokenResponse Post(OAuthTokenRequest request)
        {
            var clientId = FlycowApiClientSettingsProvider.GetClientId();
            var clientSecret = FlycowApiClientSettingsProvider.GetClientSecret();

            if (clientId != request.client_id || clientSecret != request.client_secret)
            {
                throw new InvalidCredentialsException();
            }

            var expiresInSeconds = 3600;
            return new OAuthTokenResponse()
            {
                access_token = clientId,
                expires_in = expiresInSeconds,
                token_type = "Bearer",
                scope = request.scope
            };
        }
    }
}
