using System.Web.Http;
using AFT.RegoV2.GameApi.Attributes;
using AFT.RegoV2.GameApi.Services;
using AFT.RegoV2.GameApi.Classes;

namespace AFT.RegoV2.GameApi.Controllers
{
    [RoutePrefix("api/sports")]
    [ForGameProvider("18FB823B-435D-42DF-867E-3BA38ED92060")]
    public sealed class MockSportsController : CommonGameProviderController
    {
        public MockSportsController(ICommonGameActionsProvider gameActions)
            : base(gameActions)
        {
        }
    }
}
