using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class VipLevelReportWorker : WorkerBase<VipLevelReportSubscriber>
    {
        public VipLevelReportWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class VipLevelReportSubscriber : IBusSubscriber,
        IConsumes<VipLevelRegistered>,
        IConsumes<VipLevelUpdated>,
        IConsumes<VipLevelActivated>,
        IConsumes<VipLevelDeactivated>
    {
        private readonly VipLevelReportEventHandlers _eventHandlers;

        public VipLevelReportSubscriber(VipLevelReportEventHandlers eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public void Consume(VipLevelRegistered message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(VipLevelUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(VipLevelActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(VipLevelDeactivated message)
        {
            _eventHandlers.Handle(message);
        }
    }
}
