using System;
using System.Net.Http;
using System.Text;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Http;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.GameApi.ServiceContracts.OAuth;
using AFT.RegoV2.GameApi.Extensions;
using AFT.RegoV2.Infrastructure.Attributes;
using AFT.RegoV2.Infrastructure.OAuth2;
using DotNetOpenAuth.OAuth2;

namespace AFT.RegoV2.GameApi.Controllers
{
    [ForceJsonFormatter("multipart/form-data", "application/x-www-form-urlencoded")]//additionalMediaTypes
    public class MockSportsTokenController : ApiController
    {
        private readonly AuthorizationServer _authServer;
        private readonly Guid _gameProviderId = new Guid("18FB823B-435D-42DF-867E-3BA38ED92060");

        public MockSportsTokenController(IGameRepository repository)
        {
            var authCertificateLocation = HostingEnvironment.MapPath(WebConfigurationManager.AppSettings["CertificateLocation"]);

            var authCryptoKeyPair = CryptoKeyPair.LoadCertificate(authCertificateLocation, WebConfigurationManager.AppSettings["CertificatePassword"]);

            var gameProviderStore = new GameProviderOAuthStore(repository);

            var regoAuthServer = new GameApiOAuthServer(new CryptoKeyStore(),
                authCryptoKeyPair,
                authCryptoKeyPair,
                gameProviderStore,
                _gameProviderId);
            _authServer = new AuthorizationServer(regoAuthServer);
        }
        [Route("api/sports/oauth/token")]
        public HttpResponseMessage Post([FromBody]OAuth2Token request)
        {
            var result = _authServer.HandleTokenRequest(Request.GetRequestBase());

            return new HttpResponseMessage
            {
                Content = new StringContent(result.Body, Encoding.UTF8, "application/json")
            };
        }
    }
}
