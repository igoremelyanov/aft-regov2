using System.Web.Http;
using AFT.RegoV2.Infrastructure.Attributes;

namespace FakeUGS.Controllers
{
    [ForceJsonFormatter]
    public class RootController : ApiController
    {
        [Route("")]
        public string[] Get()
        {
            return new[] { "FakeUGS Game API" };
        }
    }
}
