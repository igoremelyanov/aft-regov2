using AFT.RegoV2.AdminWebsite;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Messaging.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.Shared.Synchronization;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Containers
{
    public class AdminWebsiteUnitTestContainerFactory : IContainerFactory
    {
        private readonly AdminWebsiteContainerFactory _adminWebsiteContainerFactory;
        private readonly SingleProcessTestContainerFactory _singleProcessTestContainerFactory;

        public AdminWebsiteUnitTestContainerFactory()
        {
            //we're not doing injection here, as it is factory already and it's very unlikely that we may need injection here
            _adminWebsiteContainerFactory = new AdminWebsiteContainerFactory();
            _singleProcessTestContainerFactory = new SingleProcessTestContainerFactory();
        }

        public IUnityContainer CreateWithRegisteredTypes()
        {
            var container = new UnityContainer();
            _adminWebsiteContainerFactory.RegisterTypes(container);
            _singleProcessTestContainerFactory.RegisterTypes(container);
            this.RegisterTypes(container);
            return container;
        }

        public void RegisterTypes(IUnityContainer container)
        {
            //those types need to be registered as singletons in our unit-tests
            container.RegisterType<IGameWalletQueries, GameWalletQueries>(new ContainerControlledLifetimeManager());
            container.RegisterType<BrandQueries>(new ContainerControlledLifetimeManager());
            container.RegisterType<IGameCommands, GameCommands>(new ContainerControlledLifetimeManager());
            container.RegisterType<IGameQueries, GameQueries>(new ContainerControlledLifetimeManager());
            container.RegisterType<IMessageTemplateQueries, MessageTemplateQueries>(new ContainerControlledLifetimeManager());
            container.RegisterType<IMessageTemplateCommands, MessageTemplateCommands>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISynchronizationService, FakeSynchronizationService>();
            container.RegisterType<IDocumentService, FakeDocumentService>();
            container.RegisterType<IActorInfoProvider, FakeActorInfoProvider>(new ContainerControlledLifetimeManager());
        }
    }
}