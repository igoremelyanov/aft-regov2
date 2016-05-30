using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using AFT.RegoV2.Core.Common.Commands;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels;
using AFT.RegoV2.RegoBus.Interfaces;
using AutoMapper;
using DotLiquid;
using DotLiquid.NamingConventions;
using Player = AFT.RegoV2.Core.Messaging.Data.Player;

namespace AFT.RegoV2.Core.Messaging.ApplicationServices
{
    public class MessageTemplateService : IMessageTemplateService
    {
        private readonly IMessagingRepository _repository;
        private readonly IServiceBus _serviceBus;

        static MessageTemplateService()
        {
            Mapper.CreateMap<Player, IPlayerMessageTemplateModel>()
                .ForMember(dest => dest.BrandWebsite, opt => opt.MapFrom(src => src.Brand.WebsiteUrl));

            Template.NamingConvention = new CSharpNamingConvention();

            RegisterMessageTypes();
        }

        public MessageTemplateService(IMessagingRepository repository, IServiceBus serviceBus)
        {
            _repository = repository;
            _serviceBus = serviceBus;
        }

        public void TrySendPlayerMessage(
            Guid playerId,
            MessageType messageType,
            MessageDeliveryMethod messageDeliveryMethod,
            IPlayerMessageTemplateModel model,
            bool ignoreAccountAlertSetting = false)
        {
            var player = _repository.Players
                .Include(x => x.Brand)
                .Include(x => x.Language)
                .Single(x => x.Id == playerId);

            var accountAlertEnabled = messageDeliveryMethod == MessageDeliveryMethod.Email
                ? player.AccountAlertEmail
                : player.AccountAlertSms;

            if (!accountAlertEnabled && !ignoreAccountAlertSetting)
                return;

            var messageTemplate = _repository.MessageTemplates.SingleOrDefault(x =>
                x.BrandId == player.Brand.Id &&
                x.LanguageCode == player.Language.Code &&
                x.MessageType == messageType &&
                x.MessageDeliveryMethod == messageDeliveryMethod &&
                x.Status == Status.Active);

            if (messageTemplate == null)
                return;

            model = Mapper.Map(player, model);

            var parsedMessage = ParseTemplate(messageTemplate.MessageContent, model);

            if (messageDeliveryMethod == MessageDeliveryMethod.Email)
            {
                SendEmail(
                    senderEmail: player.Brand.Email,
                    senderName: player.Brand.Name,
                    recipientEmail: player.Email,
                    recipientName: string.Format("{0} {1}", player.FirstName, player.LastName),
                    subject: ParseTemplate(messageTemplate.Subject, model),
                    body: parsedMessage);
            }
            else
            {
                SendSms(player.Brand.SmsNumber, player.PhoneNumber, parsedMessage);
            }
        }

        public void TrySendBrandEmail(
            string recipientName,
            string recipientEmail,
            Guid brandId,
            MessageType messageType,
            IBrandMessageTemplateModel model,
            string languageCode = null)
        {
            TrySendBrandMessage(
                recipientName,
                recipientEmail,
                languageCode,
                brandId,
                messageType,
                MessageDeliveryMethod.Email,
                model);
        }

        public void TrySendBrandSms(
            string recipientName,
            string recipientNumber,
            Guid brandId,
            MessageType messageType,
            IBrandMessageTemplateModel model,
            string languageCode = null)
        {
            TrySendBrandMessage(
                recipientName,
                recipientNumber,
                languageCode,
                brandId,
                messageType,
                MessageDeliveryMethod.Sms,
                model);
        }

        string IMessageTemplateService.ParseTemplate(string content, IMessageTemplateModel model)
        {
            return ParseTemplate(content, model);
        }

        private void TrySendBrandMessage(
            string recipientName,
            string recipientContact,
            string languageCode,
            Guid brandId,
            MessageType messageType,
            MessageDeliveryMethod messageDeliveryMethod,
            IBrandMessageTemplateModel model)
        {
            var brand = _repository.Brands.Single(x => x.Id == brandId);

            var messageTemplate = _repository.MessageTemplates.SingleOrDefault(x =>
                x.BrandId == brandId &&
                x.LanguageCode == (languageCode ?? brand.DefaultLanguageCode) &&
                x.MessageType == messageType &&
                x.MessageDeliveryMethod == messageDeliveryMethod &&
                x.Status == Status.Active);

            if (messageTemplate == null)
                return;

            model.BrandName = brand.Name;
            model.RecipientName = recipientName;

            var parsedMessage = ParseTemplate(messageTemplate.MessageContent, model);

            if (messageDeliveryMethod == MessageDeliveryMethod.Email)
            {
                SendEmail(
                    senderEmail: brand.Email,
                    senderName: brand.Name,
                    recipientEmail: recipientContact,
                    recipientName: recipientName,
                    subject: ParseTemplate(messageTemplate.Subject, model),
                    body: parsedMessage);
            }
            else
            {
                SendSms(brand.SmsNumber, recipientContact, parsedMessage);
            }
        }

        private static string ParseTemplate(string content, IMessageTemplateModel model)
        {
            var parsedTemplate = Template.Parse(content);
            var parsedMessage = parsedTemplate.Render(Hash.FromAnonymousObject(new {model}));

            return parsedMessage;
        }

        private void SendEmail(
            string senderEmail,
            string senderName,
            string recipientEmail,
            string recipientName,
            string subject,
            string body)
        {
            var emailCommandMessage = new EmailCommandMessage(
                senderEmail,
                senderName,
                recipientEmail,
                recipientName,
                subject,
                body);

            _serviceBus.PublishMessage(emailCommandMessage);
        }

        private void SendSms(string from, string to, string body)
        {
            var smsCommandMessage = new SmsCommandMessage(from, to, body);

            _serviceBus.PublishMessage(smsCommandMessage);
        }

        private static void RegisterMessageTypes()
        {
            var messageTypes = Assembly.GetAssembly(typeof(IMessageTemplateModel))
                .GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && x.GetInterfaces().Contains(typeof(IMessageTemplateModel)))
                .ToArray();

            foreach (var messageType in messageTypes)
            {
                Template.RegisterSafeType(messageType, messageType.GetProperties().Select(x => x.Name).ToArray());
            }
        }
    }
}