using System;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using AFT.RegoV2.Bonus.Api;
using AFT.RegoV2.Bonus.Api.Filters;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Api.Provider;
using AFT.RegoV2.Bonus.Infrastructure;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Providers;
using AFT.RegoV2.Infrastructure.DataAccess.Base;
using AFT.RegoV2.Infrastructure.DataAccess.Event;
using AFT.RegoV2.Infrastructure.DataAccess.Security;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.Infrastructure.GameIntegration;
using AFT.RegoV2.Infrastructure.Providers;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Shared.Synchronization;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Owin;
using Unity.WebApi;

[assembly: OwinStartup(typeof(Startup))]
namespace AFT.RegoV2.Bonus.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var unityContainer = GetUnityContainer();

            var logger = unityContainer.Resolve<ILog>();
            logger.Info("Starting BonusApi application ...");

            app.UseCors(CorsOptions.AllowAll);
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString(Routes.Token),
                Provider = new AuthServerProvider(unityContainer),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(5)
            })
            .UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            var config = new HttpConfiguration { DependencyResolver = new UnityDependencyResolver(unityContainer) };
            MapRoutes(config);

            config.Services.Replace(typeof(IExceptionLogger), new ApiExceptionLogger(unityContainer));
            config.Services.Replace(typeof(IExceptionHandler), new ApiExceptionHandler());
            config.MessageHandlers.Add(new RequireHttpsHandler());

            app.UseWebApi(config);

            var synchronizationService = unityContainer.Resolve<ISynchronizationService>();
            synchronizationService.Execute("WinService", () =>
            {
                var repositoryBase = unityContainer.Resolve<IRepositoryBase>();
                if (!repositoryBase.IsDatabaseSeeded())
                    throw new Exception(Messages.DbExceptionMessage);
            });

            logger.Info("BonusApi application started successfully.");
        }

        private void MapRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "Error404",
                routeTemplate: "{*url}",
                defaults: new { controller = "Error", action = "Handle404" }
            );
        }

        protected virtual IUnityContainer GetUnityContainer()
        {
            var container = new UnityContainer();
            container.RegisterType<IActorInfoProvider, ActorInfoProvider>();
            container.RegisterType<IEventRepository, EventRepository>(new PerHttpRequestLifetime());
            container.RegisterType<ISynchronizationService, SynchronizationService>();
            new EventBusContainerFactory().RegisterTypes(container);
            new ServiceBusContainerFactory().RegisterTypes(container);
            container.RegisterType<IServiceBusSettingsProvider, ServiceBusSettingsProvider>();
            container.RegisterType<ICommonSettingsProvider, CommonSettingsProvider>();
            new SettingsContainerFactory().RegisterTypes(container);
            container.RegisterType<ILog, LogDecorator>(new ContainerControlledLifetimeManager());
            new AuthContainerFactory().RegisterTypes(container);
            container.RegisterType<ISecurityRepository, SecurityRepository>(new PerHttpRequestLifetime());
            container.RegisterType<IAdminQueries, AdminQueries>();
            container.RegisterType<IBrandApiClientFactory, BrandApiClientFactory>();
            container.RegisterType<IBrandOperations, BrandOperations>();
            new WalletContainerFactory().RegisterTypes(container);
            new GameContainerFactory().RegisterTypes(container);
            new PlayerContainerFactory().RegisterTypes(container);
            new BrandContainerFactory().RegisterTypes(container);
            container.RegisterType<ISynchronizationService, SynchronizationService>();
            container.RegisterType<IRepositoryBase, RepositoryBase>();

            new BonusContainerFactory().RegisterTypes(container);
            return container;
        }
    }
}