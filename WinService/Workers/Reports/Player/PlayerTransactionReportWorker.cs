using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Bonus.Core.Models.Events.Wallet;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class PlayerTransactionReportWorker : WorkerBase<PlayerTransactionReportSubscriber>
    {
        public PlayerTransactionReportWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class PlayerTransactionReportSubscriber : IBusSubscriber,
        IConsumes<BonusWalletBalanceChanged>
    {
        private readonly PlayerTransactionReportEventHandlers _eventHandlers;

        public PlayerTransactionReportSubscriber(PlayerTransactionReportEventHandlers eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public void Consume(BonusWalletBalanceChanged message)
        {
            _eventHandlers.Handle(message);
        }
    }
}
