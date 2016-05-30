using AFT.RegoV2.Tests.Selenium.MemberWebSite.Pages;
using AFT.RegoV2.Tests.Selenium.MemberWebSite.HelpersAPI;

namespace AFT.RegoV2.Tests.Selenium.MemberWebSite.Tests
{
    class Register : BaseTest
    {
        public void RegisterNewUser()
        {
            RegistrationData registrationData = MemberSiteUserRegistrationHelper.RegisterUserAPI();
            HomePage homePage = new HomePage(_driver);
            
            //homePage.GoToRegistrationPage();
            //RegistrationPage registrationPage = new RegistrationPage(_driver);
            //registrationPage.EnterRegistrationData();
            //Console.WriteLine("This is test");
            homePage.GoToLoginForm();
            LoginPage loginPage = new LoginPage(_driver);
            loginPage.SubmitLogin(registrationData.Username, registrationData.Password);
            MyAccountPage myAccountPage = new MyAccountPage(_driver);

            myAccountPage.NavigateToResponsibleGamblingPage();
            ResponsibleGamblingPage responsibleGemblingPage = new ResponsibleGamblingPage(_driver);
            responsibleGemblingPage.SetTimeOut();
            ConfirmationPopUpForm confirmationPopUpForm = new ConfirmationPopUpForm(_driver);
            confirmationPopUpForm.ApproveTimeOut();
            HomePage homePage2 = new HomePage(_driver);
            homePage2.GoToLoginForm();
            loginPage.SubmitLogin(registrationData.Username, registrationData.Password);
        }
    }
}
