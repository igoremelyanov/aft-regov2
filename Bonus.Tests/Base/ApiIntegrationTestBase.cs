using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using AFT.RegoV2.Bonus.Api;
using AFT.RegoV2.Bonus.Api.Interface.Proxy;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Shared.Synchronization;
using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Base
{
    public abstract class ApiIntegrationTestBase : IntegrationTestBase
    {
        private IDisposable _webServer;
        private HttpClient HttpClient { get; set; }
        public ApiProxy ApiProxy { get; set; }
        protected readonly Guid DefaultBrandId = new Guid("00000000-0000-0000-0000-000000000138");

        public override void BeforeAll()
        {
            base.BeforeAll();

            var apiUrl = GetApiUrl();
            HttpClient = new HttpClient
            {
                BaseAddress = new Uri(apiUrl),
                DefaultRequestHeaders =
                {
                    Accept = { new MediaTypeWithQualityHeaderValue("application/json") }
                }
            };
            _webServer = WebApp.Start<TestStartup>(apiUrl);
        }

        public override void BeforeEach()
        {
            base.BeforeEach();

            LogInApi(Guid.NewGuid());
        }

        protected void LogInApi(Guid actorId)
        {
            ApiProxy = new ApiProxy(HttpClient, "Test", "Test", actorId);
        }

        public override void AfterAll()
        {
            base.AfterAll();

            _webServer.Dispose();
            HttpClient.Dispose();
        }

        private static string GetApiUrl()
        {
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();

            return $"http://localhost:{port}/";
        }

        protected void AssertActionIsForbidden(Func<Task> action)
        {
            var ex = Assert.Throws<HttpException>(async () => await action());
            Assert.That(ex.GetHttpCode(), Is.EqualTo((int)HttpStatusCode.Forbidden));
        }
    }

    public class TestStartup : Startup
    {
        protected override IUnityContainer GetUnityContainer()
        {
            var container = base.GetUnityContainer();

            container.RegisterInstance(new Mock<IBrandOperations>().Object);
            container.RegisterInstance(new Mock<ISynchronizationService>().Object);

            var authQueriesMock = new Mock<IAuthQueries>();
            authQueriesMock.Setup(queries => queries.GetActor(It.IsAny<Guid>())).Returns(() => new ActorData { UserName = "" });
            container.RegisterInstance(authQueriesMock.Object);

            var configurationMock = new Mock<ICommonSettingsProvider>();
            configurationMock
                .Setup(provider => provider.GetBonusApiCredentials())
                .Returns(() => new BonusApiCredentials { ClientId = "Test", ClientSecret = "Test" });
            container.RegisterInstance(configurationMock.Object);

            return container;
        }
    }
}