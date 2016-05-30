using System.Linq;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player.Events;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using WinService.Workers;

namespace AFT.RegoV2.Tests.Unit.Player
{
    internal class ActivationMessageTests : PlayerServiceTestsBase
    {
        private PlayerCommands _playerCommands;
        private EmailNotificationWorker _emailNotificationWorker;
        private SmsNotificationWorker _smsNotificationWorker;

        public override void BeforeEach()
        {
            base.BeforeEach();
            
            _playerCommands = Container.Resolve<PlayerCommands>();
            _emailNotificationWorker = Container.Resolve<EmailNotificationWorker>();
            _smsNotificationWorker = Container.Resolve<SmsNotificationWorker>();
        }

        [Test]
        public async void Can_send_activation_email()
        {
            _emailNotificationWorker.Start();

            FakeBrandRepository.Brands.First().PlayerActivationMethod = PlayerActivationMethod.Email;
            FakeBrandRepository.SaveChanges();

            await RegisterPlayer(false);
            var events = FakeEventRepository.GetEvents<NotificationSentEvent>().ToArray();

            Assert.That(events.Length, Is.EqualTo(1));
            Assert.That(events.First().Type, Is.EqualTo(NotificationType.Email));
        }

        [Test]
        public async void Can_send_activation_sms()
        {
            _smsNotificationWorker.Start();

            FakeBrandRepository.Brands.First().PlayerActivationMethod = PlayerActivationMethod.Sms;
            FakeBrandRepository.SaveChanges();

            await RegisterPlayer(false);
            var events = FakeEventRepository.GetEvents<NotificationSentEvent>().ToArray();

            Assert.That(events.Length, Is.EqualTo(1));
            Assert.That(events.First().Type, Is.EqualTo(NotificationType.Sms));
        }

        [Test]
        public async void Can_send_activation_email_and_sms()
        {
            _emailNotificationWorker.Start();
            _smsNotificationWorker.Start();

            FakeBrandRepository.Brands.First().PlayerActivationMethod = PlayerActivationMethod.EmailOrSms;
            FakeBrandRepository.SaveChanges();

            await RegisterPlayer(false);
            var notificationSentEvents = FakeEventRepository.GetEvents<NotificationSentEvent>().ToArray();

            Assert.That(notificationSentEvents.Length, Is.EqualTo(2));
            Assert.That(notificationSentEvents.Count(x => x.Type == NotificationType.Email), Is.EqualTo(1));
            Assert.That(notificationSentEvents.Count(x => x.Type == NotificationType.Sms), Is.EqualTo(1));
        }

        [Test]
        public async void Can_resend_activation_email()
        {
            _emailNotificationWorker.Start();

            FakeBrandRepository.Brands.First().PlayerActivationMethod = PlayerActivationMethod.Email;
            FakeBrandRepository.SaveChanges();

            await RegisterPlayer(false);
            var playerId = FakePlayerRepository.Players.First().Id;
            _playerCommands.ResendActivationEmail(playerId);

            var events = FakeEventRepository.GetEvents<NotificationSentEvent>().ToArray();

            Assert.That(events.Length, Is.EqualTo(2));
        }

        [Test]
        public async void Can_send_mobile_verification_sms()
        {
            await RegisterPlayer(false);
            var playerId = FakePlayerRepository.Players.First().Id;
            _playerCommands.SendMobileVerificationCode(playerId);

            var events = FakeEventRepository.GetEvents<MobileVerificationCodeSentSms>().ToArray();

            Assert.That(events.Length, Is.EqualTo(1));
            Assert.That(events.First().PlayerId, Is.EqualTo(playerId));
        }
    }
}