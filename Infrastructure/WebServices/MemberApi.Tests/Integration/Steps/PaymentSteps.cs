using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using AFT.RegoV2.MemberApi.Interface.Payment;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.MemberApi.Tests.Integration.MockHelpers;
using AFT.RegoV2.Tests.Common;
using FluentAssertions;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.MemberApi.Tests.Integration.Steps
{
    [Binding]
    public class PaymentSteps : BaseSteps
    {
        [Given(@"User with valid credentials is logged in")]
        public async Task GivenUserWithValidCredentialsIsLoggedIn()
        {
            await LogInMemberApiWithNewUser();
            Token.Should().NotBeNullOrWhiteSpace();
        }

        #region Status 200 expected
        [When(@"I try to send GET request to Payment  Controller API (.*)")]
        public void WhenITryToSendGETRequestToPaymentControllerAPI(string p0)
        {
            //In order to get 200 we need to Mock the service layer in order to skip validation errors
            ServiceLayerMockingHelper.MockPaymentQueriesForHttpStatus200(Container);
        }
        
        [Then(@"The received Status Code must be (.*) Successful and Response schema validated")]
        public void ThenTheReceivedStatusCodeMustBeSuccessfulAndResponseSchemaValidated(int p0)
        {
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetOfflineDepositFormDataAsync(Guid.NewGuid()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.PlayerLastDepositSummaryResponse());
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetOnlinePaymentSetting(Guid.NewGuid(), String.Empty));
            Assert.DoesNotThrow(async () => await MemberApiProxy.IsDepositorsFullNameValid(TestDataGenerator.GetRandomString()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetBankAccountsForOfflineDeposit());
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetPendingDeposits());
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetDeposits(1, null, null, null));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetOfflineDeposit(Guid.NewGuid()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetWithdrawalFormDataAsync(Guid.NewGuid()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetBanks());
            Assert.DoesNotThrow(async () => await MemberApiProxy.ValidatePlayerBankAccount(new PlayerBankAccountRequest()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetFundTransferFormDataAsync(Guid.NewGuid()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.ValidateOnlineDepositAmount(new ValidateOnlineDepositAmount()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetBankAccountForOfflineDeposit(Guid.NewGuid()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.CheckOnlineDepositStatusAsync(new CheckOnlineDepositStatusRequest()));
            Assert.DoesNotThrow(async () => await MemberApiProxy.GetOnlineDepositFormDataAsync(Guid.NewGuid()));
        }
        #endregion

        #region Status 401 expected
        [Given(@"User with invalid credentials is logged in")]
        public async Task GivenUserWithInvalidCredentialsIsLoggedIn()
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


        [When(@"I try to send GET request without having a valid Token to Payment Controller end point")]
        public void WhenITryToSendGETRequestWithoutHavingAValidTokenToPaymentControllerEndPoint()
        {
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetOfflineDepositFormDataAsync(Guid.NewGuid())).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.PlayerLastDepositSummaryResponse()).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetOnlinePaymentSetting(Guid.NewGuid(), String.Empty)).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.IsDepositorsFullNameValid(TestDataGenerator.GetRandomString())).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetBankAccountsForOfflineDeposit()).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetPendingDeposits()).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetDeposits(1, null, null, null)).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetOfflineDeposit(Guid.NewGuid())).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetWithdrawalFormDataAsync(Guid.NewGuid())).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetBanks()).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.ValidatePlayerBankAccount(new PlayerBankAccountRequest())).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetFundTransferFormDataAsync(Guid.NewGuid())).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.ValidateOnlineDepositAmount(new ValidateOnlineDepositAmount())).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetBankAccountForOfflineDeposit(Guid.NewGuid())).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.CheckOnlineDepositStatusAsync(new CheckOnlineDepositStatusRequest())).Exception.ErrorMessage);
            ErrorMessagesFromUnauthorizedCalls.Add(Assert.Throws<MemberApiProxyException>(
                async () => await MemberApiProxy.GetOnlineDepositFormDataAsync(Guid.NewGuid())).Exception.ErrorMessage);

            //TODO: Add test cases for POST methods
        }

        [Then(@"I should see unautorized response as Status Code (.*)")]
        public void ThenIShouldSeeUnautorizedResponseAsStatusCode(int p0)
        {
            Assert.That(ErrorMessagesFromUnauthorizedCalls.All(el => el == HttpStatusCode.Unauthorized.ToString()));
        }

        #endregion
    }
}
