using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Messaging.ApplicationServices;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.RegoBus.Bus;
using AFT.RegoV2.RegoBus.EventStore;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Infrastructure.DependencyResolution
{
    public class EventBusContainerFactory
    {
        public void RegisterTypes(IUnityContainer container)
        {
            var bus = new EventBus();

            //========================
            //========================

            bus.Subscribe(() => container.Resolve<EventStoreSubscriber>());
            bus.Subscribe(() => container.Resolve<PaymentSubscriber>());
            bus.Subscribe(() => container.Resolve<FraudSubdomainSubscriber>());
            bus.Subscribe(() => container.Resolve<WalletSubscriber>());
            bus.Subscribe(() => container.Resolve<GameSubscriber>());
            bus.Subscribe(() => container.Resolve<PlayerSubscriber>());
            bus.Subscribe(() => container.Resolve<RiskLevelSubscriber>());
            // bus.Subscribe(() => container.Resolve<BonusSubscriber>());
            bus.Subscribe(() => container.Resolve<SecuritySubscriber>());
            bus.Subscribe(() => container.Resolve<BrandSubscriber>());
            bus.Subscribe(() => container.Resolve<MessagingSubscriber>());
            container.RegisterInstance<IEventBus>(bus, new ContainerControlledLifetimeManager());
        }
    }
}
