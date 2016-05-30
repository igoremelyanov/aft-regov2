using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Providers;
using AFT.RegoV2.Infrastructure.DataAccess.Game;
using AFT.RegoV2.Infrastructure.Providers;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace AFT.RegoV2.Infrastructure.DependencyResolution
{
    public class GameContainerFactory
    {
        public void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IGameRepository, GameRepository>(new PerHttpRequestLifetime());
            container.RegisterType<IUgsGameCommandsAdapter, UgsGameCommandsAdapter>(new PerHttpRequestLifetime());
            container.RegisterType<ITransactionScopeProvider, TransactionScopeProvider>();
            container.RegisterType<IJsonSerializationProvider, JsonSerializationProvider>();
            container.RegisterType<ITokenProvider, TokenProvider>();
            container.RegisterType<IGameWalletOperations, GameWalletOperations>();
            container.RegisterType<IGameCommands, GameCommands>();

            container.RegisterType<IGameQueries, GameQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
            container.RegisterType<IGameManagement, GameManagement>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior());
        }

        public virtual InterceptionBehavior GetSecurityInterceptionBehavior()
        {
            return new InterceptionBehavior<DummyInterceptionBehavior>();
        }
    }
}
