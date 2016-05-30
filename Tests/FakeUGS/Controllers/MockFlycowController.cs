using System.Web.Http;
using FakeUGS.Attributes;
using FakeUGS.Classes;
using FakeUGS.Core.Interfaces;
using FakeUGS.Core.Services;

namespace AFT.RegoV2.FakeUGS.Controllers
{
    [RoutePrefix("api/mockflycow")]
    [ForGameProvider("FC")]
    public sealed class MockFlycowController : CommonGameProviderController
    {
        public MockFlycowController(ICommonGameActionsProvider gameActions,
            IFlycowApiClientSettingsProvider flycowApiClientSettingsProvider)
            : base(gameActions, flycowApiClientSettingsProvider)
        {
        }
    }
}