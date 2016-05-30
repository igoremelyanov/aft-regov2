using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AFT.RegoV2.MemberApi.Interface.Exceptions;
using AFT.RegoV2.MemberApi.Interface.GameProvider;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.MemberApi.Tests.Integration.MockHelpers;
using AFT.RegoV2.Tests.Common;
using FluentAssertions;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.MemberApi.Tests.Integration.Steps
{
    [Binding]
    public class GameSteps : BaseSteps
    {

        [Given(@"Access Token for player is available")]
        public async Task GivenAccessTokenForPlayerIsAvailable()
        {
            await LogInMemberApiWithNewUser();
            Token.Should().NotBeNullOrWhiteSpace();
        }

        #region Status 200 expected
        [When(@"I try to send GET request to Game Controller API (.*)")]
        public void WhenITryToSendGETRequestToGameControllerAPI(string p0)
        {
            ServiceLayerMockingHelper.MockGameAndOperationQueriesForHttpStatus200(Container);
        }

        [Then(@"I should see Status Code (.*) Successful for all of the calls")]
        public void ThenIShouldSeeStatusCodeSuccessfulForAllOfTheCalls(int p0)
        {
            Assert.DoesNotThrow(async () => await MemberApiProxy.GameListAsync(new GamesRequest
            {
                IsForMobile = true,
                LobbyUrl = TestDataGenerator.GetRandomString(),
                PlayerIpAddress = TestDataGenerator.GetRandomIpAddress(),
                PlayerUsername = TestDataGenerator.GetRandomString(),
                UserAgent = TestDataGenerator.GetRandomString()
            }));
        }
        #endregion

        #region Status 400 expected
        [When(@"I try to send request to Game Controller with validation errors in the request")]
        public void WhenITryToSendRequestToGameControllerWithValidationErrorsInTheRequest()
        {
            ServiceLayerMockingHelper.MockGameAndOperationQueriesForHttpStatus400(Container);
        }

        [Then(@"I should see Status Code (.*) for all of the calls")]
        public void ThenIShouldSeeStatusCodeForAllOfTheCalls(int p0)
        {
            Assert.Throws<MemberApiValidationException>(async () =>
            await MemberApiProxy.GameListAsync(new GamesRequest
            {
                PlayerUsername = TestDataGenerator.GetRandomString()
            }));
        }
        #endregion

        #region Status 401 expected
        [Given(@"Login with invalid credentials")]
        public async Task GivenLoginWithInvalidCredentials()
        {
            try
            {
                await LogInMemberApi(Guid.NewGuid().ToString().Substring(0, 8),
                                        Guid.NewGuid().ToString().Substring(0, 8));
            }
            catch (Exception ex)
            {
                ScenarioContext.Current[ResultFromInvalidUsernameAndPassword] = ex.Message;
            }
        }


        [When(@"I try to send GET request to Game Controller end point")]
        public void WhenITryToSendGETRequestToGameControllerEndPoint()
        {
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GameListAsync(new GamesRequest())).Exception.ErrorMessage);
        }

        [Then(@"I should see Status Code Unauthorised Request (.*)")]
        public void ThenIShouldSeeStatusCodeUnauthorisedRequest(int p0)
        {
            Assert.That(ErrorMessagesFromUnauthorizedCalls.All(el => el == HttpStatusCode.Unauthorized.ToString()));
        }

        #endregion
    }
}
