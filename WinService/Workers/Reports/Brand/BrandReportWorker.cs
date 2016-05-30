using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class BrandReportWorker : WorkerBase<BrandReportSubscriber>
    {
        public BrandReportWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class BrandReportSubscriber : IBusSubscriber,
        IConsumes<BrandRegistered>,
        IConsumes<BrandUpdated>,
        IConsumes<BrandActivated>,
        IConsumes<BrandDeactivated>
    {
        private readonly BrandReportEventHandlers _eventHandlers;

        public BrandReportSubscriber(BrandReportEventHandlers eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public void Consume(BrandRegistered message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandDeactivated message)
        {
            _eventHandlers.Handle(message);
        }
    }
}
