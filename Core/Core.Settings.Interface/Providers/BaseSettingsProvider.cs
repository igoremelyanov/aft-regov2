using System.Configuration;
using System.Linq;

using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Exceptions;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;

namespace AFT.RegoV2.Core.Settings.Interface.Providers
{
    public abstract class BaseSettingsProvider : IBaseSettingsProvider
    {
        public string SeedSettingsPrefix
        {
            get { return "dbsettings:"; }
        }

        protected ISettingsQueries SettingsQueries { get; private set; }

        protected BaseSettingsProvider(ISettingsQueries settingsQueries)
        {
            SettingsQueries = settingsQueries;
        }

        // using 'checkConfigurationFileIfMissing = false' is only suitable for cases when fall back to file based configuration values is not desirable
        protected string GetByKey(string key, bool checkConfigurationFileIfMissing = true)
        {
            try
            {
                var value = SettingsQueries.Get(key);
                return value;
            }
            catch (MissingKeyException)
            {
                if (checkConfigurationFileIfMissing)
                {
                    var configKey = SeedSettingsPrefix + key;
                    if (ConfigurationManager.AppSettings.AllKeys.Contains(configKey))
                    {
                        return ConfigurationManager.AppSettings[configKey];
                    }

                    if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
                    {
                        return ConfigurationManager.AppSettings[key];
                    }
                }

                throw;
            }
        }
    }
}
