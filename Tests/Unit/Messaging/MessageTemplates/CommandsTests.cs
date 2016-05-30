using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateCommands;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Messaging.MessageTemplates
{
    class CommandsTests : MessagingTestsBase
    {
        [Test]
        public void Can_add_message_template()
        {
            var data = MessagingTestHelper.CreateAddMessageTemplateData();

            var id = MessageTemplateCommands.Add(data);

            var messageTemplate = MessagingRepository.MessageTemplates.SingleOrDefault(x => x.Id == id);

            Assert.That(messageTemplate, Is.Not.Null);
        }

        [Test]
        public void Can_fail_add_message_template()
        {
            var data = MessagingTestHelper.CreateAddMessageTemplateData();
            data.BrandId = Guid.NewGuid();

            var exception = Assert.Throws<RegoValidationException>(() => MessageTemplateCommands.Add(data));

            Assert.That(exception.Message, Is.EqualTo(
                Enum.GetName(typeof(MessagingValidationError), MessagingValidationError.InvalidBrand)));
        }

        [Test]
        public void Can_edit_message_template()
        {
            var data = MessagingTestHelper.CreateEditMessageTemplateData();

            MessageTemplateCommands.Edit(data);

            var messageTemplate = MessagingRepository.MessageTemplates.SingleOrDefault(x => x.Id == data.Id);

            Assert.That(messageTemplate, Is.Not.Null);
            Assert.That(messageTemplate.Id, Is.EqualTo(data.Id));
            Assert.That(messageTemplate.TemplateName, Is.EqualTo(data.TemplateName));
            Assert.That(messageTemplate.Subject, Is.EqualTo(data.Subject));
            Assert.That(messageTemplate.MessageContent, Is.EqualTo(data.MessageContent));
        }

        [Test]
        public void Can_fail_edit_message_template()
        {
            var data = MessagingTestHelper.CreateEditMessageTemplateData();
            data.Id = Guid.NewGuid();

            var exception = Assert.Throws<RegoValidationException>(() => MessageTemplateCommands.Edit(data));

            Assert.That(exception.Message, Is.EqualTo(
                Enum.GetName(typeof(MessagingValidationError), MessagingValidationError.InvalidId)));
        }

        [Test]
        public void Can_activate_message_template()
        {
            var addData = MessagingTestHelper.CreateAddMessageTemplateData();
            
            var id = MessageTemplateCommands.Add(addData);

            var activateData = new ActivateMessageTemplate
            {
                Id = id,
                Remarks = TestDataGenerator.GetRandomString()
            };

            MessageTemplateCommands.Activate(activateData);

            var messageTemplate = MessagingRepository.MessageTemplates.Single(x => x.Id == id);

            Assert.That(messageTemplate.Status, Is.EqualTo(Status.Active));
        }

        [Test]
        public void Can_fail_activate_message_template()
        {
            var data = new ActivateMessageTemplate
            {
                Id = MessagingRepository.MessageTemplates.First(x => x.Status == Status.Active).Id,
                Remarks = TestDataGenerator.GetRandomString()
            };

            var exception = Assert.Throws<RegoValidationException>(() => MessageTemplateCommands.Activate(data));

            Assert.That(exception.Message, Is.EqualTo(
                Enum.GetName(typeof(MessagingValidationError), MessagingValidationError.AlreadyActive)));
        }
    }
}