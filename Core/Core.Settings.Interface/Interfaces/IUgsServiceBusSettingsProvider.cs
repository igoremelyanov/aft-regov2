using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Settings.Interface.Interfaces
{
    public interface IUgsServiceBusSettingsProvider : IBaseSettingsProvider
    {
        string GetUgsBusConnectionString();
    }
}
