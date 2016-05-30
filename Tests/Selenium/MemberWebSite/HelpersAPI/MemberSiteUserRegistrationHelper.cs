using System;
using RestSharp;
using NUnit.Framework;
using Newtonsoft.Json;
using AFT.RegoV2.Tests.Common;
using System.Configuration;


namespace AFT.RegoV2.Tests.Selenium.MemberWebSite.HelpersAPI
{
    [Ignore(" Igor, 25-Aiprl-2016")]
    class MemberSiteUserRegistrationHelper
    {
        public static RegistrationData RegisterUserAPI()
        {
            String host = ConfigurationManager.AppSettings["MemberWebsiteUrl"];
            String registerDataAPI = RegistrationDataAPI();
            var client = new RestClient();
            
            client.BaseUrl = new Uri(host+"api/Register");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", registerDataAPI, ParameterType.RequestBody);            
            IRestResponse response = client.Execute(request);
            Assert.IsTrue(response.Content.Contains("success"));
            return JsonConvert.DeserializeObject<RegistrationData>(registerDataAPI);
        }

        public static String RegistrationDataAPI()
        {
            RegistrationData registrationData = new RegistrationData();

            registrationData.FirstName = "AutoAPI"+TestDataGenerator.GetRandomAlphabeticString(4);
            registrationData.LastName = "AutoAPI"+TestDataGenerator.GetRandomAlphabeticString(4);
            registrationData.Username = "AutoAPI"+TestDataGenerator.GetRandomAlphabeticString(4);
            registrationData.Password = "123456";
            registrationData.PasswordConfirm = "123456";
            registrationData.AccountStatus = "Active";
            registrationData.IdStatus = "Verified";
            registrationData.DateOfBirth = "1995/01/01";
            registrationData.Email = "AutoAPI"+TestDataGenerator.GetRandomAlphabeticString(4)+"@mail.com";
            registrationData.PhoneNumber = "0987654321";
            registrationData.Address = "test";
            registrationData.ZipCode = "12345";
            registrationData.CountryCode = "CA";
            registrationData.CurrencyCode = "CAD";
            registrationData.CultureCode = "en-US";
            registrationData.Brand = "00000000-0000-0000-0000-000000000138";
            registrationData.Gender = "Male";
            registrationData.Title = "Mr";
            registrationData.Comments = "test";
            registrationData.MailingAddressLine1 = "test";
            registrationData.PhysicalAddressLine1 = "test";
            registrationData.AddressLine4 = "test";
            registrationData.MailingAddressCity = "test";
            registrationData.PhysicalAddressCity = "test";
            registrationData.ContactPreference = "Email";
            registrationData.MailingAddressPostalCode = "12345";
            registrationData.PhysicalAddressPostalCode = "12345";
            registrationData.ReferralId = "";
            registrationData.SecurityQuestionId = "a59635c7-523d-4c74-b456-483eeb458b6d";
            registrationData.SecurityAnswer = "test";
            registrationData.BrandId = "00000000-0000-0000-0000-000000000138";

            return JsonConvert.SerializeObject(registrationData);        
        }
    }
}
