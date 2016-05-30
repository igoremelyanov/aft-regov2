using System.Web.Http;
using AFT.RegoV2.GameApi.Attributes;
using AFT.RegoV2.GameApi.Services;
using AFT.RegoV2.GameApi.Classes;

namespace AFT.RegoV2.GameApi.Controllers
{
    [RoutePrefix("api/mock")]
    [ForGameProvider("1814418D-BC00-43B4-AD18-BEBEF6501D7F")]
    public sealed class MockCasinoController : CommonGameProviderController
    {
        public MockCasinoController(ICommonGameActionsProvider gameActions) : base(gameActions)
        {
        }
    }
}
