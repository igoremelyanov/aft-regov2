using System;
using System.Web.Http.ExceptionHandling;

using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Interface.Data;

using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Bonus.Api.Filters
{
    public class ApiExceptionLogger : ExceptionLogger
    {
        private readonly IUnityContainer _container;

        public ApiExceptionLogger(IUnityContainer container)
        {
            _container = container;
        }

        public override void Log(ExceptionLoggerContext context)
        {
            var logger = _container.Resolve<ILog>();
            var exceptionMessage = context.Exception.Message + Environment.NewLine +
                context.Request.RequestUri.AbsoluteUri;
            logger.Error(exceptionMessage, context.Exception);

            // Publish to event bus
            var service = _container.Resolve<LoggingService>();
            var error = new Error
            {
                Id = Guid.NewGuid(),
                Message = context.Exception.Message,
                Type = context.Exception.GetType().ToString(),
                Source = context.Exception.Source,
                HostName = context.Request.RequestUri.Host,
                Detail = context.Exception.StackTrace,
                Time = DateTime.UtcNow,
                User = context.RequestContext.Principal.Identity.Name
            };
            service.Log(error);
        }
    }
}