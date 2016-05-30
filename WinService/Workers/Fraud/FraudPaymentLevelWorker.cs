using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.RegoBus.Interfaces;

namespace WinService.Workers.Fraud
{
    public class FraudPaymentLevelWorker : WorkerBase<FraudPaymentLevelSubscriber>
    {
        public FraudPaymentLevelWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class FraudPaymentLevelSubscriber : IBusSubscriber,
        IConsumes<PaymentLevelAdded>,
        IConsumes<PaymentLevelActivated>,
        IConsumes<PaymentLevelDeactivated>,
        IConsumes<PaymentLevelEdited>
    {
        private readonly FraudSubdomainSubscriber _eventHandlers;

        public FraudPaymentLevelSubscriber(FraudSubdomainSubscriber eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public void Consume(PaymentLevelAdded message)
        {
            _eventHandlers.Consume(message);
        }

        public void Consume(PaymentLevelActivated message)
        {
            _eventHandlers.Consume(message);
        }

        public void Consume(PaymentLevelDeactivated message)
        {
            _eventHandlers.Consume(message);
        }

        public void Consume(PaymentLevelEdited message)
        {
            _eventHandlers.Consume(message);
        }
    }
}
