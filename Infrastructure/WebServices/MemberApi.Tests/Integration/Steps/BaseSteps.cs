using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AFT.RegoV2.MemberApi.Interface;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.MemberApi.Tests.Integration.Core;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using FluentValidation.Results;
using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using TechTalk.SpecFlow;
namespace AFT.RegoV2.MemberApi.Tests.Integration.Steps
{
    [Binding]
    public class BaseSteps : IntegrationSpecFlowTestsBase
    {
        private const int PortRangeStart = 2368;
        private const int PortRangeEnd = 64536;

        protected int NumberOfInternalServerErrors = 0;

        private IDisposable _webServer;
        protected MemberApiProxy MemberApiProxy { get; set; }
        protected string Token { get; set; }
        protected string MemberApiUrl { get; set; }
        protected static Guid PlayerId;
        protected static RegistrationDataForMemberWebsite LoggedInPlayerMetadata;

        protected RegoValidationException ValidationException;
        protected List<string> ErrorMessagesFromUnauthorizedCalls;

        private PlayerTestHelper _playerTestHelper;
        protected SecurityTestHelper SecurityHelper { get; private set; }

        protected const string ResultFromInvalidUsernameAndPassword = "ResultFromInvalidUsernameAndPassword";

        protected Guid BrandId
        {
            get
            {
                return new Guid("00000000-0000-0000-0000-000000000138");
            }
        }

        [BeforeScenario]
        public void Before()
        {
            SecurityHelper = Container.Resolve<SecurityTestHelper>();

            ValidationException =
                new RegoValidationException(
                    new ValidationResult(new[] { new ValidationFailure("brandName", "InvalidBrandCode") }));

            ErrorMessagesFromUnauthorizedCalls = new List<string>();

            _playerTestHelper = Container.Resolve<PlayerTestHelper>();

            TestStartup.Container = Container;

            MemberApiUrl = GetAvailableUrl(PortRangeStart, PortRangeEnd);

            _webServer = WebApp.Start<TestStartup>(MemberApiUrl);

            MemberApiProxy = new MemberApiProxy(MemberApiUrl);

        }

        [AfterScenario]
        public void After()
        {
            _webServer.Dispose();
        }

        protected async Task LogInMemberApi(string username, string password)
        {
            var response = await MemberApiProxy.Login(new LoginRequest
            {
                BrandId = BrandId,
                Username = username,
                IPAddress = "::1",
                Password = password,
                RequestHeaders = new Dictionary<string, string>()
            });

            Token = response.AccessToken;
        }

        protected void SetInvalidToken()
        {
            Token = TestDataGenerator.GetRandomString(300);

            MemberApiProxy = new MemberApiProxy(MemberApiUrl, Token);
        }

        protected async Task LogInMemberApiWithNewUser()
        {
            // create a player
            var player = _playerTestHelper.CreatePlayerForMemberWebsite(currencyCode: "CAD");
            LoggedInPlayerMetadata = player;
            PlayerId = _playerTestHelper.GetPlayerByUsername(player.Username);

            await LogInMemberApi(player.Username, player.Password);
        }

        #region Helper methods
        protected void AssignDefaultHeaderValues(HttpClient client)
        {
            client.BaseAddress = new Uri(MemberApiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected void CheckUpdatedEntityAvailability(HttpClient client, string action, string query)
        {
            Assert.DoesNotThrow(async () => await client.SecureGetAsync(Token, action, query));
        }
        #endregion
    }
}