using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interface.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Infrastructure.GameIntegration;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Infrastructure.DependencyResolution
{
    public class WalletContainerFactory
    {
        public void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IGameWalletQueries, GameWalletQueries>();
            container.RegisterType<IWalletOperations, WalletOperations>();
            container.RegisterType<IBrandOperations, BrandOperations>();
            container.RegisterType<IProductOperations, ProductOperations>();
            container.RegisterType<IBrandCredentialsQueries, BrandCredentialsQueries>();
        }
    }
}
