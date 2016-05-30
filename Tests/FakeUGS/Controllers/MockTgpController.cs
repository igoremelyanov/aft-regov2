using System.Web.Http;
using FakeUGS.Attributes;
using FakeUGS.Classes;
using FakeUGS.Core.Interfaces;
using FakeUGS.Core.Services;

namespace AFT.RegoV2.FakeUGS.Controllers
{
    [RoutePrefix("api/mocktgp")]
    [ForGameProvider("TGP")]
    public sealed class MockTgpController : CommonGameProviderController
    {
        public MockTgpController(ICommonGameActionsProvider gameActions,
            IFlycowApiClientSettingsProvider flycowApiClientSettingsProvider)
            : base(gameActions, flycowApiClientSettingsProvider)
        {
        }
    }
}