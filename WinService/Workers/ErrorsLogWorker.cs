using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.RegoBus.Interfaces;

using Microsoft.Practices.Unity;
using Newtonsoft.Json;

namespace AFT.RegoV2.WinService.Workers
{
    public class ErrorsLogWorker : WorkerBase<ErrorsLogSubscriber>
    {
        public ErrorsLogWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class ErrorsLogSubscriber : IBusSubscriber, 
        IConsumes<ErrorRaised>
    {
        private readonly ILog _logger;

        public ErrorsLogSubscriber(ILog logger)
        {
            _logger = logger;
        }

        public void Consume(ErrorRaised @event)
        {
            var json = JsonConvert.SerializeObject(@event);
            _logger.Debug("ErrorRaised message details: " + json);
        }
    }
}
