using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class DepositReportWorker : WorkerBase<DepositReportSubscriber>
    {
        public DepositReportWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class DepositReportSubscriber : IBusSubscriber,
        IConsumes<DepositSubmitted>,
        IConsumes<DepositConfirmed>,
        IConsumes<DepositVerified>,
        IConsumes<DepositApproved>,
        IConsumes<DepositUnverified>,
        IConsumes<DepositRejected>
    {
        private readonly DepositReportEventHandlers _eventHandlers;

        public DepositReportSubscriber(DepositReportEventHandlers eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public void Consume(DepositSubmitted message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(DepositConfirmed message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(DepositVerified message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(DepositApproved message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(DepositUnverified message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(DepositRejected message)
        {
            _eventHandlers.Handle(message);
        }
    }
}
