using System.Web.Http;
using FakeUGS.Attributes;
using FakeUGS.Classes;
using FakeUGS.Core.Interfaces;
using FakeUGS.Core.Services;

namespace AFT.RegoV2.FakeUGS.Controllers
{
    [RoutePrefix("api/mockgolddeluxe")]
    [ForGameProvider("GD")]
    public sealed class MockGoldDeluxeController : CommonGameProviderController
    {
        public MockGoldDeluxeController(ICommonGameActionsProvider gameActions,
            IFlycowApiClientSettingsProvider flycowApiClientSettingsProvider)
            : base(gameActions, flycowApiClientSettingsProvider)
        {
        }
    }
}