using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.Infrastructure.DataAccess.Player;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Infrastructure.DependencyResolution
{
    public class PlayerContainerFactory
    {
        public void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IPlayerQueries, PlayerQueries>();
            container.RegisterType<IPlayerRepository, PlayerRepository>();
            container.RegisterType<IPlayerIdentityValidator, PlayerIdentityValidator>();
            container.RegisterType<IPlayerCommands, PlayerCommands>();
        }
    }
}
