using System;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.Security;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Infrastructure.DataAccess.Base;
using AFT.RegoV2.Infrastructure.Providers;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Shared.Synchronization;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;

namespace AFT.RegoV2.AdminWebsite
{
    public class MvcApplication : HttpApplication
    {
        private IUnityContainer _container;
        protected void Application_Start()
        {
            _container = Bootstrapper.Initialise();

            var logger = _container.Resolve<ILog>();
            logger.Info("Starting AdminWebsite application ...");

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
           

            var synchronizationService = _container.Resolve<SynchronizationService>();
            synchronizationService.Execute("WinService", () =>
            {
                var repositoryBase = _container.Resolve<IRepositoryBase>();
                if (!repositoryBase.IsDatabaseSeeded())
                    throw new RegoException(Messages.DbExceptionMessage);
            });

            logger.Info("AdminWebsite application started successfully.");
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            Response.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.AddHeader("Pragma", "no-cache"); // HTTP 1.0.
            Response.AddHeader("Expires", "0"); // Proxies.
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exc = Server.GetLastError();

            var isAjaxCall = string.Equals("XMLHttpRequest", Context.Request.Headers["x-requested-with"], StringComparison.OrdinalIgnoreCase);

            if (isAjaxCall)
            {
                Context.ClearError();
                Context.Response.ContentType = "application/json";
                Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Context.Response.Write(
                    new JavaScriptSerializer().Serialize(
                        new
                        {
                            exc.Message,
                            Detail = exc.StackTrace,
                            MethodName = exc.TargetSite.Name,
                            exc.Source,
                            User = Context.User.Identity.Name,
                            Time = DateTimeOffset.Now
                        }
                    )
                );
            }
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                var authUser = JsonConvert.DeserializeObject<AuthUser>(authTicket.UserData);

                var idenityProvider = DependencyResolver.Current.GetService<ClaimsIdentityProvider>();
                var identity = idenityProvider.GetActorIdentity(authUser.UserId, Thread.CurrentPrincipal.Identity.AuthenticationType);
                HttpContext.Current.User = Thread.CurrentPrincipal = new ClaimsPrincipal(identity);
            }
        }
    }
}