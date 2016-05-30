using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class PlayerBetHistoryReportWorker : WorkerBase<PlayerBetHistoryReportSubscriber>
    {
        public PlayerBetHistoryReportWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class PlayerBetHistoryReportSubscriber : IBusSubscriber,
        IConsumes<BetPlaced>,
        IConsumes<BetWon>,
        IConsumes<BetAdjusted>,
        IConsumes<BetCancelled>
    {
        private readonly PlayerBetHistoryReportEventHandlers _eventHandlers;

        public PlayerBetHistoryReportSubscriber(PlayerBetHistoryReportEventHandlers eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public void Consume(BetPlaced message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BetWon message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BetAdjusted message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BetCancelled message)
        {
            _eventHandlers.Handle(message);
        }
    }
}
