using System;
using System.Web.Http.ExceptionHandling;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Interface.Data;
using AFT.RegoV2.Infrastructure.DependencyResolution;

using Microsoft.Practices.Unity;

namespace AFT.RegoV2.MemberApi.Filters
{
    public class MemberApiExceptionLogger : ExceptionLogger
    {
        private readonly IUnityContainer _container;

        public MemberApiExceptionLogger(IUnityContainer container)
        {
            _container = container;
        }
        public override void Log(ExceptionLoggerContext context)
        {
            if (context.Exception.Message.Contains("Access forbidden"))
                return;

            var exceptionMessage = context.Exception.Message + Environment.NewLine +
                                   context.Request.RequestUri.AbsoluteUri;
            
            var logger = _container.Resolve<ILog>();
            logger.Error(exceptionMessage, context.Exception);

            // Publish to event bus
            var service = _container.Resolve<ILoggingService>();
            var actorInfoProvider = _container.Resolve<IActorInfoProvider>();

            var error = new Error
            {
                Id = Guid.NewGuid(),
                Message = exceptionMessage,
                Type = context.Exception.GetType().ToString(),
                Source = context.Exception.Source,
                HostName = context.Request.RequestUri.Host,
                Detail = context.Exception.StackTrace,
                Time = DateTime.UtcNow,
                User = actorInfoProvider.Actor.UserName
            };

            service.Log(error);
        }
    }
}