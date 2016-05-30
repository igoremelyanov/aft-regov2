using System.Collections.Generic;
using System.Net.Http;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Proxy;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Shared.OAuth2;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.AdminApi.Tests.Integration.Steps
{
    [Binding]
    public class BaseSteps
    {
        protected AdminApiProxy AdminApiProxy
        {
            get
            {
                return ScenarioContext.Current.ContainsKey("AdminApiProxy") ? ScenarioContext.Current.Get<AdminApiProxy>("AdminApiProxy") : null;
            }
            set
            {
                ScenarioContext.Current.Set(value, "AdminApiProxy");
            }
        }

        protected string AdminApiUrl { get; set; }
        protected string Token { get; set; }
        protected IUnityContainer Container { get; set; }

        public BaseSteps()
        {
            Container = ScenarioContext.Current.Get<IUnityContainer>();
            AdminApiUrl = (string)ScenarioContext.Current["AdminApiUrl"];
        }

        protected void LogInAdminApi(string username, string password)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            var response = new HttpClient().PostAsync(AdminApiUrl + "token", formContent).Result;
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(response.Content.ReadAsStringAsync().Result);
            Token = tokenResponse.AccessToken;

            AdminApiProxy = new AdminApiProxy(AdminApiUrl, Token);
        }

        protected void SetInvalidToken()
        {
            Token = TestDataGenerator.GetRandomString(300);

            AdminApiProxy = new AdminApiProxy(AdminApiUrl, Token);
        }

        protected Admin CreateUserWithPermissions(string category, string[] permissions, string password)
        {
            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);
            var brands = new[] { brand };

            return securityTestHelper.CreateAdmin(category, permissions, brands, password);
        }

        protected void LogWithNewUser(string category, string permission)
        {
            var password = TestDataGenerator.GetRandomString(8);
            var user = CreateUserWithPermissions(category, new[] { permission }, password);
            LogInAdminApi(user.Username, password);
        }
    }
}