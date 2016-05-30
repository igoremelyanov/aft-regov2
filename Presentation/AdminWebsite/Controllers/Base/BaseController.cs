using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AFT.RegoV2.AdminApi.Interface.Proxy;
using AFT.RegoV2.AdminWebsite.Common.ErrorHandling;
using AFT.RegoV2.AdminWebsite.Common.IpFiltering;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Common.Interfaces;

using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [AuthorizeIpAddress]
    [SessionExpire]
    public class BaseController : Controller
    {
        public ActorInfo CurrentUser
        {
            get
            {
                var actorInfo = DependencyResolver.Current.GetService<IActorInfoProvider>();
                return actorInfo.Actor;
            }
        }

        public string Token
        {
            get
            {
                var cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (cookie != null)
                {
                    var formsTicket = FormsAuthentication.Decrypt(cookie.Value);
                    var user = JsonConvert.DeserializeObject<AuthUser>(formsTicket.UserData);
                    return user.Token;
                }
                return string.Empty;
            }
        }

        public void LogoutUser()
        {
            FormsAuthentication.SignOut();
        }

        protected string SerializeJson(object response)
        {
            Response.ContentType = "application/json";
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return JsonConvert.SerializeObject(response, jsonSettings);
        }

        protected static string ToCamelCase(string str)
        {
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public ActionResult ValidationErrorResponseActionResult(ValidationError e)
        {
            var fields = e.Violations
                .GroupBy(x => x.FieldName)
                .Select(x => new
                {
                    Name = ToCamelCase(x.Key),
                    Errors = x.Select(y => y.ErrorMessage).ToArray()
                }).ToArray();

            var jsonString = SerializeJson(new { Result = "failed", Fields = fields });

            return Content(jsonString, "application/json");
        }

        public ActionResult ValidationErrorResponseActionResult(IList<ValidationFailure> e)
        {
            var fields = e
                .GroupBy(x => x.PropertyName)
                .Select(x => new
                {
                    Name = ToCamelCase(x.Key),
                    Errors = x.Select(y => y.ErrorMessage).ToArray()
                }).ToArray();

            var jsonString = SerializeJson(new { Result = "failed", Fields = fields });

            return Content(jsonString, "application/json");
        }

        public JsonResult ValidationErrorResponse(ValidationResult validationResult)
        {
            return Json(new { Success = false, Errors = validationResult.Errors.Select(er => new { er.PropertyName, er.ErrorMessage }) });
        }

        public string GetAdminApiUrl()
        {
            var settingsProvider = DependencyResolver.Current.GetService<ICommonSettingsProvider>();
            return settingsProvider.GetAdminApiUrl();
        }

        public AdminApiProxy GetAdminApiProxy(HttpRequestBase request)
        {
            return new AdminApiProxy(GetAdminApiUrl(), Token);
        }
    }
}