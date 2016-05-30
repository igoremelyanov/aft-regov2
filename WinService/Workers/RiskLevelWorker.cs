using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

using RiskLevelEventHandlers = AFT.RegoV2.Core.Fraud.ApplicationServices.RiskLevelSubscriber;

namespace AFT.RegoV2.WinService.Workers
{
    public class RiskLevelWorker : WorkerBase<RiskLevelSubscriber>
    {
        public RiskLevelWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) {}
    }

    public class RiskLevelSubscriber : IBusSubscriber,
        IConsumes<BrandRegistered>,
        IConsumes<BrandUpdated>
    {
        private readonly RiskLevelEventHandlers _eventHandlers;

        public RiskLevelSubscriber(RiskLevelEventHandlers handlers)
        {
            this._eventHandlers = handlers;
        }

        public void Consume(BrandRegistered message)
        {
            _eventHandlers.Consume(message);
        }

        public void Consume(BrandUpdated message)
        {
            _eventHandlers.Consume(message);
        }
    }
}
