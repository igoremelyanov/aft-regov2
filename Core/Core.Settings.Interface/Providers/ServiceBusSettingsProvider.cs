using System.Security;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Settings.Interface.Providers
{
    public class ServiceBusSettingsProvider : BaseSettingsProvider, IServiceBusSettingsProvider
    {
        private static class SettingKeys
        {
            public static readonly string WsbHttpPort = "wsb.httpport";
            public static readonly string WsbTcpPort = "wsb.tcpport";
            public static readonly string WsbNamespace = "wsb.namespace";
            public static readonly string WsbHost = "wsb.host";
            public static readonly string WsbUsername = "wsb.username";
            public static readonly string WsbPassword = "wsb.password";
            public static readonly string WsbUserDomain = "wsb.userdomain";
        }

        public ServiceBusSettingsProvider(ISettingsQueries settingsQueries) : base(settingsQueries)
        {
        }

        public int GetWsbHttpPort()
        {
            return int.Parse(GetByKey(SettingKeys.WsbHttpPort));
        }

        public int GetWsbTcpPort()
        {
            return int.Parse(GetByKey(SettingKeys.WsbTcpPort));
        }

        public string GetWsbNamespace()
        {
            return GetByKey(SettingKeys.WsbNamespace);
        }

        public string GetWsbHost()
        {
            return GetByKey(SettingKeys.WsbHost);
        }

        public string GetWsbUsername()
        {
            return GetByKey(SettingKeys.WsbUsername);
        }

        public string GetWsbUserDomain()
        {
            return GetByKey(SettingKeys.WsbUserDomain);
        }

        public SecureString GetWsbPasswordSecure()
        {
            return GetByKey(SettingKeys.WsbPassword).ConvertToSecureString();
        }

        public string GetWsbPassword()
        {
            return GetByKey(SettingKeys.WsbPassword);
        }
    }
}