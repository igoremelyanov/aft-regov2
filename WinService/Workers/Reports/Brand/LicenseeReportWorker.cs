using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class LicenseeReportWorker : WorkerBase<LicenseeReportSubscriber>
    {
        public LicenseeReportWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }
    
    public class LicenseeReportSubscriber : IBusSubscriber,
        IConsumes<LicenseeCreated>,
        IConsumes<LicenseeUpdated>,
        IConsumes<LicenseeActivated>,
        IConsumes<LicenseeDeactivated>
    {
        private readonly LicenseeReportEventHandlers _eventHandlers;

        public LicenseeReportSubscriber(LicenseeReportEventHandlers eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public void Consume(LicenseeCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LicenseeUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LicenseeActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LicenseeDeactivated message)
        {
            _eventHandlers.Handle(message);
        }
    }
}
