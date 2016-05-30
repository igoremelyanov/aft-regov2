using System.Web.Http.Filters;

namespace FakeUGS.Attributes
{
    public class ProcessErrorAttribute : BaseErrorAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if(context.Exception == null) return;
            
            context.Response = GetResponseByException(context);
        }
    }
}