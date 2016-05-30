using System;
using System.Net.Http;
using System.Web.Http;

namespace AFT.RegoV2.AdminApi.Controllers.Base
{
    public class ErrorController : ApiController
    {
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpHead, HttpOptions, AcceptVerbs("PATCH"), System.Diagnostics.DebuggerHidden]
        public HttpResponseMessage Handle404()
        {
            throw new Exception("The requested resource is not found.");
        }
    }
}