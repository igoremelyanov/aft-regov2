using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Providers;

using FakeUGS.Core.Interfaces;

namespace FakeUGS.Core.Providers
{
    public class FlycowApiClientSettingsProvider : BaseSettingsProvider, IFlycowApiClientSettingsProvider
    {
        private static class SettingKeys
        {
            public static readonly string ClientId = "FlycowApiClient.ClientId";
            public static readonly string ClientSecret = "FlycowApiClient.ClientSecret";
        }

        public FlycowApiClientSettingsProvider(ISettingsQueries settingsQueries) : base(settingsQueries)
        {
        }

        public string GetClientId()
        {
            return GetByKey(SettingKeys.ClientId);
        }

        public string GetClientSecret()
        {
            return GetByKey(SettingKeys.ClientSecret);
        }
    }
}