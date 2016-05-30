using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Providers;
using AFT.RegoV2.RegoBus.Bus;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.RegoBus.Providers;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Infrastructure.DependencyResolution
{
    public class ServiceBusContainerFactory
    {
        public void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IServiceBusConnectionStringProvider, ServiceBusConnectionStringProvider>();
            container.RegisterType<IUgsServiceBusSettingsProvider, UgsServiceBusSettingsProvider>();

            container.RegisterType<IServiceBus, WindowsServiceBus>();
            container.RegisterType<IUgsServiceBus, UgsServiceBus>();
        }
    }
}
