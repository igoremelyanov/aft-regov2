using System;
using System.Net;
using System.Web;
using AFT.RegoV2.AdminApi.Interface.Bonus;
using AFT.RegoV2.AdminApi.Interface.Common;
using AFT.RegoV2.AdminApi.Interface.Proxy;
using AFT.RegoV2.Bonus.Api.Interface.Responses;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Core.Security.Common;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.AdminApi.Tests.Integration.Steps
{
    public class BonusSteps : BaseSteps
    {
        [Then(@"I am forbidden to execute permission protected bonus methods with insufficient permissions")]
        public void ThenIAmForbiddenToExecutePermissionProtectedBonusMethodsWithInsufficientPermissions()
        {
            LogWithNewUser(Modules.VipLevelManager, Permissions.View);

            const int statusCode = (int)HttpStatusCode.Forbidden;

            Assert.AreEqual(statusCode, Assert.Throws<HttpException>(() => AdminApiProxy.ChangeBonusStatus(new ToggleBonusStatus())).GetHttpCode());
            Assert.AreEqual(statusCode, Assert.Throws<HttpException>(() => AdminApiProxy.CreateUpdateBonus(new CreateUpdateBonus())).GetHttpCode());
            Assert.AreEqual(statusCode, Assert.Throws<HttpException>(() => AdminApiProxy.GetBonusRelatedData()).GetHttpCode());
            Assert.AreEqual(statusCode, Assert.Throws<HttpException>(() => AdminApiProxy.DeleteBonusTemplate(new DeleteTemplate())).GetHttpCode());
            Assert.AreEqual(statusCode, Assert.Throws<HttpException>(() => AdminApiProxy.CreateUpdateBonusTemplate(new CreateUpdateTemplate())).GetHttpCode());
            Assert.AreEqual(statusCode, Assert.Throws<HttpException>(() => AdminApiProxy.GetBonusTemplateRelatedData()).GetHttpCode());
        }

        [Then(@"I am unauthorized to execute bonus methods with invalid token")]
        public void ThenIAmUnauthorizedToExecuteBonusMethodsWithInvalidToken()
        {
            Assert.AreEqual(HttpStatusCode.Unauthorized, Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.ChangeBonusStatus(new ToggleBonusStatus())).StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CreateUpdateBonus(new CreateUpdateBonus())).StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetBonusRelatedData()).StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.DeleteBonusTemplate(new DeleteTemplate())).StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CreateUpdateBonusTemplate(new CreateUpdateTemplate())).StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.GetBonusTemplateRelatedData()).StatusCode);
        }

        [Then(@"I am not allowed to execute bonus methods using GET")]
        public void ThenIAmNotAllowedToExecuteBonusMethodsUsingGET()
        {
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<ToggleBonusStatusResponse>(AdminApiRoutes.ChangeBonusStatus, string.Empty)).StatusCode);
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<AddEditBonusResponse>(AdminApiRoutes.CreateUpdateBonus, string.Empty)).StatusCode);
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<DeleteTemplateResponse>(AdminApiRoutes.DeleteBonusTemplate, string.Empty)).StatusCode);
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonGet<AddEditTemplateResponse>(AdminApiRoutes.CreateEditBonusTemplate, string.Empty)).StatusCode);
        }

        [Then(@"I am not allowed to execute bonus methods using POST")]
        public void ThenIAmNotAllowedToExecuteBonusMethodsUsingPOST()
        {
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonPost<object, BonusDataResponse>(AdminApiRoutes.GetBonusRelatedData + "?id=", new Guid())).StatusCode);
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, Assert.Throws<AdminApiProxyException>(() => AdminApiProxy.CommonPost<object, TemplateDataResponse>(AdminApiRoutes.GetBonusTemplateRelatedData + "?id=", new Guid())).StatusCode);
        }
    }
}
