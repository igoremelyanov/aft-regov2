
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface ICommonSettingsProvider : IBaseSettingsProvider
    {
        string GetOperatorApiUrl();
        string GetGameApiUrl();

        string GetBonusApiUrl();
        BonusApiCredentials GetBonusApiCredentials();

        string GetAdminApiUrl();
        string GetMemberApiUrl();
        string GetMemberWebsiteUrl();
        string GetAdminWebsiteUrl();
        string GetGameWebsiteUrl();
        string GetPaymentProxyUrl();
        string GetPaymentNotifyUrl();
    }
}
