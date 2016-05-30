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
using FakeUGS.Core.Interfaces;
using FakeUGS.Core.ServiceContracts.OAuth;
using FakeUGS.Extensions;

namespace FakeUGS.Controllers
{
    [ForceJsonFormatter("multipart/form-data", "application/x-www-form-urlencoded")]//additionalMediaTypes
    public class MockGoldDeluxeTokenController : ApiController
    {
        private readonly AuthorizationServer _authServer;
        private readonly Guid _gameProviderId = new Guid("4CCB0717-353C-4050-9221-0667A177E224");

        public MockGoldDeluxeTokenController(IRepository repository)
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

        [Route("api/mockgolddeluxe/oauth/token")]
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
