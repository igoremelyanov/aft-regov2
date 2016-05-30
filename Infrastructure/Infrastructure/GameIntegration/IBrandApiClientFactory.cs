using AFT.UGS.Core.BrandClient;

namespace AFT.RegoV2.Infrastructure.GameIntegration
{
    public interface IBrandApiClientFactory
    {
        IBrandApiClient GetApiClient();
    }
}
