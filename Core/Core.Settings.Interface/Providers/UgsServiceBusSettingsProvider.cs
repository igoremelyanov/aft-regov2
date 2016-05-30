
using AFT.RegoV2.Core.Settings.Interface.Interfaces;

namespace AFT.RegoV2.Core.Settings.Interface.Providers
{
    public class UgsServiceBusSettingsProvider : BaseSettingsProvider, IUgsServiceBusSettingsProvider
    {
        private static class SettingKeys
        {
            public const string UgsBusConnectionString = "ugsbus.connectionString";
        }

        public string GetUgsBusConnectionString()
        {
            return GetByKey(SettingKeys.UgsBusConnectionString);
        }

        public UgsServiceBusSettingsProvider(ISettingsQueries settingsQueries) : base(settingsQueries)
        {
        }
    }
}
