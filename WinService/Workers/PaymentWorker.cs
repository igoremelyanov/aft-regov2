using AFT.RegoV2.Core.Payment.Interface.Commands;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.WinService.Workers;

using Microsoft.Practices.Unity;

using PaymentEventHandlers = AFT.RegoV2.Core.Payment.ApplicationServices.PaymentSubscriber;

namespace WinService.Workers
{
    public class PaymentWorker : WorkerBase<PaymentSubscriber>
    {
        public PaymentWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class PaymentSubscriber : IBusSubscriber,
        IConsumes<Deposit>,
        IConsumes<DepositApproved>,
        IConsumes<WithdrawRequestSubmit>,
        IConsumes<WithdrawRequestCancel>,
        IConsumes<WithdrawRequestApprove>
    {
        private readonly PaymentEventHandlers _paymentSubscriber;

        public PaymentSubscriber(PaymentEventHandlers paymentSubscriber)
        {
            _paymentSubscriber = paymentSubscriber;
        }

        public void Consume(Deposit message)
        {
            _paymentSubscriber.Consume(message);
        }

        public void Consume(WithdrawRequestSubmit message)
        {
            _paymentSubscriber.Consume(message);
        }

        public void Consume(WithdrawRequestCancel message)
        {
            _paymentSubscriber.Consume(message);
        }

        public void Consume(WithdrawRequestApprove message)
        {
            _paymentSubscriber.Consume(message);
        }

        public void Consume(DepositApproved message)
        {
            _paymentSubscriber.Consume(message);
        }
    }
}