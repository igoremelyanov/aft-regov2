using System;
using System.Linq;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateCommands;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Messaging.MessageTemplates
{
    class ValidationTests : MessagingTestsBase
    {
        [TestCase(MessageDeliveryMethod.Email)]
        [TestCase(MessageDeliveryMethod.Sms)]
        public void Can_validate_add(MessageDeliveryMethod messageDeliveryMethod)
        {
            var data = MessagingTestHelper.CreateAddMessageTemplateData(messageDeliveryMethod: messageDeliveryMethod);

            var result = MessageTemplateQueries.GetValidationResult(data);

            Assert.That(result.Errors.Count, Is.EqualTo(0));
        }

        [TestCase(MessageDeliveryMethod.Email)]
        [TestCase(MessageDeliveryMethod.Sms)]
        public void Can_validate_edit(MessageDeliveryMethod messageDeliveryMethod)
        {
            var data = MessagingTestHelper.CreateEditMessageTemplateData(messageDeliveryMethod);

            var result = MessageTemplateQueries.GetValidationResult(data);

            Assert.That(result.Errors.Count, Is.EqualTo(0));
        }

        [Test]
        public void Can_fail_missing_required_fields_email()
        {
            var data = new AddMessageTemplate
            {
                BrandId = Brand.Id,
                MessageType = TestDataGenerator.GetRandomMessageType(),
                MessageDeliveryMethod = MessageDeliveryMethod.Email,
            };

            var result = MessageTemplateQueries.GetValidationResult(data);

            Assert.That(result.Errors.Count, Is.EqualTo(4));
            Assert.That(result.Errors.All(x => x.ErrorMessage == Enum.GetName(typeof(MessagingValidationError), MessagingValidationError.Required)));
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "LanguageCode"), Is.Not.Null);
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "TemplateName"), Is.Not.Null);
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "Subject"), Is.Not.Null);
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "MessageContent"), Is.Not.Null);
        }

        [Test]
        public void Can_fail_missing_required_fields_sms()
        {
            var data = new AddMessageTemplate
            {
                BrandId = Brand.Id,
                MessageType = TestDataGenerator.GetRandomMessageType(),
                MessageDeliveryMethod = MessageDeliveryMethod.Sms,
            };

            var result = MessageTemplateQueries.GetValidationResult(data);

            Assert.That(result.Errors.Count, Is.EqualTo(3));
            
            Assert.That(result.Errors.All(x => x.ErrorMessage ==
                Enum.GetName(typeof(MessagingValidationError), MessagingValidationError.Required)));

            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "LanguageCode"), Is.Not.Null);
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "TemplateName"), Is.Not.Null);
            Assert.That(result.Errors.SingleOrDefault(x => x.PropertyName == "MessageContent"), Is.Not.Null);
        }

        [Test]
        public void Can_fail_invalid_id()
        {
            var data = MessagingTestHelper.CreateEditMessageTemplateData();
            data.Id = Guid.NewGuid();

            var result = MessageTemplateQueries.GetValidationResult(data);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First().ErrorMessage, Is.EqualTo(
                Enum.GetName(typeof(MessagingValidationError), MessagingValidationError.InvalidId)));
        }

        [Test]
        public void Can_fail_invalid_brand()
        {
            var data = MessagingTestHelper.CreateAddMessageTemplateData();
            data.BrandId = Guid.Empty;

            var result = MessageTemplateQueries.GetValidationResult(data);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First().ErrorMessage, Is.EqualTo(
                Enum.GetName(typeof(MessagingValidationError), MessagingValidationError.InvalidBrand)));
        }

        [Test]
        public void Can_fail_invalid_language()
        {
            var data = MessagingTestHelper.CreateAddMessageTemplateData();
            data.LanguageCode = TestDataGenerator.GetRandomString(5);

            var result = MessageTemplateQueries.GetValidationResult(data);

            Assert.That(result.Errors.Count, Is.EqualTo(1));

            Assert.That(result.Errors.First().ErrorMessage, Is.EqualTo(
                Enum.GetName(typeof(MessagingValidationError), MessagingValidationError.InvalidLanguage)));
        }

        [Test]
        public void Can_fail_invalid_message_type()
        {
            var data = MessagingTestHelper.CreateAddMessageTemplateData();
            data.MessageType = (MessageType)Enum.GetValues(typeof(MessageType)).Length;

            var result = MessageTemplateQueries.GetValidationResult(data);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First().ErrorMessage, Is.EqualTo(
                Enum.GetName(typeof(MessagingValidationError), MessagingValidationError.InvalidMessageType)));
        }

        [Test]
        public void Can_fail_invalid_delivery_type()
        {
            var data = MessagingTestHelper.CreateAddMessageTemplateData();
            data.MessageDeliveryMethod = (MessageDeliveryMethod) Enum.GetValues(typeof (MessageDeliveryMethod)).Length;

            var result = MessageTemplateQueries.GetValidationResult(data);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First().ErrorMessage, Is.EqualTo(
                Enum.GetName(typeof(MessagingValidationError), MessagingValidationError.InvalidMessageDeliveryMethod)));
        }

        [Test]
        public void Can_fail_template_name_in_use()
        {
            var messageTemplate = MessagingRepository.MessageTemplates.First();

            var data = MessagingTestHelper.CreateAddMessageTemplateData(
                messageTemplate.BrandId,
                messageTemplate.LanguageCode,
                messageTemplate.MessageType,
                messageTemplate.MessageDeliveryMethod);

            data.TemplateName = messageTemplate.TemplateName;

            var result = MessageTemplateQueries.GetValidationResult(data);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First().ErrorMessage, Is.EqualTo(
                Enum.GetName(typeof(MessagingValidationError), MessagingValidationError.TemplateNameInUse)));
        }

        [Test]
        public void Can_fail_message_compile_error()
        {
            const string template = @"{% block content -%}<h1>Hello</h1>";

            var data = MessagingTestHelper.CreateAddMessageTemplateData(content: template);

            var result = MessageTemplateQueries.GetValidationResult(data);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First().ErrorMessage, Is.EqualTo(
                Enum.GetName(typeof(MessagingValidationError), MessagingValidationError.InvalidMessageContent)));
        }

        [Test]
        public void Can_fail_subject_compile_error()
        {
            const string subject = @"{% block subject -%}Hi";

            var data = MessagingTestHelper.CreateAddMessageTemplateData(
                messageDeliveryMethod: MessageDeliveryMethod.Email, 
                subject: subject);

            var result = MessageTemplateQueries.GetValidationResult(data);

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors.First().ErrorMessage, Is.EqualTo(
                Enum.GetName(typeof(MessagingValidationError), MessagingValidationError.InvalidSubject)));
        }
    }
}