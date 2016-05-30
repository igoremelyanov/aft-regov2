using System.Net;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Bonus.Api.Filters
{
    public class ApiExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            context.Result = new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(context.Exception.Message)
            });
        }

        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return context.Exception is RegoException;
        }
    }
}