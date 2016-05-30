using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common
{
    public class MemberWebsiteUnitTestContainerFactory
    {
        private readonly SingleProcessTestContainerFactory _singleProcessTestContainerFactory;

        public MemberWebsiteUnitTestContainerFactory()
        {
            _singleProcessTestContainerFactory = new SingleProcessTestContainerFactory();
        }

        public void RegisterTypes(IUnityContainer container)
        {
            _singleProcessTestContainerFactory.RegisterTypes(container);

            container.RegisterType<IFileStorage, FileSystemStorage>();
            container.RegisterType<IWithdrawalService, WithdrawalService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IGameWalletQueries, GameWalletQueries>(new ContainerControlledLifetimeManager());
            container.RegisterType<BrandQueries, BrandQueries>(new ContainerControlledLifetimeManager());
            container.RegisterType<IGameCommands, GameCommands>(new ContainerControlledLifetimeManager());
            container.RegisterType<IGameQueries, GameQueries>(new ContainerControlledLifetimeManager());
        }

        public IUnityContainer CreateWithRegisteredTypes()
        {
            return _singleProcessTestContainerFactory.CreateWithRegisteredTypes();
        }
    }
}