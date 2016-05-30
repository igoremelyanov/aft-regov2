using AFT.RegoV2.Core.Common.Commands;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;

namespace WinService.Workers
{
    public class SmsNotificationWorker : WorkerBase<SmsNotificationSubscriber>
    {
        public SmsNotificationWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) {}
    }
    
    public class SmsNotificationSubscriber : IBusSubscriber, 
        IConsumes<SmsCommandMessage>
    {
        private readonly ISmsNotifier _smsNotifier;
        private readonly IEventBus _eventBus;

        public SmsNotificationSubscriber(ISmsNotifier smsNotifier, IEventBus eventBus)
        {
            _smsNotifier = smsNotifier;
            _eventBus = eventBus;
        }

        public void Consume(SmsCommandMessage message)
        {
            var notificationSentEvent = _smsNotifier.SendSms(message.From, message.To, message.Body);
            _eventBus.Publish(notificationSentEvent);
        }
    }
}