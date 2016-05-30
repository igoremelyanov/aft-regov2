using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class LanguageReportWorker : WorkerBase<LanguageReportSubscriber>
    {
        public LanguageReportWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class LanguageReportSubscriber : IBusSubscriber,
        IConsumes<LanguageCreated>,
        IConsumes<LanguageUpdated>,
        IConsumes<LanguageStatusChanged>,
        IConsumes<LicenseeCreated>,
        IConsumes<LicenseeUpdated>,
        IConsumes<BrandLanguagesAssigned>
    {
        private readonly LanguageReportEventHandlers _eventHandlers;

        public LanguageReportSubscriber(LanguageReportEventHandlers eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public void Consume(LanguageCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LanguageUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LanguageStatusChanged message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LicenseeCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LicenseeUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandLanguagesAssigned message)
        {
            _eventHandlers.Handle(message);
        }
    }
}
