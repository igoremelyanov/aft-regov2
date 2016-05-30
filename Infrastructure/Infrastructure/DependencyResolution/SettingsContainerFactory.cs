using AFT.RegoV2.Core.Settings.ApplicationServices;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Settings;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Infrastructure.DependencyResolution
{
    public class SettingsContainerFactory
    {
        public void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<ISettingsRepository, SettingsRepository>();
            container.RegisterType<ISettingsQueries, SettingsQueries>();
            container.RegisterType<ISettingsCommands, SettingsCommands>();
        }
    }
}
