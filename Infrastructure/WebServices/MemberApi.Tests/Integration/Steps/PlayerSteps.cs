using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AFT.RegoV2.MemberApi.Interface.Payment;
using AFT.RegoV2.Core.Payment.Interface.Helpers;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Core.Security.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Interface.Data;
using AFT.RegoV2.MemberApi.Interface;
using AFT.RegoV2.MemberApi.Interface.Exceptions;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.MemberApi.Tests.Integration.MockHelpers;
using AFT.RegoV2.Tests.Common;
using FluentAssertions;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using OnlineDepositPayNotifyRequest = AFT.RegoV2.MemberApi.Interface.Payment.OnlineDepositPayNotifyRequest;
using OnlineDepositRequest = AFT.RegoV2.MemberApi.Interface.Payment.OnlineDepositRequest;

namespace AFT.RegoV2.MemberApi.Tests.Integration.Steps
{
    [Binding]
    public class PlayerSteps : BaseSteps
    {
        private const string OnlineDepositResponseKey = "OnlineDepositResponseKey";
        private const string OnlineDepositOrderIdKey = "OnlineDepositOrderIdKey";
        private const string OnlineDepositNotifyResponseKey = "OnlineDepositNotifyResponseKey";
        private const string OnlineDepositQueryStatusResponseKey = "OnlineDepositQueryStatusResponseKey";

        private const string ResponseFromPasswordChanged = "ResponseFromPasswordChanged";
        private const string ResponseFromPersonalInfoChanged = "ResponseFromPersonalInfoChanged";
        private const string ResponseFromContactInfoChanged = "ResponseFromContactInfoChanged";
        private const string ResponseFromSelfExclusion = "ResponseFromSelfExclusion";
        private const string ResponseFromTimeOut = "ResponseFromTimeOut";
        private const string ResponseFromChangedSecurityQuestion = "ResponseFromChangedSecurityQuestion";
        private const string ResponseFromMobilePhoneVerification = "ResponseFromMobilePhoneVerification";
        private const string ResponseFromMobilePhoneVerified = "ResponseFromMobilePhoneVerified";

        private string OnlineDepositKey
        {
            get
            {
                var testKey = "testKey";
                if (ConfigurationManager.AppSettings["OnlineDepositKey"] != null)
                    testKey = ConfigurationManager.AppSettings["OnlineDepositKey"];
                return testKey;
            }
        }


        public PlayerSteps()
        {            
        }

        [Given(@"I am logged in and have access token for player")]
        public async Task GivenIAmLoggedInAndHaveAccessTokenForPlayer()
        {
            await LogInMemberApiWithNewUser();
            Token.Should().NotBeNullOrWhiteSpace();
        }

        [Then(@"Online deposit form data is visible to me")]
        public async Task ThenOnlineDepositFormDataIsVisibleToMe()
        {
            var result = await MemberApiProxy.GetOnlineDepositFormDataAsync(BrandId);

            result.Should().NotBeNull();
            result.PaymentGatewaySettings.Any().Should().BeTrue();
            var settings = result.PaymentGatewaySettings.FirstOrDefault();
            settings.PaymentGatewayName.Should().NotBeNullOrEmpty();
            settings.Channel.Should().BeGreaterOrEqualTo(0);
        }

        [When(@"I submit online deposit request")]
        public async Task WhenISubmitOnlineDepositRequest()
        {
            var result = await MemberApiProxy.OnlineDepositAsync(new OnlineDepositRequest
            {
                Amount = 200,
                BrandId = BrandId,
                CultureCode = "en-US",
                NotifyUrl = "http://membersite/notifyurl",
                ReturnUrl = "http://memebersite/returnurl"
            });

            Set(OnlineDepositResponseKey, result);
        }

        [Then(@"Online deposit request is submitted successfully")]
        public void ThenOnlineDepositRequestIsSubmittedSuccessfully()
        {
            var response = Get<OnlineDepositResponse>(OnlineDepositResponseKey);
            response.Should().NotBeNull();
            response.DepositRequestResult.RedirectUrl.Should().NotBeNull("RedirectUrl");
            response.DepositRequestResult.RedirectParams.Should().NotBeNull();
            response.DepositRequestResult.RedirectParams.MerchantId.Should().NotBeNull("MerchantId");
            response.DepositRequestResult.RedirectParams.Method.Should().NotBeNull("Method");
            response.DepositRequestResult.RedirectParams.NotifyUrl.Should().NotBeNull("NotifyUrl");
            response.DepositRequestResult.RedirectParams.ReturnUrl.Should().NotBeNull("ReturnUrl");
            response.DepositRequestResult.RedirectParams.Signature.Should().NotBeNull("Signature");
            response.DepositRequestResult.RedirectParams.Language.Should().NotBeNull("Language");
            response.DepositRequestResult.RedirectParams.Currency.Should().NotBeNull("Currency");
            response.DepositRequestResult.RedirectParams.OrderId.Should().NotBeNull("OrderId");

            Set(OnlineDepositOrderIdKey, response.DepositRequestResult.RedirectParams.OrderId);
        }

        [When(@"I pay on payment gateway")]
        public async Task WhenIPayOnPaymentGateway()
        {
            var orderId = Get<string>(OnlineDepositOrderIdKey);
            var fakeOrderId = DateTime.Now.ToString("yyyyMMddHHmmss");

            var request = new OnlineDepositPayNotifyRequest
            {
                OrderIdOfMerchant = orderId,
                OrderIdOfGateway = "OID-Gateway" + fakeOrderId,
                OrderIdOfRouter = "OID-Router" + fakeOrderId,
                PayMethod = "XPAY",
                Language = "zh-CN"
            };
            var sign = request.OrderIdOfMerchant + request.OrderIdOfRouter + request.OrderIdOfGateway + request.Language +
                       OnlineDepositKey;
            request.Signature = EncryptHelper.GetMD5HashInHexadecimalFormat(sign);
            var result = await MemberApiProxy.OnlineDepositPayNotifyAsync(request);
            Set(OnlineDepositNotifyResponseKey, result);
        }

        [Then(@"Online deposit is processed successfully")]
        public void ThenOnlineDepositIsProcessedSuccessfully()
        {
            var response = Get<string>(OnlineDepositNotifyResponseKey);
            response.Should().NotBeNull();
            response.Should().Be("SUCCESS");
        }

        [When(@"I query the online deposit")]
        public async Task WhenIQueryTheOnlineDeposit()
        {
            var orderId = Get<string>(OnlineDepositOrderIdKey);
            var request = new CheckOnlineDepositStatusRequest
            {
                TransactionNumber = orderId
            };
            var result = await MemberApiProxy.CheckOnlineDepositStatusAsync(request);
            Set(OnlineDepositQueryStatusResponseKey, result);
        }

        [Then(@"Online deposit is approved")]
        public void ThenOnlineDepositIsApproved()
        {
            var response = Get<CheckOnlineDepositStatusResponse>(OnlineDepositQueryStatusResponseKey);
            response.Should().NotBeNull();
            response.DepositStatus.IsPaid.Should().Be(true);
            response.DepositStatus.Amount.Should().Be(200);
        }

        #region Status 200 expected after GET request

        [When(@"I try to send GET request to Player Controller API (.*)")]
        public void WhenITryToSendGETRequestToPlayerControllerAPI(string p0)
        {
            //In order to get 200 we need to Mock the service layer in order to skip validation errors
            ServiceLayerMockingHelper.MockPlayerQueriesForHttpStatus200(Container);
            ServiceLayerMockingHelper.MockBrandQueriesForHttpStatus200(Container);
            ServiceLayerMockingHelper.MockWalletQueriesForHttpStatus200(Container);
            ServiceLayerMockingHelper.MockBonusQueriesForHttpStatus200(Container);
        }

        [Then(@"I should see Status Code (.*) Successful for each (.*)")]
        public void ThenIShouldSeeStatusCodeSuccessfulForEach(int p0, string p1)
        {
            Assert.DoesNotThrow(async () => await MemberApiProxy.ProfileAsync());
            Assert.DoesNotThrow(async () => await MemberApiProxy.SecurityQuestionsAsync());
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetBalancesSetAsync());
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetAcknowledgementDataAsync(Guid.NewGuid()));
            Assert.DoesNotThrow(
                async () =>
                    await
                        MemberApiProxy.GetOnSiteMessageAsync(new OnSiteMessageRequest()
                        {
                            OnSiteMessageId = Guid.NewGuid()
                        }));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetOnSiteMessagesAsync());
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetOnSiteMessagesCountAsync());

            Assert.DoesNotThrow(async () => await MemberApiProxy.GetWalletsAsync(Guid.NewGuid()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetPlayerData(TestDataGenerator.GetRandomString()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.ArePlayersIdDocumentsValid());

            //Test anonymous requests
            SetInvalidToken();
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetSecurityQuestionAsync(Guid.NewGuid()));

            Assert.DoesNotThrow(
                async () =>
                    await
                        MemberApiProxy.RegistrationFormDataAsync(new RegistrationFormDataRequest()
                        {
                            BrandId = Guid.NewGuid()
                        }));

            Assert.DoesNotThrow(() => MemberApiProxy.GetAvailableLanguages(Guid.NewGuid()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetAcknowledgementDataAsync(Guid.NewGuid()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetPlayerByResetPasswordToken(Token));
            
        }

        #endregion

        #region Status 400 expected after GET request

        [When(@"I try to send request with invalid parameters to Player Controller  API (.*)")]
        public void WhenITryToSendRequestWithInvalidParametersToPlayerControllerAPI(string p0)
        {
            //In order to be able to get validation errors 400 we need to mock some methods on service layer
            ServiceLayerMockingHelper.MockPlayerQueriesForHttpStatus400(Container);
            ServiceLayerMockingHelper.MockBrandQueriesForHttpStatus400(Container);
        }

        [Then(@"As a response I should see Status Code (.*) Bad Request")]
        public void ThenAsAResponseIShouldSeeStatusCodeBadRequest(int p0)
        {
            Assert.Throws<MemberApiValidationException>(async () => await MemberApiProxy.ProfileAsync());
            Assert.Throws<MemberApiValidationException>(
                async () => await MemberApiProxy.GetOnSiteMessageAsync(new OnSiteMessageRequest()
                {
                    OnSiteMessageId = Guid.NewGuid()
                }));
            Assert.Throws<MemberApiValidationException>(
                async () => await MemberApiProxy.GetWalletsAsync(Guid.NewGuid()));

            Assert.Throws<MemberApiValidationException>(
                async () => await MemberApiProxy.GetPlayerData(TestDataGenerator.GetRandomString()));

            //Test anonymous requests
            SetInvalidToken();

            Assert.Throws<MemberApiValidationException>(
                async () => await MemberApiProxy.GetSecurityQuestionAsync(Guid.NewGuid()));

            Assert.Throws<MemberApiValidationException>(
                async () => await MemberApiProxy.GetAcknowledgementDataAsync(Guid.NewGuid()));

            Assert.Throws<MemberApiValidationException>(
                async () => await MemberApiProxy.GetPlayerByResetPasswordToken(TestDataGenerator.GetRandomString()));
        }

        #endregion

        #region Status 401 expected after GET request
        [Given(@"User with invalid login password combination")]
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

        [When(@"I try to send GET request to Player Controller end point")]
        public void WhenITryToSendGETRequestToPlayerControllerEndPoint()
        {
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.ProfileAsync()).Exception.ErrorMessage);

            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.SecurityQuestionsAsync()).Exception.ErrorMessage);

            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetOnSiteMessagesCountAsync()).Exception.ErrorMessage);

            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetBalancesSetAsync()).Exception.ErrorMessage);

            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetWalletsAsync(Guid.NewGuid())).Exception.ErrorMessage);

            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetPlayerData(TestDataGenerator.GetRandomString())).Exception.ErrorMessage);

            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.ArePlayersIdDocumentsValid()).Exception.ErrorMessage);
        }

        [Then(@"I should see Status Code (.*) Unauthorised Request")]
        public void ThenIShouldSeeStatusCodeUnauthorisedRequest(int p0)
        {
            Assert.That(ErrorMessagesFromUnauthorizedCalls.All(el => el == HttpStatusCode.Unauthorized.ToString()));
        }

        #endregion

        #region Status 500 expected after GET request

        [When(@"I try to send GET request and logical server error occurs")]
        public void WhenITryToSendGETRequestAndLogicalServerErrorOccurs()
        {
            ServiceLayerMockingHelper.MockPlayerQueriesForHttpStatus500(Container);
            ServiceLayerMockingHelper.MockWalletQueriesForHttpStatus500(Container);
            ServiceLayerMockingHelper.MockWalletQueriesForHttpStatus500(Container);
            ServiceLayerMockingHelper.MockLoggingServiceForHttpStatus500(Container);  
            ServiceLayerMockingHelper.MockBonusApiProxyForHttpStatus500(Container);         
        }

        [Then(@"I should see Status Code (.*) indicating internal server error")]
        public void ThenIShouldSeeStatusCodeIndicatingInternalServerError(int p0)
        {
            Assert.That(
                Assert.Throws<MemberApiProxyException>(async () => await MemberApiProxy.GetOnSiteMessagesAsync())
                    .StatusCode,
                Is.EqualTo(HttpStatusCode.InternalServerError));

            Assert.That(
                Assert.Throws<MemberApiProxyException>(async () => await MemberApiProxy.GetBalancesSetAsync())
                    .StatusCode,
                Is.EqualTo(HttpStatusCode.InternalServerError));

            Assert.That(
                Assert.Throws<MemberApiProxyException>(
                    () => MemberApiProxy.GetAvailableLanguages(Guid.NewGuid())).StatusCode,
                Is.EqualTo(HttpStatusCode.InternalServerError));

            Assert.That(
                Assert.Throws<MemberApiProxyException>(
                    async () => await MemberApiProxy.RegistrationFormDataAsync(new RegistrationFormDataRequest()
                    {
                        BrandId = Guid.NewGuid()
                    })).StatusCode,
                Is.EqualTo(HttpStatusCode.InternalServerError));

            Assert.That(
                Assert.Throws<MemberApiProxyException>(
                    async () => await MemberApiProxy.ArePlayersIdDocumentsValid()).StatusCode,
                Is.EqualTo(HttpStatusCode.InternalServerError));

            ServiceLayerMockingHelper.VerifyLogginServiceWasCalledAfterInternalServerError(5);
        }

        #endregion

        #region Status 201 expected after POST request

        [When(@"I try to send POST request to Player controller end point")]
        public void WhenITryToSendPOSTRequestToPlayerControllerEndPoint()
        {
            ServiceLayerMockingHelper.MockPlayerCommandsForHttpStatus201(Container);

            Assert.DoesNotThrow(async () =>
                ScenarioContext.Current[ResponseFromPasswordChanged] =
                    await MemberApiProxy.ChangePasswordAsync(new ChangePasswordRequest()
                    {
                        OldPassword = LoggedInPlayerMetadata.Password,
                        NewPassword = TestDataGenerator.GetRandomString(),
                        Username = LoggedInPlayerMetadata.Username
                    }));

            Assert.DoesNotThrow(async () =>
                ScenarioContext.Current[ResponseFromPersonalInfoChanged] =
                    await MemberApiProxy.ChangePersonalInfoAsync(new ChangePersonalInfoRequest()
                    {
                        PlayerId = PlayerId,
                        FirstName = TestDataGenerator.GetRandomString(),
                        LastName = TestDataGenerator.GetRandomString(),
                        Email = TestDataGenerator.GetRandomEmail(),
                        CurrencyCode = "CAD",
                        DateOfBirth = DateTime.UtcNow.AddYears(-30).ToString("yyyy/MM/dd"),
                        Gender = "Male",
                        Title = "Mr",
                        CountryCode = TestDataGenerator.GetRandomCountryCode(),
                        MailingAddressCity = TestDataGenerator.GetRandomString(),
                        MailingAddressLine1 = TestDataGenerator.GetRandomString(),
                        MailingAddressPostalCode = TestDataGenerator.GetRandomNumber(1000).ToString(),
                        PhoneNumber = TestDataGenerator.GetRandomPhoneNumber(),
                        AccountAlertEmail = false,
                        AccountAlertSms = true,
                        MarketingAlertEmail = true,
                        MarketingAlertPhone = false,
                        MarketingAlertSms = false
                    }));

            Assert.DoesNotThrow(async () =>
                ScenarioContext.Current[ResponseFromContactInfoChanged] =
                    await MemberApiProxy.ChangeContactInfoAsync(new ChangeContactInfoRequest()
                    {
                        PlayerId = PlayerId,
                        MailingAddressCity = TestDataGenerator.GetRandomString(),
                        PhoneNumber = TestDataGenerator.GetRandomPhoneNumber(false),
                        CountryCode = LoggedInPlayerMetadata.Country,
                        MailingAddressLine1 = TestDataGenerator.GetRandomString(),
                        MailingAddressPostalCode = TestDataGenerator.GetRandomNumber(1000).ToString()
                    }));

            Assert.DoesNotThrow(async () =>
                ScenarioContext.Current[ResponseFromSelfExclusion] =
                    await MemberApiProxy.SelfExclude(new SelfExclusionRequest()
                    {
                        Option = (int)PlayerEnums.SelfExclusion._1Year,
                        PlayerId = PlayerId
                    }));

            Assert.DoesNotThrow(async () =>
                ScenarioContext.Current[ResponseFromTimeOut] =
                await MemberApiProxy.TimeOut(new TimeOutRequest()
                {
                    PlayerId = PlayerId,
                    Option = (int)PlayerEnums.TimeOut.Month
                }));

            Assert.DoesNotThrow(async () =>
                ScenarioContext.Current[ResponseFromChangedSecurityQuestion] =
                await MemberApiProxy.ChangeSecurityQuestionAsync(new ChangeSecurityQuestionRequest()
                {
                    Id = PlayerId.ToString(),
                    SecurityAnswer = TestDataGenerator.GetRandomString(),
                    SecurityQuestionId = LoggedInPlayerMetadata.SecurityQuestion
                }));

            Assert.DoesNotThrow(async () =>
                ScenarioContext.Current[ResponseFromMobilePhoneVerification] =
                await MemberApiProxy.VerificationCodeAsync(new VerificationCodeRequest()));

            Assert.DoesNotThrow(async () =>
                ScenarioContext.Current[ResponseFromMobilePhoneVerified] =
                await MemberApiProxy.VerifyMobileAsync(new VerifyMobileRequest()));
        }

        [Then(@"I should see status code (.*) and uri which points to the updated entity")]
        public void ThenIShouldSeeStatusCodeAndUriWhichPointsToTheUpdatedEntity(int p0)
        {
            //From this point on we check if the returned by the POST uri is not empty
            ((ChangePasswordResponse)ScenarioContext.Current[ResponseFromPasswordChanged])
                .UriToUserWithUpdatedPassword
                .Should().NotBeNullOrEmpty();

            ((ChangePersonalInfoResponse)ScenarioContext.Current[ResponseFromPersonalInfoChanged])
                .UriToUserWithPersonalInfoUpdated
                .Should().NotBeNullOrEmpty();

            ((ChangeContactInfoResponse)ScenarioContext.Current[ResponseFromContactInfoChanged])
                .UriToProfileWithUpdatedContactInfo
                .Should().NotBeNullOrEmpty();

            ((SelfExclusionResponse)ScenarioContext.Current[ResponseFromSelfExclusion])
                .UriToPlayerThatSelfExclusionWasAppliedTo
                .Should().NotBeNullOrEmpty();

            ((TimeOutResponse)ScenarioContext.Current[ResponseFromTimeOut])
                .UriToPlayerWhoWasTimeOuted
                .Should().NotBeNullOrEmpty();

            ((ChangeSecurityQuestionResponse)ScenarioContext.Current[ResponseFromChangedSecurityQuestion])
                .UriToPlayerWhoseSecurityQuestionWasChanged
                .Should().NotBeNullOrEmpty();

            ((VerificationCodeResponse)ScenarioContext.Current[ResponseFromMobilePhoneVerification])
                .UriToPlayerWhomMobileVerificationCodeWasSent
                .Should().NotBeNullOrEmpty();

            ((VerifyMobileResponse)ScenarioContext.Current[ResponseFromMobilePhoneVerified])
                .UriToPlayerWhoseMobileWasVerified
                .Should().NotBeNullOrEmpty();

            //From this point on we check if the returned by the POST uri is accurate and working
            using (var client = new HttpClient())
            {
                AssignDefaultHeaderValues(client);

                CheckUpdatedEntityAvailability(client,
                    ((ChangePasswordResponse)ScenarioContext.Current[ResponseFromPasswordChanged])
                    .UriToUserWithUpdatedPassword.Split('?')[0],
                    ((ChangePasswordResponse)ScenarioContext.Current[ResponseFromPasswordChanged])
                    .UriToUserWithUpdatedPassword.Split('?')[1]);

                CheckUpdatedEntityAvailability(client,
                    ((ChangePersonalInfoResponse)ScenarioContext.Current[ResponseFromPersonalInfoChanged])
                    .UriToUserWithPersonalInfoUpdated.Split('?')[0],
                    ((ChangePersonalInfoResponse)ScenarioContext.Current[ResponseFromPersonalInfoChanged])
                    .UriToUserWithPersonalInfoUpdated.Split('?')[1]
                    );

                CheckUpdatedEntityAvailability(client,
                    ((ChangeContactInfoResponse)ScenarioContext.Current[ResponseFromContactInfoChanged])
                    .UriToProfileWithUpdatedContactInfo.Split('?')[0],
                    ((ChangeContactInfoResponse)ScenarioContext.Current[ResponseFromContactInfoChanged])
                    .UriToProfileWithUpdatedContactInfo.Split('?')[1]
                    );

                CheckUpdatedEntityAvailability(client,
                    (((SelfExclusionResponse)ScenarioContext.Current[ResponseFromSelfExclusion])
                    .UriToPlayerThatSelfExclusionWasAppliedTo.Split('?'))[0],
                    (((SelfExclusionResponse)ScenarioContext.Current[ResponseFromSelfExclusion])
                    .UriToPlayerThatSelfExclusionWasAppliedTo.Split('?')[1]
                    ));

                CheckUpdatedEntityAvailability(client,
                    (((TimeOutResponse)ScenarioContext.Current[ResponseFromTimeOut])
                    .UriToPlayerWhoWasTimeOuted.Split('?'))[0],
                    ((TimeOutResponse)ScenarioContext.Current[ResponseFromTimeOut])
                    .UriToPlayerWhoWasTimeOuted.Split('?')[1]
                    );

                CheckUpdatedEntityAvailability(client,
                    (((ChangeSecurityQuestionResponse)ScenarioContext.Current[ResponseFromChangedSecurityQuestion])
                    .UriToPlayerWhoseSecurityQuestionWasChanged.Split('?'))[0],
                    ((ChangeSecurityQuestionResponse)ScenarioContext.Current[ResponseFromChangedSecurityQuestion])
                    .UriToPlayerWhoseSecurityQuestionWasChanged.Split('?')[1]
                    );

                CheckUpdatedEntityAvailability(client,
                    (((VerificationCodeResponse)ScenarioContext.Current[ResponseFromMobilePhoneVerification])
                    .UriToPlayerWhomMobileVerificationCodeWasSent.Split('?'))[0],
                    ((VerificationCodeResponse)ScenarioContext.Current[ResponseFromMobilePhoneVerification])
                    .UriToPlayerWhomMobileVerificationCodeWasSent.Split('?')[1]
                    );

                CheckUpdatedEntityAvailability(client,
                    (((VerifyMobileResponse)ScenarioContext.Current[ResponseFromMobilePhoneVerified])
                    .UriToPlayerWhoseMobileWasVerified.Split('?'))[0],
                    ((VerifyMobileResponse)ScenarioContext.Current[ResponseFromMobilePhoneVerified])
                    .UriToPlayerWhoseMobileWasVerified.Split('?')[1]
                    );
            }

            #endregion
        }
    }
}