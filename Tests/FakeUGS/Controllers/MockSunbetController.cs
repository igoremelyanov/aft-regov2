using System.Web.Http;
using FakeUGS.Attributes;
using FakeUGS.Classes;
using FakeUGS.Core.Interfaces;
using FakeUGS.Core.Services;

namespace AFT.RegoV2.FakeUGS.Controllers
{
    [RoutePrefix("api/mocksunbet")]
    [ForGameProvider("SB")]
    public sealed class MockSunbetController : CommonGameProviderController
    {
        public MockSunbetController(ICommonGameActionsProvider gameActions,
            IFlycowApiClientSettingsProvider flycowApiClientSettingsProvider)
            : base(gameActions, flycowApiClientSettingsProvider)
        {
        }
    }
}