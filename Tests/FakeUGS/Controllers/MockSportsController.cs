using System.Web.Http;
using FakeUGS.Attributes;
using FakeUGS.Classes;
using FakeUGS.Core.Interfaces;
using FakeUGS.Core.Services;

namespace FakeUGS.Controllers
{
    [RoutePrefix("api/sports")]
    [ForGameProvider("MOCK_SPORT_BETS")]
    public sealed class MockSportsController : CommonGameProviderController
    {
        public MockSportsController(
            ICommonGameActionsProvider gameActions,
            IFlycowApiClientSettingsProvider flycowApiClientSettingsProvider)
            : base(gameActions, flycowApiClientSettingsProvider)
        {
        }
    }
}
