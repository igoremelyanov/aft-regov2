using AFT.UGS.Core.ProductConsumerClient;

namespace AFT.RegoV2.Infrastructure.GameIntegration
{
    public interface IProductApiClientFactory
    {
        IProductApiClient GetApiClient();
    }
}
