using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AFT.RegoV2.MemberApi.Interface;
using AFT.RegoV2.MemberApi.Interface.Bonus;
using AFT.RegoV2.MemberApi.Interface.Exceptions;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.MemberApi.Tests.Integration.MockHelpers;
using AFT.RegoV2.Tests.Common;
using FluentAssertions;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.MemberApi.Tests.Integration.Steps
{
    [Binding]
    public class BonusSteps : BaseSteps
    {
        private const string StatusCodeFromMemberApiRequest = "StatusCodeFromMemberApiRequest";
        private const string ClaimedBonusRedemption = "ClaimedBonusRedemption";


        public BonusSteps()
        {
        }

        [Given(@"User with valid credentials")]
        public async Task GivenUserWithValidCredentials()
        {
            await LogInMemberApiWithNewUser();
            Token.Should().NotBeNull();
        }

        #region Status 200 expected after GET request
        [When(@"I try to send GET request to Bonus  Controller API (.*)")]
        public void WhenITryToSendGETRequestToBonusControllerAPI(string p0)
        {
            ServiceLayerMockingHelper.MockBonusApiProxyForHttpStatus200(Container);
        }

        [Then(@"I should see Status Code (.*) Successful and Response schema validated")]
        public void ThenIShouldSeeStatusCodeSuccessfulAndResponseSchemaValidated(int p0)
        {
            Assert.DoesNotThrow(async () => await MemberApiProxy.BonusRedemptionsAsync());
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetRedemption(Guid.NewGuid()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetQualifiedBonuses(new QualifiedBonusRequest() { Amount = 10 }));
            Assert.DoesNotThrow(async () => await MemberApiProxy.QualifiedFirstDepositBonuses(new QualifiedBonusRequest() { Amount = 10 }));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetFirstDepositBonuseByCode(new FirstDepositApplicationRequest()));
            Assert.DoesNotThrow(async () =>
                await MemberApiProxy.ValidateFirstOnlineDeposit(new FirstDepositApplicationRequest()
                {
                    BonusCode = TestDataGenerator.GetRandomString(4),
                    DepositAmount = 50
                }));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetCompleteBonuses());
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetBonusesWithIncompleteWagering());
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetVisibleDepositQualifiedBonuses(new QualifiedBonusRequest()
            {
                Amount = 10
            }));
        }
        #endregion

        #region Status 201 expected after POST request
        [When(@"I try to send POST request to Bonus Controller  API (.*)")]
        public void WhenITryToSendPOSTRequestToBonusControllerAPI(string p0)
        {
            ServiceLayerMockingHelper.MockBonusApiProxyForHttpStatus201(Container);
        }

        [Then(@"I should see Status Code (.*) Created and Response schema validated")]
        public async Task ThenIShouldSeeStatusCodeCreatedAndResponseSchemaValidated(int p0)
        {
            //Act
            ScenarioContext.Current[ClaimedBonusRedemption] =
                await MemberApiProxy.ClaimRedemptionAsync(new ClaimRedemptionRequest());

            //From this point on we check if the returned by the POST uri is not empty
            ScenarioContext.Current[ClaimedBonusRedemption].Should().NotBeNull();

            var uriToClaimedRedemption = ((ClaimRedemptionResponse)
                (ScenarioContext.Current[ClaimedBonusRedemption]))
                .UriToClaimedRedemption;

            uriToClaimedRedemption.Should()
                .NotBeEmpty();

            //From this point on we check if the returned by the POST uri is accurate and working
            using (var client = new HttpClient())
            {
                AssignDefaultHeaderValues(client);

                CheckUpdatedEntityAvailability(client,
                    uriToClaimedRedemption.Split('?')[0],
                    uriToClaimedRedemption.Split('?')[1]);
            }
        }
        #endregion

        #region Status 400 expected
        [When(@"I try to send request with invalid parameters to Bonus Controller  API (.*)")]
        public void WhenITryToSendRequestWithInvalidParametersToBonusControllerAPI(string p0)
        {
            ServiceLayerMockingHelper.MockBonusApiProxyForHttpStatus400(Container);
        }

        [Then(@"I should see Status Code (.*) Bad Request")]
        public void ThenIShouldSeeStatusCodeBadRequest(int p0)
        {
            Assert.Throws<MemberApiValidationException>(async () => await MemberApiProxy.GetRedemption(Guid.NewGuid()));
            Assert.Throws<MemberApiValidationException>(
                async () => await MemberApiProxy.GetFirstDepositBonuseByCode(new FirstDepositApplicationRequest()
                {
                    DepositAmount = 10,
                    BonusCode = Guid.NewGuid().ToString(),
                    PlayerId = Guid.NewGuid()
                }));

            Assert.Throws<MemberApiValidationException>(async () => await MemberApiProxy.GetVisibleDepositQualifiedBonuses(new QualifiedBonusRequest()
            {
                Amount = 100
            }));
        }
        #endregion

        #region Status 401 expected
        [Given(@"User with invalid login password combination tries to use the system")]
        public async Task GivenUserWithInvalidLoginPasswordCombination()
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

        [When(@"I try to send request to Bonus Controller  API (.*)")]
        public void WhenITryToSendRequestToBonusControllerAPI(string p0)
        {
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.BonusRedemptionsAsync()).Exception.ErrorMessage);

            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetQualifiedBonuses(new QualifiedBonusRequest())).Exception.ErrorMessage);

            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.QualifiedFirstDepositBonuses(new QualifiedBonusRequest())).Exception.ErrorMessage);

            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetFirstDepositBonuseByCode(new FirstDepositApplicationRequest())).Exception.ErrorMessage);

            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.ValidateFirstOnlineDeposit(new FirstDepositApplicationRequest())).Exception.ErrorMessage);

            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetCompleteBonuses()).Exception.ErrorMessage);

            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetBonusesWithIncompleteWagering()).Exception.ErrorMessage);

            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetVisibleDepositQualifiedBonuses(new QualifiedBonusRequest() {Amount = 10})).Exception.ErrorMessage);
        }

        [Then(@"I should see Status Code (.*) Unauthorised")]
        public void ThenIShouldSeeStatusCodeUnauthorised(int p0)
        {
            Assert.That(ErrorMessagesFromUnauthorizedCalls.All(el => el == HttpStatusCode.Unauthorized.ToString()));
        }
        #endregion

        #region Status 404 expected
        [When(@"I try to send request to Bonus Controller API which has invalid URI")]
        public async Task WhenITryToSendRequestToBonusControllerAPIWhichHasInvalidURI()
        {
            using (var client = new HttpClient())
            {
                AssignDefaultHeaderValues(client);

                var response = await client.SecureGetAsync(Token, Guid.NewGuid().ToString());
                ScenarioContext.Current[StatusCodeFromMemberApiRequest] = response.StatusCode;
            }
        }

        [Then(@"I should see Status Code (.*) Not Found")]
        public void ThenIShouldSeeStatusCodeNotFound(int p0)
        {
            Assert.AreEqual(ScenarioContext.Current[StatusCodeFromMemberApiRequest], HttpStatusCode.NotFound);
        }
        #endregion

        #region Status 500 expected
        [When(@"I try to send request to Bonus Controller and service is not available")]
        public void WhenITryToSendRequestToBonusControllerAndServiceIsNotAvailable()
        {
            ServiceLayerMockingHelper.MockBonusApiProxyForHttpStatus500(Container);
            ServiceLayerMockingHelper.MockLoggingServiceForHttpStatus500(Container);
        }

        [Then(@"I should see Status for not available service")]
        public void ThenIShouldSeeStatusForNotAvailableService()
        {
            Assert.That(
                Assert.Throws<MemberApiProxyException>(async () => await MemberApiProxy.GetCompleteBonuses())
                    .StatusCode,
                Is.EqualTo(HttpStatusCode.InternalServerError));

            Assert.That(
                Assert.Throws<MemberApiProxyException>(async () => await MemberApiProxy.GetBonusesWithIncompleteWagering())
                    .StatusCode,
                Is.EqualTo(HttpStatusCode.InternalServerError));

            ServiceLayerMockingHelper.VerifyLogginServiceWasCalledAfterInternalServerError(2);
        }

        #endregion
    }
}
