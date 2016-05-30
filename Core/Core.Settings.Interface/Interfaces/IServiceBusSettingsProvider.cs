using System.Security;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Settings.Interface.Interfaces
{
    public interface IServiceBusSettingsProvider : IBaseSettingsProvider
    {
        int GetWsbHttpPort();

        int GetWsbTcpPort();

        string GetWsbNamespace();

        string GetWsbHost();

        string GetWsbUsername();

        string GetWsbUserDomain();

        SecureString GetWsbPasswordSecure();
        string GetWsbPassword();
    }
}
