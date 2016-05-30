using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using AFT.RegoV2.AdminApi.Interface.Common;
using Newtonsoft.Json;

namespace AFT.RegoV2.AdminApi.Filters
{
    public class AdminApiExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            base.Handle(context);

            if (context.Exception is HttpException)
            {
                return;
            }

            var content = new AdminApiException
            {
                ErrorCode = HttpStatusCode.InternalServerError.ToString(),
                ErrorMessage = context.Exception.Message,
                StackTrace = context.Exception.StackTrace
            };

            context.Result = new ErrorResult
            {
                Request = context.ExceptionContext.Request,
                Content = JsonConvert.SerializeObject(content)
            };
        }

        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return true;
        }

        private class ErrorResult : IHttpActionResult
        {
            public HttpRequestMessage Request { get; set; }
            public string Content { get; set; }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(Content),
                    RequestMessage = Request
                };

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return Task.FromResult(response);
            }
        }
    }
}