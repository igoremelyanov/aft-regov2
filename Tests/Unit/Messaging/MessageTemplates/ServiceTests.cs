using System.Linq;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using WinService.Workers;

namespace AFT.RegoV2.Tests.Unit.Messaging.MessageTemplates
{
    class ServiceTests : MessagingTestsBase
    {
        private IEventRepository _eventRepository;
        private EmailNotificationWorker _emailNotificationWorker;
        private SmsNotificationWorker _smsNotificationWorker;
        private PlayerTestHelper _playerTestHelper;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _eventRepository = Container.Resolve<IEventRepository>();
            _emailNotificationWorker = Container.Resolve<EmailNotificationWorker>();
            _smsNotificationWorker = Container.Resolve<SmsNotificationWorker>();
            _playerTestHelper = Container.Resolve<PlayerTestHelper>();
        }

        [TestCase(MessageDeliveryMethod.Email)]
        [TestCase(MessageDeliveryMethod.Sms)]
        public void Can_send_player_message(MessageDeliveryMethod messageDeliveryMethod)
        {
            var player = SetUpPlayerMessageTest(messageDeliveryMethod);

            MessageTemplateService.TrySendPlayerMessage(
                player.Id,
                MessageType.PlayerRegistered, 
                messageDeliveryMethod,
                new PlayerRegisteredModel());

            var events = _eventRepository.GetEvents<NotificationSentEvent>().ToArray();

            Assert.That(events.Length, Is.EqualTo(1));

            var notificationType = messageDeliveryMethod == MessageDeliveryMethod.Email
                ? NotificationType.Email
                : NotificationType.Sms;

            Assert.That(events.First().Type, Is.EqualTo(notificationType));
            Assert.That(events.First().Status, Is.EqualTo(NotificationStatus.Send));
        }

        [TestCase(MessageDeliveryMethod.Email)]
        [TestCase(MessageDeliveryMethod.Sms)]
        public void Can_send_player_message_ignoring_player_account_setting(MessageDeliveryMethod messageDeliveryMethod)
        {
            var player = SetUpPlayerMessageTest(messageDeliveryMethod, false);

            MessageTemplateService.TrySendPlayerMessage(
                player.Id,
                MessageType.PlayerRegistered,
                messageDeliveryMethod,
                new PlayerRegisteredModel(),
                true);

            var events = _eventRepository.GetEvents<NotificationSentEvent>().ToArray();

            Assert.That(events.Length, Is.EqualTo(1));

            var notificationType = messageDeliveryMethod == MessageDeliveryMethod.Email
                ? NotificationType.Email
                : NotificationType.Sms;

            Assert.That(events.First().Type, Is.EqualTo(notificationType));
            Assert.That(events.First().Status, Is.EqualTo(NotificationStatus.Send));
        }

        [TestCase(MessageDeliveryMethod.Email)]
        [TestCase(MessageDeliveryMethod.Sms)]
        public void Can_fail_sending_player_message_because_player_account_setting(MessageDeliveryMethod messageDeliveryMethod)
        {
            var player = SetUpPlayerMessageTest(messageDeliveryMethod, false);

            MessageTemplateService.TrySendPlayerMessage(
                player.Id,
                MessageType.PlayerRegistered,
                messageDeliveryMethod,
                new PlayerRegisteredModel());

            var events = _eventRepository.GetEvents<NotificationSentEvent>().ToArray();

            Assert.That(events, Is.Empty);
        }

        [TestCase(MessageDeliveryMethod.Email)]
        [TestCase(MessageDeliveryMethod.Sms)]
        public void Can_send_brand_message(MessageDeliveryMethod messageDeliveryMethod)
        {
            if (messageDeliveryMethod == MessageDeliveryMethod.Email)
                _emailNotificationWorker.Start();
            else
                _smsNotificationWorker.Start();

            if (messageDeliveryMethod == MessageDeliveryMethod.Email)
                MessageTemplateService.TrySendBrandEmail(
                    TestDataGenerator.GetRandomString(),
                    TestDataGenerator.GetRandomEmail(),
                    Brand.Id,
                    MessageType.ReferFriends,
                    new ReferFriendsModel());
            else
                MessageTemplateService.TrySendBrandSms(
                    TestDataGenerator.GetRandomString(),
                    TestDataGenerator.GetRandomPhoneNumber(),
                    Brand.Id,
                    MessageType.ReferFriends,
                    new ReferFriendsModel());

            var events = _eventRepository.GetEvents<NotificationSentEvent>().ToArray();

            Assert.That(events.Length, Is.EqualTo(1));

            var notificationType = messageDeliveryMethod == MessageDeliveryMethod.Email
                ? NotificationType.Email
                : NotificationType.Sms;

            Assert.That(events.First().Type, Is.EqualTo(notificationType));
            Assert.That(events.First().Status, Is.EqualTo(NotificationStatus.Send));
        }

        [Test]
        public void Can_parse_message()
        {
            var player = SetUpPlayerMessageTest(MessageDeliveryMethod.Sms);

            MessageTemplateService.TrySendPlayerMessage(
                player.Id,
                MessageType.PlayerRegistered,
                MessageDeliveryMethod.Sms, 
                new PlayerRegisteredModel());

            var events = _eventRepository.GetEvents<NotificationSentEvent>().ToArray();

            Assert.That(events.Length, Is.EqualTo(1));

            var notificationSentEvent = events.First();

            Assert.That(notificationSentEvent.Message, Contains.Substring(player.Username));
            Assert.That(notificationSentEvent.Status, Is.EqualTo(NotificationStatus.Send));
        }

        private Core.Messaging.Data.Player SetUpPlayerMessageTest(MessageDeliveryMethod messageDeliveryMethod, bool enableAccountAlert = true)
        {
            var player = _playerTestHelper.CreatePlayer();

            var messagingPlayer = MessagingRepository.Players.Single(x => x.Id == player.Id);

            if (messageDeliveryMethod == MessageDeliveryMethod.Email)
            {
                _emailNotificationWorker.Start();
                messagingPlayer.AccountAlertEmail = enableAccountAlert;
            }
            else
            {
                _smsNotificationWorker.Start();
                messagingPlayer.AccountAlertSms = enableAccountAlert;
            }

            return messagingPlayer;
        }
    }
}