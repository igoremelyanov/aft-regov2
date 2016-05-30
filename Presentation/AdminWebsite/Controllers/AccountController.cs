using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AFT.RegoV2.AdminApi.Interface;
using AFT.RegoV2.AdminApi.Interface.Proxy;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using Newtonsoft.Json;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IEventBus _eventBus;
        private readonly IAdminQueries _adminQueries;

        public AccountController(IEventBus eventBus, IAdminQueries adminQueries)
        {
            _eventBus = eventBus;
            _adminQueries = adminQueries;
        }

        [HttpGet]
        public ActionResult Login()
        {
            ClearCookies();

            return View(new LoginViewModel());
        }

        private void ClearCookies()
        {
            foreach (var c in Request.Cookies.AllKeys.Select(
                cookieName => new HttpCookie(cookieName)
                {
                    Expires = DateTime.Now.AddDays(-1)
                }))
            {
                Response.Cookies.Add(c);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            var validationResult = _adminQueries.GetValidationResult(new LoginAdmin
            {
                Username = model.Username,
                Password = model.Password
            });

            var ipAddress = Request.ServerVariables["REMOTE_ADDR"];
            var headers = Request.Headers.AllKeys.ToDictionary(key => key, key => Request.Headers.Get(key));

            if (validationResult.IsValid)
            {
                await LoginUser(model.Username, model.Password, model.RememberMe);

                _eventBus.Publish(new AdminAuthenticationSucceded
                {
                    IPAddress = ipAddress,
                    Headers = headers,
                    EventCreatedBy = model.Username
                });
                var decodedUrl = "";
                if (!string.IsNullOrEmpty(returnUrl))
                    decodedUrl = Server.UrlDecode(returnUrl);

                if (Url.IsLocalUrl(decodedUrl))
                {
                    return Json(new
                    {
                        Success = true,
                        RedirectTo = decodedUrl
                    });
                }

                return Json(new
                {
                    Success = true,
                    RedirectTo = Url.Action("Index", "Home")
                });
            }

            _eventBus.Publish(new AdminAuthenticationFailed
            {
                Username = model.Username,
                IPAddress = ipAddress,
                Headers = headers,
                FailReason = validationResult.Errors.First().ErrorMessage
            });

            LogoutUser();

            return Json(new
            {
                Success = false,
                Errors = validationResult.Errors.Select(vr => vr.ErrorMessage)
            });
        }

        public async Task LoginUser(string username, string password, bool rememberMe)
        {
            var admin = _adminQueries.GetAdminByName(username);

            var result = await GetAdminApiProxy(Request).Login(new LoginRequest
            {
                Username = username,
                Password = password
            });

            var authUser = new AuthUser
            {
                UserId = admin.Id,
                UserName = admin.Username,
                Token = result.AccessToken,
                RefreshToken = result.RefreshToken
            };

            SetAuthCookie(username, rememberMe, authUser);
        }

        private void SetAuthCookie(string username, bool remeberMe, AuthUser user)
        {
            var cookie = FormsAuthentication.GetAuthCookie(username, remeberMe);
            var ticket = FormsAuthentication.Decrypt(cookie.Value);
            var newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate,
                ticket.Expiration,
                ticket.IsPersistent, JsonConvert.SerializeObject(user), ticket.CookiePath);

            string encryptedTicket = FormsAuthentication.Encrypt(newTicket);
            cookie.Value = encryptedTicket;
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
        }

        [HttpGet]
        public ActionResult Logout()
        {
            LogoutUser();
            return Json(Url.Action("Login"), JsonRequestBehavior.AllowGet);
        }
    }
}