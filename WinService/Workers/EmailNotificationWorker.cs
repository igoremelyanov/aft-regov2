using AFT.RegoV2.Core.Common.Commands;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;

namespace WinService.Workers
{
    public class EmailNotificationWorker : WorkerBase<EmailNotificationSubscriber>
    {
        public EmailNotificationWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class EmailNotificationSubscriber : IBusSubscriber,
        IConsumes<EmailCommandMessage>
    {
        private readonly IEmailNotifier _emailNotifier;
        private readonly IEventBus _eventBus;

        public EmailNotificationSubscriber(IEmailNotifier emailNotifier, IEventBus eventBus)
        {
            _emailNotifier = emailNotifier;
            _eventBus = eventBus;
        }

        public void Consume(EmailCommandMessage message)
        {
            var notificationSentEvent = _emailNotifier.SendEmail(
                message.SenderEmail,
                message.SenderName,
                message.RecipientEmail,
                message.RecipientName,
                message.Subject,
                message.Body);

            _eventBus.Publish(notificationSentEvent);
        }
    }
}