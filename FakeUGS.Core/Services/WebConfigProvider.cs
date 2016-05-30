using System.Web.Configuration;

namespace FakeUGS.Core.Services
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
