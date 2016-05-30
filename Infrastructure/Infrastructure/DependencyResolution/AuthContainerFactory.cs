using AFT.RegoV2.Core.Auth;
using AFT.RegoV2.Core.Auth.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Infrastructure.DataAccess.Auth;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Infrastructure.DependencyResolution
{
    public class AuthContainerFactory
    {
        public void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IAuthRepository, AuthRepository>(new PerHttpRequestLifetime());
            container.RegisterType<IActorInfoProvider, ActorInfoProvider>();
            container.RegisterType<IAuthCommands, AuthCommands>();
            container.RegisterType<IAuthQueries, AuthQueries>();
        }
    }
}
