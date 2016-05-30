using System.Web.Configuration;

namespace AFT.RegoV2.GameApi.Shared.Services
{
    public interface IWebConfigProvider
    {
        string GetAppSettingByKey(string key);
    }

    public sealed class WebConfigProvider : IWebConfigProvider
    {
        string IWebConfigProvider.GetAppSettingByKey(string key)
        {
            return WebConfigurationManager.AppSettings[key];
        }
    }
}
