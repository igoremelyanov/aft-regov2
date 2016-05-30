using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AFT.RegoV2.MemberApi.Interface;
using AFT.RegoV2.MemberApi.Interface.Exceptions;
using AFT.RegoV2.MemberApi.Interface.Security;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using SecurityTestHelper = AFT.RegoV2.Tests.Common.Helpers.SecurityTestHelper;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.MemberApi.Tests.Integration.Steps
{
    [Binding]
    public class SecuritySteps : BaseSteps
    {
        [Given(@"Anonymous request allowed, valid IP address and brandName")]
        public void GivenAnonymousRequestAllowedValidIpAddressAndBrandName()
        {
            Set(SecurityContextFields.IpAddress, SecurityStepsHelper.GetValidIpAddress());
            Set(SecurityContextFields.BrandName, SecurityStepsHelper.ValidBrandName);
        }

        [Given(@"Anonymous request allowed, valid IP address and invalid brand")]
        public void GivenAnonymousRequestAllowedValidIpAddressAndInvalidBrand()
        {
            Set(SecurityContextFields.IpAddress, SecurityStepsHelper.GetValidIpAddress());
            Set(SecurityContextFields.BrandName, SecurityStepsHelper.InvalidBrandName);
        }

        [Given(@"Anonymous request allowed, invalid IP address and valid brand")]
        public void GivenAnonymousRequestAllowedInalidIpAddressAndValidBrand()
        {
            Set(SecurityContextFields.IpAddress, SecurityStepsHelper.GetInvalidIpAddress());
            Set(SecurityContextFields.BrandName, SecurityStepsHelper.ValidBrandName);
        }

        [Given(@"Anonymous request allowed, invalid IP address and invalid brand")]
        public void GivenAnonymousRequestAllowedInalidIpAddressAndInvalidBrand()
        {
            Set(SecurityContextFields.IpAddress, SecurityStepsHelper.GetInvalidIpAddress());
            Set(SecurityContextFields.BrandName, SecurityStepsHelper.InvalidBrandName);
        }

        [Given(@"Anonymous request allowed, empty value for IP address and empty value for brand name")]
        public void GivenAnonymousRequestAllowedEmptyValueForIpAddressAndEmptyValueForBrandName()
        {
            Set(SecurityContextFields.IpAddress, string.Empty);
            Set(SecurityContextFields.BrandName, string.Empty);
        }

        [Given(@"Anonymous request allowed, “localhost” value for IP address and valid brand")]
        public void GivenAnonymousRequestAllowedLocalhostValueForIpAddressAndValidBrand()
        {
            Set(SecurityContextFields.IpAddress, SecurityStepsHelper.GetLocalhostIpAddress());
            Set(SecurityContextFields.BrandName, SecurityStepsHelper.ValidBrandName);
        }

        [Given(@"Anonymous request allowed, ""(.*)"" as redirect URL, valid IP address and valid brand")]
        public void GivenAnonymousRequestAllowedRedirectUrlInvalidIpAddressAndValidBrand(string redirectUrl)
        {
            var ipAddress = SecurityStepsHelper.GetValidIpAddress();
            const string brandName = SecurityStepsHelper.ValidBrandName;

            SecurityHelper.CreateBrandIpRegulation(ipAddress, brandName, redirectUrl);

            Set(SecurityContextFields.IpAddress, ipAddress);
            Set(SecurityContextFields.BrandName, brandName);
        }

        [Given(@"Anonymous request allowed, ""(.*)"" as blocking type, valid IP address and valid brand")]
        public void GivenAnonymousRequestAllowedBlockingTypeInvalidIpAddressAndValidBrand(string blockingType)
        {
            var ipAddress = SecurityStepsHelper.GetValidIpAddress();
            const string brandName = SecurityStepsHelper.ValidBrandName;

            SecurityHelper.CreateBrandIpRegulation(ipAddress, brandName, blockingType: blockingType);

            Set(SecurityContextFields.IpAddress, ipAddress);
            Set(SecurityContextFields.BrandName, SecurityStepsHelper.ValidBrandName);
        }

        [When(@"I try to send request to VerifyIp end point and get Response")]
        public void WhenITryToSendRequestToEndPointAndGetResponse()
        {
            var ipAddress = Get<string>(SecurityContextFields.IpAddress);
            var brandName = Get<string>(SecurityContextFields.BrandName);

            var request = new VerifyIpRequest
            {
                IpAddress = ipAddress,
                BrandName = brandName
            };

            try
            {
                var result = MemberApiProxy.VerifyIp(request);

                result.Should().NotBeNull();

                Set(SecurityContextFields.ErrorCode, HttpStatusCode.OK);
                Set(SecurityContextFields.Result, result);
            }
            catch (MemberApiValidationException e)
            {
                Set(SecurityContextFields.ErrorCode, e.StatusCode);

                var regoValidationError = e.ValidationErrors.FirstOrDefault();
                if (regoValidationError != null)
                    Set(SecurityContextFields.ErrorMessage, regoValidationError.ErrorMessage);
            }
        }

        [Then(@"I should see Status Code (.*)")]
        public void ThenIShouldSeeStatusCode(int status)
        {
            var statusCode = Get<HttpStatusCode>(SecurityContextFields.ErrorCode);
            statusCode.Should().Be((HttpStatusCode)status);
        }

        [Then(@"I should see successfull VerifyIp response")]
        public void ThenIShouldSeeSuccessfullVerifyIpResponse()
        {
            var result = Get<VerifyIpResponse>(SecurityContextFields.Result);
            result.Should().NotBeNull();
            result.Allowed.Should().BeTrue();
        }

        [Then(@"I should see localisation code ""(.*)"" for error message")]
        public void ThenIShouldSeeLocalisationCodeForErrorMessage(string code)
        {
            var errorMessage = Get<string>(SecurityContextFields.ErrorMessage);

            errorMessage.Should().NotBeNullOrEmpty();
            errorMessage.Should().Be(code);
        }

        [Then(@"I should see redirect URL ""(.*)""")]
        public void ThenIShouldSeeRedirectUrl(string redirectUrl)
        {
            var result = Get<VerifyIpResponse>(SecurityContextFields.Result);
            result.Should().NotBeNull();
            result.RedirectionUrl.Should().Be(redirectUrl);
        }

        [Then(@"I should see blocking type ""(.*)""")]
        public void ThenIShouldSeeBlockingType(string blockingType)
        {
            var result = Get<VerifyIpResponse>(SecurityContextFields.Result);
            result.Should().NotBeNull();
            result.BlockingType.Should().Be(blockingType);
        }
    }

    public static class SecurityStepsHelper
    {
        public const string ValidBrandName = "138";
        public const string InvalidBrandName = "invalid brand";

        public static string GetValidIpAddress()
        {
            return TestDataGenerator.GetRandomIpAddress();
        }

        public static string GetInvalidIpAddress()
        {
            return TestDataGenerator.GetRandomIpAddress() + TestDataGenerator.GetRandomString(5);
        }

        public static string GetLocalhostIpAddress()
        {
            return IPAddress.Loopback.ToString();
        }
    }

    public class SecurityContextFields
    {
        public const string IpAddress = "ipAddress";
        public const string BrandName = "brandName";
        public const string ErrorCode = "errorCode";
        public const string ErrorMessage = "errorMessage";
        public const string Result = "result";
        public const string RedirectUrl = "redirectUrl";
        public const string BlockingType = "blockingType";
    }
}
