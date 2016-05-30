using System;
using System.Web.Http;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Infrastructure.DataAccess.Base;
using AFT.RegoV2.MemberApi.Filters;
using AFT.RegoV2.MemberApi.Provider;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Shared.Synchronization;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Owin;
using Unity.WebApi;

[assembly: OwinStartup(typeof(AFT.RegoV2.MemberApi.Startup))]
namespace AFT.RegoV2.MemberApi
{
    public class Startup
    {
        public static IUnityContainer Container { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            Container = GetUnityContainer();
            var logger = Container.Resolve<ILog>();
            logger.Info("Starting MemberApi application ...");

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            app.Use<InvalidLoginOwinMiddleware>();

            
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AuthorizationCodeExpireTimeSpan = TimeSpan.FromHours(1),
                Provider = GetAuthorizationServerProvider(Container)
            });
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            var config = new HttpConfiguration { DependencyResolver = new UnityDependencyResolver(Container) };

            WebApiConfig.Register(config, Container);

            app.UseWebApi(config);

            var synchronizationService = Container.Resolve<ISynchronizationService>();
            synchronizationService.Execute("WinService", () =>
            {
                var repositoryBase = Container.Resolve<IRepositoryBase>();
                if (!repositoryBase.IsDatabaseSeeded())
                    throw new RegoException(Messages.DbExceptionMessage);
            });
            
            logger.Info("MemberApi application started sucessfully.");
        }

        protected virtual OAuthAuthorizationServerProvider GetAuthorizationServerProvider(IUnityContainer container)
        {
            return new AuthServerProvider(container);
        }

        protected virtual IUnityContainer GetUnityContainer()
        {
            return new MemberApiContainerFactory().CreateWithRegisteredTypes();
        }
    }
}