using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Shared.Constants;
using Newtonsoft.Json;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var cookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                var user = JsonConvert.DeserializeObject<AuthUser>(ticket.UserData);
                ViewBag.Token = user.Token;
                ViewBag.RefreshToken = user.RefreshToken;
            }

            ViewBag.AdminApiUrl = GetAdminApiUrl();

            return View();
        }

        [HttpPost]
        public string GetSecurityData()
        {
            var adminQueries = DependencyResolver.Current.GetService<IAdminQueries>();
            var authQueries = DependencyResolver.Current.GetService<IAuthQueries>();
            var admin = adminQueries.GetAdminById(CurrentUser.Id);

            var viewModel = new SecurityViewModel
            {
                UserName = CurrentUser.UserName,
                Operations = authQueries.GetPermissions().Select(p => new PermissionViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Module = p.Module
                }),
                UserPermissions = authQueries.GetActorPermissions(admin.Id).Select(p => p.Id),
                Licensees = admin.Licensees.Select(l => l.Id),
                Permissions = ConstantsHelper.GetConstantsDictionary<Permissions>(),
                Categories = ConstantsHelper.GetConstantsDictionary<Modules>(),
                IsSingleBrand = admin.AllowedBrands.Count == 1,
                IsSuperAdmin = admin.Role.Id == RoleIds.SuperAdminId
            };

            return SerializeJson(viewModel);
        }
    }
}