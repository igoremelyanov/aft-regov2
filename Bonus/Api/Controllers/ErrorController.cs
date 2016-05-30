using System.Web.Http;
using System.Web.Http.Results;

namespace AFT.RegoV2.Bonus.Api.Controllers
{
    public class ErrorController : ApiController
    {
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpHead, HttpOptions, AcceptVerbs("PATCH"), System.Diagnostics.DebuggerHidden]
        public JsonResult<string> Handle404()
        {
            return Json("The requested resource is not found.");
        }
    }
}