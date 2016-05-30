using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.Auth;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Messaging;
using AFT.RegoV2.Core.Fraud;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Infrastructure.DataAccess.Base;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.Infrastructure.GameIntegration;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Synchronization;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using Moq;

namespace AFT.RegoV2.Tests.Common
{
    public class SingleProcessTestContainerFactory : ApplicationContainerFactory
    {
        public override void RegisterTypes(IUnityContainer container)
        {
            base.RegisterTypes(container);

            //we are mocking only out-of-process components of the system, like database, filesystem, message serviceBus
            container.RegisterInstance<IFileStorage>(new Mock<IFileStorage>().Object);

            container.RegisterType<IServiceBus, FakeServiceBus>(new ContainerControlledLifetimeManager());
            container.RegisterType<IBrandRepository, FakeBrandRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPlayerRepository, FakePlayerRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPaymentRepository, FakePaymentRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IFraudRepository, FakeFraudRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISecurityRepository, FakeSecurityRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IAuthRepository, FakeAuthRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IGameRepository, FakeGameRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IEventRepository, FakeEventRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IReportRepository, FakeReportRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IFraudRepository, FakeFraudRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IMessagingRepository, FakeMessagingRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISettingsRepository, FakeSettingsRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IEmailNotifier, FakeEmailNotifier>(new ContainerControlledLifetimeManager());
            container.RegisterType<IActorInfoProvider, FakeActorInfoProvider>(new ContainerControlledLifetimeManager());
            container.RegisterType<IUgsGameCommandsAdapter, UgsGameCommandsAdapter>(new ContainerControlledLifetimeManager());


            container.RegisterType<IBrandApiClientFactory, LocalBrandApiClientFactory>();
            container.RegisterType<IProductApiClientFactory, LocalProductApiClientFactory>();
            container.RegisterInstance(new Mock<IBonusApiProxy>().Object);

            container.RegisterType<IRepositoryBase, FakeRepositoryBase>();
            container.RegisterType<ISynchronizationService, FakeSynchronizationService>(new ContainerControlledLifetimeManager());
            //TestServiceBusSubscriber publishes events to fake service bus
            var domainBus = container.Resolve<IEventBus>();
            domainBus.Subscribe<TestServiceBusSubscriber>(subscriberFactory: () => container.Resolve<TestServiceBusSubscriber>());
        }
    }
}