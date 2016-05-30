using System;
using System.Web.Http.ExceptionHandling;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Interface.Data;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.AdminApi.Filters
{
    public class AdminApiExceptionLogger : ExceptionLogger
    {
        private IUnityContainer _container;
        private ILog _logger;
        private LoggingService _service;
        private IActorInfoProvider _actorInfoProvider;

        public override void Log(ExceptionLoggerContext context)
        {
            _container = new AdminApiContainerFactory().CreateWithRegisteredTypes();

            if (context.Exception.Message.Contains("Access forbidden"))
                return;

            var exceptionMessage = context.Exception.Message + Environment.NewLine +
                       context.Request.RequestUri.AbsoluteUri;
            
            _logger = _container.Resolve<ILog>();
            _logger.Error(exceptionMessage, context.Exception);

            // Publish to event bus
            _service = _container.Resolve<LoggingService>();
            _actorInfoProvider = _container.Resolve<IActorInfoProvider>();

            var error = new Error
            {
                Id = Guid.NewGuid(),
                Message = exceptionMessage,
                Type = context.Exception.GetType().ToString(),
                Source = context.Exception.Source,
                HostName = context.Request.RequestUri.Host,
                Detail = context.Exception.StackTrace,
                Time = DateTime.UtcNow,
                User = _actorInfoProvider.Actor.UserName
            };

            _service.Log(error);
        }
    }
}