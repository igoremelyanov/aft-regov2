using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;

namespace WinService.Workers.Fraud
{
    public class FraudPlayerWorker : WorkerBase<FraudPlayerSubscriber>
    {
        public FraudPlayerWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class FraudPlayerSubscriber : IBusSubscriber,
        IConsumes<PlayerRegistered>
    {
        private readonly FraudSubdomainSubscriber _eventHandlers;

        public FraudPlayerSubscriber(FraudSubdomainSubscriber eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public void Consume(PlayerRegistered message)
        {
            _eventHandlers.Consume(message);
        }
    }
}
