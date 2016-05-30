using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Providers;

namespace AFT.RegoV2.Infrastructure.Providers
{
    public class CommonSettingsProvider : BaseSettingsProvider, ICommonSettingsProvider
    {
        private static class SettingKeys
        {
            public static readonly string BonusApiUrl = "BonusApiUrl";
            public static readonly string BonusClientId = "BonusClientId";
            public static readonly string BonusClientSecret = "BonusClientSecret";

            public static readonly string OperatorApiUrl = "OperatorApiUrl";
            public static readonly string GameApiUrl = "GameApiUrl";
            public static readonly string AdminApiUrl = "AdminApiUrl";
            public static readonly string MemberApiUrl = "MemberApiUrl";
            public static readonly string MemberWebsiteUrl = "MemberWebsiteUrl";
            public static readonly string AdminWebsiteUrl = "AdminWebsiteUrl";
            public static readonly string GameWebsiteUrl = "GameWebsiteUrl";
            public static readonly string PaymentProxyUrl = "PaymentProxyUrl";
            public static readonly string PaymentNotifyUrl = "PaymentNotifyUrl";
        }

        public CommonSettingsProvider(ISettingsQueries settingsQueries) : base (settingsQueries)
        {
        }

        public string GetOperatorApiUrl()
        {
            return GetByKey(SettingKeys.OperatorApiUrl);
        }

        public string GetGameApiUrl()
        {
            return GetByKey(SettingKeys.GameApiUrl);
        }

        public string GetBonusApiUrl()
        {
            return GetByKey(SettingKeys.BonusApiUrl);
        }

        public BonusApiCredentials GetBonusApiCredentials()
        {
            return new BonusApiCredentials
            {
                ClientId = GetByKey(SettingKeys.BonusClientId),
                ClientSecret = GetByKey(SettingKeys.BonusClientSecret)
            };
        }

        public string GetAdminApiUrl()
        {
            return GetByKey(SettingKeys.AdminApiUrl);
        }

        public string GetMemberApiUrl()
        {
            return GetByKey(SettingKeys.MemberApiUrl);
        }

        public string GetMemberWebsiteUrl()
        {
            return GetByKey(SettingKeys.MemberWebsiteUrl);
        }

        public string GetAdminWebsiteUrl()
        {
            return GetByKey(SettingKeys.AdminWebsiteUrl);
        }

        public string GetGameWebsiteUrl()
        {
            return GetByKey(SettingKeys.GameWebsiteUrl);
        }

        public string GetPaymentProxyUrl()
        {
            return GetByKey(SettingKeys.PaymentProxyUrl);
        }

        public string GetPaymentNotifyUrl()
        {
            return GetByKey(SettingKeys.PaymentNotifyUrl);
        }
    }
}