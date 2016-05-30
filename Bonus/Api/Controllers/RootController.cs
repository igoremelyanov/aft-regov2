using System.Web.Http;
using System.Web.Http.Results;

namespace AFT.RegoV2.Bonus.Api.Controllers
{
    public class RootController : ApiController
    {
        [Route]
        [AllowAnonymous]
        public JsonResult<string> Get()
        {
            return Json("RegoV2 Bonus API");
        }
    }
}