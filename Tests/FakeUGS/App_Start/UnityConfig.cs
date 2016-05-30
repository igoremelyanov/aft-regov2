using System;
using System.Web;
using System.Web.Hosting;

using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Core.Settings.ApplicationServices;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Providers;
using AFT.RegoV2.Infrastructure.DataAccess.Base;
using AFT.RegoV2.Infrastructure.DataAccess.Settings;
using AFT.RegoV2.Infrastructure.OAuth2;
using AFT.RegoV2.Infrastructure.Providers;
using AFT.RegoV2.RegoBus.Bus;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Synchronization;
using FakeUGS.Core.ApplicationServices;
using FakeUGS.Core.Bus;
using FakeUGS.Core.DataAccess;
using FakeUGS.Core.Interfaces;
using FakeUGS.Core.Providers;
using FakeUGS.Core.Services;
using Microsoft.Practices.Unity;

namespace FakeUGS.App_Start
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container
        private static readonly Lazy<IUnityContainer> Container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return Container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {

            RegisterEventBus(container);
            
            container.RegisterType<ILog, LogDecorator>();
            container.RegisterType<ITokenProvider, TokenProvider>();

            //use of explicit factory method to prevent Func from doing property injection 
            //and stuffing your public properties with nulls (surprise!)
            container.RegisterType<IRepository>(ReuseWithinRequest, new InjectionFactory(c => new Repository()));

            container.RegisterType<IWalletOperations, WalletOperations>();
            container.RegisterType<ISynchronizationService, SynchronizationService>();
            container.RegisterType<IUgsServiceBus, FakeUgsServiceBus>();

            container.RegisterType<ISettingsRepository, SettingsRepository>();
            container.RegisterType<ISettingsQueries, SettingsQueries>();
            container.RegisterType<IUgsServiceBusSettingsProvider, UgsServiceBusSettingsProvider>();
            container.RegisterType<ICommonSettingsProvider, CommonSettingsProvider>();

            container.RegisterType<IRepositoryBase, RepositoryBase>();
            container.RegisterType<ISynchronizationService, SynchronizationService>();
            container.RegisterType<IJsonSerializationProvider, JsonSerializationProvider>();
            container.RegisterType<IWebConfigProvider, WebConfigProvider>();
            container.RegisterType<ITokenProvider, TokenProvider>();
            container.RegisterType<IErrorManager, ErrorManager>();
            container.RegisterType<ITokenValidationProvider, TokenValidationProvider>();
            container.RegisterType<IGameProviderLog, GameProviderLog>();
            container.RegisterType<ITransactionScopeProvider, TransactionScopeProvider>();
            container.RegisterType<IGameCommands, GameCommands>();
            container.RegisterType<IGameQueries, GameQueries>();
            container.RegisterType<ICommonGameActionsProvider, CommonGameActionsProvider>();
            container.RegisterType<IGameManagement, GameManagement>();
            container.RegisterType<IGameWalletOperations, GameWalletOperations>();
            container.RegisterType<IGameWalletQueries, GameWalletQueries>();
            container.RegisterType<IPlayerCommands, PlayerCommands>();
            container.RegisterType<IGameEventsProcessor, GameEventsProcessor>();
            container.RegisterType<IFlycowApiClientSettingsProvider, FlycowApiClientSettingsProvider>();
            container.RegisterType<IModeSwitch, ModeSwitch>();
            container.RegisterType<IFlycowApiClientProvider, FlycowApiClientProvider>();

            container.RegisterType<ICryptoKeyPair>("authServer", NewEachTime, new InjectionFactory(c =>
            {
                var config = c.Resolve<IWebConfigProvider>();
                return CryptoKeyPair.LoadCertificate(
                            HostingEnvironment.MapPath(config.GetAppSettingByKey("CertificateLocation")),
                            config.GetAppSettingByKey("CertificatePassword"));
            }));

            container.RegisterType<ICryptoKeyPair>("dataServer", NewEachTime, new InjectionFactory(c =>
            {
                var config = c.Resolve<IWebConfigProvider>();
                return CryptoKeyPair.LoadCertificate(
                            HostingEnvironment.MapPath(config.GetAppSettingByKey("CertificateLocation")),
                            config.GetAppSettingByKey("CertificatePassword"));
            }));

        }

        private static void RegisterEventBus(IUnityContainer container)
        {
            var bus = new EventBus();
            container.RegisterInstance<IEventBus>(bus, new ContainerControlledLifetimeManager());
        }

        private static LifetimeManager ReuseWithinContainer => new ContainerControlledLifetimeManager();
        private static LifetimeManager ReuseWithinRequest => new PerHttpRequestLifetime();
        private static LifetimeManager NewEachTime => new PerResolveLifetimeManager();

    }

    public class PerHttpRequestLifetime : LifetimeManager
    {
        private readonly Guid _key = Guid.NewGuid();

        public override object GetValue()
        {
            var context = HttpContext.Current;
            return context?.Items[_key];
        }

        public override void SetValue(object newValue)
        {
            var context = HttpContext.Current;
            if (context != null)
            {
                context.Items[_key] = newValue;
            }
        }

        public override void RemoveValue()
        {
            var context = HttpContext.Current;
            if (context == null) return;
            var obj = GetValue();
            context.Items.Remove(obj);
        }
    }
}
