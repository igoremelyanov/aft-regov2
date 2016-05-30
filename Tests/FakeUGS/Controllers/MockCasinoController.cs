using System.Web.Http;
using FakeUGS.Attributes;
using FakeUGS.Classes;
using FakeUGS.Core.Interfaces;
using FakeUGS.Core.Services;

namespace FakeUGS.Controllers
{
    [RoutePrefix("api/mock")]
    [ForGameProvider("MOCK_CASINO")]
    public sealed class MockCasinoController : CommonGameProviderController
    {
        public MockCasinoController(
            ICommonGameActionsProvider gameActions,
            IFlycowApiClientSettingsProvider flycowApiClientSettingsProvider)
            : base(gameActions, flycowApiClientSettingsProvider)
        {
        }
    }
}
