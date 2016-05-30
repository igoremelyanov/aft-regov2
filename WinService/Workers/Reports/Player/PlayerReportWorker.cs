using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class PlayerReportWorker : WorkerBase<PlayerReportSubscriber>
    {
        public PlayerReportWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class PlayerReportSubscriber : IBusSubscriber,
        IConsumes<PlayerRegistered>,
        IConsumes<PlayerUpdated>,
        IConsumes<PlayerActivated>,
        IConsumes<PlayerDeactivated>
    {
        private readonly PlayerReportEventHandlers _eventHandlers;

        public PlayerReportSubscriber(PlayerReportEventHandlers eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public void Consume(PlayerRegistered message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerDeactivated message)
        {
            _eventHandlers.Handle(message);
        }
    }
}
