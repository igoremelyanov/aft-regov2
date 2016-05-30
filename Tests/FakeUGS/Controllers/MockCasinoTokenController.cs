using System;
using System.Net.Http;
using System.Text;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Http;
using AFT.RegoV2.Infrastructure.Attributes;
using AFT.RegoV2.Infrastructure.OAuth2;
using DotNetOpenAuth.OAuth2;
using FakeUGS.Classes;
using FakeUGS.Core.Classes;
using FakeUGS.Core.DataAccess;
using FakeUGS.Core.Interfaces;
using FakeUGS.Core.ServiceContracts.OAuth;
using FakeUGS.Extensions;

namespace FakeUGS.Controllers
{
    [ForceJsonFormatter("multipart/form-data", "application/x-www-form-urlencoded")]//additionalMediaTypes
    public class MockCasinoTokenController : ApiController
    {
        private readonly AuthorizationServer _authServer;
        private readonly Guid _gameProviderId = new Guid("1814418D-BC00-43B4-AD18-BEBEF6501D7F");

        public MockCasinoTokenController(IRepository repository)
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

        [Route("api/mock/oauth/token")]
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
