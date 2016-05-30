using System.Threading.Tasks;

using AFT.UGS.Core.FlyCowClient;

namespace FakeUGS.Core.Interfaces
{
    public interface IFlycowApiClientProvider
    {
        IFlyCowApiClient GetApiClient();

        Task<string> GetApiToken(IFlyCowApiClient client);
    }
}
