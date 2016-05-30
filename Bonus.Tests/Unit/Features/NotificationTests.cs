using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Tests.Base;
using AFT.RegoV2.Core.Messaging.Interface.Commands;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Tests.Common.TestDoubles;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Bonus.Tests.Unit.Features
{
    internal class NotificationTests : UnitTestBase
    {
        private FakeServiceBus _bus;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _bus = (FakeServiceBus)ServiceBus;
        }

        [Test]
        public void Email_is_sent_upon_bonus_activation()
        {
            var bonus = CreateFirstDepositBonus();

            bonus.Template.Notification.Triggers.Add(new NotificationMessageType
            {
                MessageType = MessageType.BonusIssued,
                TriggerType = TriggerType.Email
            });

            MakeDeposit(PlayerId);

            var command = (SendPlayerAMessage)_bus.PublishedCommands.Last();

            command.MessageType.Should().Be(MessageType.BonusIssued);
            command.MessageDeliveryMethod.Should().Be(MessageDeliveryMethod.Email);
        }

        [Test]
        public void Sms_is_sent_upon_bonus_activation()
        {
            var bonus = CreateFirstDepositBonus();

            bonus.Template.Notification.Triggers.Add(new NotificationMessageType
            {
                MessageType = MessageType.BonusIssued,
                TriggerType = TriggerType.Sms
            });

            MakeDeposit(PlayerId);

            var command = (SendPlayerAMessage)_bus.PublishedCommands.Last();

            command.MessageType.Should().Be(MessageType.BonusIssued);
            command.MessageDeliveryMethod.Should().Be(MessageDeliveryMethod.Sms);
        }
    }
}