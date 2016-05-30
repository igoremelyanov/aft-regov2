using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Core.Security.Interface.ApplicationServices;
using AFT.RegoV2.Core.Security.Interface.Data;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AutoMapper;

namespace AFT.RegoV2.Core.Security.ApplicationServices
{
    public class LoggingService : IApplicationService, ILoggingService
    {
        private readonly IEventBus _eventBus;
        private readonly ISecurityRepository _repository;

        public LoggingService(IEventBus eventBus, ISecurityRepository repository)
        {
            _eventBus = eventBus;
            _repository = repository;
        }

        public void Log(Error error)
        {
            if (error.Type == typeof(RegoValidationException).FullName)
            {
                var validationEvent = Mapper.DynamicMap<ValidationFailed>(error);
                _eventBus.Publish(validationEvent);
            }
            else
            {
                var errorEvent = Mapper.DynamicMap<ErrorRaised>(error);
                _eventBus.Publish(errorEvent);
            }
        }

        public Error GetError(Guid id)
        {
            return _repository.Errors.SingleOrDefault(e => e.Id == id);
        }

        public IQueryable<Error> GetErrors()
        {
            return _repository.Errors.AsQueryable();
        }
    }
}
