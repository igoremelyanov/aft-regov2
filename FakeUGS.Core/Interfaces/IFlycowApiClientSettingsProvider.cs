namespace FakeUGS.Core.Interfaces
{
    public interface IFlycowApiClientSettingsProvider
    {
        string GetClientId();

        string GetClientSecret();
    }
}
