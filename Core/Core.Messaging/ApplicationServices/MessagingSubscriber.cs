using System;
using System.Collections;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Messaging.Data;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Commands;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels;
using AFT.RegoV2.Core.Messaging.Interface.Events;
using AFT.RegoV2.Core.Messaging.Resources;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;
using AutoMapper;
using Newtonsoft.Json;
using Language = AFT.RegoV2.Core.Messaging.Data.Language;
using MessageTemplate = AFT.RegoV2.Core.Messaging.Data.MessageTemplate;
using Player = AFT.RegoV2.Core.Messaging.Data.Player;

namespace AFT.RegoV2.Core.Messaging.ApplicationServices
{
    public class MessagingSubscriber :
        IConsumes<LanguageCreated>,
        IConsumes<LanguageUpdated>,
        IConsumes<BrandRegistered>,
        IConsumes<BrandUpdated>,
        IConsumes<BrandLanguagesAssigned>,
        IConsumes<PaymentLevelAdded>,
        IConsumes<PaymentLevelEdited>,
        IConsumes<VipLevelRegistered>,
        IConsumes<VipLevelUpdated>,
        IConsumes<PlayerRegistered>,
        IConsumes<PlayerUpdated>,
        IConsumes<PlayerVipLevelChanged>,
        IConsumes<PlayerActivated>,
        IConsumes<PlayerDeactivated>,
        IConsumes<MassMessageSendRequestedEvent>,
        IConsumes<SendBrandSms>,
        IConsumes<SendPlayerAMessage>
    {
        private readonly IMessagingRepository _repository;
        private readonly IMessageTemplateService _service;
        private readonly IEventBus _eventBus;

        static MessagingSubscriber()
        {
            Mapper.CreateMap<BonusIssuedModel, BonusIssuedFormattedModel>()
                .ForMember(bi => bi.Amount, opt => opt.MapFrom(model => model.Amount.Format()));
            Mapper.CreateMap<HighDepositReminderModel, HighDepositReminderFormattedModel>()
                .ForMember(bi => bi.BonusAmount, opt => opt.MapFrom(model => model.BonusAmount.Format()))
                .ForMember(bi => bi.RemainingAmount, opt => opt.MapFrom(model => model.RemainingAmount.Format()))
                .ForMember(bi => bi.DepositAmountRequired, opt => opt.MapFrom(model => model.DepositAmountRequired.Format()));
            Mapper.CreateMap<BonusWageringRequirementModel, BonusWageringRequirementFormattedModel>()
                .ForMember(bi => bi.BonusAmount, opt => opt.MapFrom(model => model.BonusAmount.Format()))
                .ForMember(bi => bi.RequiredWagerAmount, opt => opt.MapFrom(model => model.RequiredWagerAmount.Format()));
        }

        public MessagingSubscriber(IMessagingRepository repository, IMessageTemplateService service, IEventBus eventBus)
        {
            _repository = repository;
            _service = service;
            _eventBus = eventBus;
        }

        public void Consume(LanguageCreated message)
        {
            _repository.Languages.Add(new Language
            {
                Code = message.Code,
                Name = message.Name
            });

            _repository.SaveChanges();
        }

        public void Consume(LanguageUpdated message)
        {
            var culture = _repository.Languages.Single(x => x.Code == message.Code);

            culture.Name = message.Name;

            _repository.SaveChanges();
        }

        public void Consume(BrandRegistered message)
        {
            _repository.Brands.Add(new Data.Brand
            {
                Id = message.Id,
                Name = message.Name,
                Email = message.Email,
                SmsNumber = message.SmsNumber,
                WebsiteUrl = message.WebsiteUrl,
                TimezoneId = message.TimeZoneId
            });

            _repository.SaveChanges();
        }

        public void Consume(BrandUpdated message)
        {
            var brand = _repository.Brands.Single(x => x.Id == message.Id);

            brand.Name = message.Name;
            brand.Email = message.Email;
            brand.SmsNumber = message.SmsNumber;
            brand.WebsiteUrl = message.WebsiteUrl;
            brand.TimezoneId = message.TimeZoneId;

            _repository.SaveChanges();
        }

        public void Consume(BrandLanguagesAssigned message)
        {
            var brand = _repository.Brands
                .Include(x => x.Languages)
                .Single(x => x.Id == message.BrandId);

            RemoveBrandLanguagesAndMessageTemplates(brand, message);

            AddBrandLanguagesAndMessageTemplates(brand, message);

            _repository.SaveChanges();
        }

        public void Consume(PaymentLevelAdded message)
        {
            _repository.PaymentLevels.Add(new PaymentLevel
            {
                Id = message.Id,
                Name = message.Name
            });
            _repository.SaveChanges();
        }

        public void Consume(PaymentLevelEdited message)
        {
            _repository.PaymentLevels.Single(x => x.Id == message.Id).Name = message.Name;
            _repository.SaveChanges();
        }

        public void Consume(VipLevelRegistered message)
        {
            _repository.VipLevels.Add(new VipLevel
            {
                Id = message.Id,
                Name = message.Name
            });
            _repository.SaveChanges();
        }

        public void Consume(VipLevelUpdated message)
        {
            _repository.VipLevels.Single(x => x.Id == message.Id).Name = message.Name;
            _repository.SaveChanges();
        }

        public void Consume(PlayerRegistered message)
        {
            _repository.Players.Add(new Player
            {
                Id = message.PlayerId,
                Username = message.UserName,
                FirstName = message.FirstName,
                LastName = message.LastName,
                Email = message.Email,
                PhoneNumber = message.PhoneNumber,
                LanguageCode = message.CultureCode,
                Language = _repository.Languages.Single(x => x.Code == message.CultureCode),
                BrandId = message.BrandId,
                Brand = _repository.Brands.Single(x => x.Id == message.BrandId),
                VipLevelId = message.VipLevelId,
                VipLevel = _repository.VipLevels.Single(x => x.Id == message.VipLevelId),
                PaymentLevelId = message.PaymentLevelId,
                PaymentLevel = message.PaymentLevelId.HasValue
                    ? _repository.PaymentLevels.SingleOrDefault(x => x.Id == message.PaymentLevelId)
                    : null,
                AccountAlertEmail = message.AccountAlertEmail,
                AccountAlertSms = message.AccountAlertSms,
                IsActive = message.IsActive,
                DateRegistered = message.DateRegistered
            });

            _repository.SaveChanges();
        }

        public void Consume(PlayerUpdated message)
        {
            var player = _repository.Players.Single(x => x.Id == message.Id);

            player.Email = message.Email;
            player.PhoneNumber = message.PhoneNumber;
            player.VipLevelId = message.VipLevelId;
            player.AccountAlertEmail = message.AccountAlertEmail;
            player.AccountAlertSms = message.AccountAlertSms;

            _repository.SaveChanges();
        }

        public void Consume(PlayerActivated message)
        {
            var player = _repository.Players.Single(x => x.Id == message.PlayerId);
            player.IsActive = true;
            _repository.SaveChanges();
        }

        public void Consume(PlayerDeactivated message)
        {
            var player = _repository.Players.Single(x => x.Id == message.PlayerId);
            player.IsActive = false;
            _repository.SaveChanges();
        }

        public void Consume(MassMessageSendRequestedEvent message)
        {
            var massMessage = _repository.MassMessages
                .Include(x => x.Recipients.Select(y => y.Language))
                .Include(x => x.Recipients.Select(y => y.Brand))
                .Single(x => x.Id == message.Request.Id);

            var brand = massMessage.Recipients.First().Brand;

            var model = new MassMessageTemplateModel
            {
                BrandName = brand.Name,
                BrandWebsite = brand.WebsiteUrl
            };

            foreach (var recipient in massMessage.Recipients)
            {
                model.FirstName = recipient.FirstName;
                model.LastName = recipient.LastName;
                model.Username = recipient.Username;

                var recipientContent = message.Request.Content
                    .SingleOrDefault(x => x.LanguageCode == recipient.LanguageCode);

                if (recipientContent != null)
                {
                    var onSiteMessageEvent = new OnSiteMessageSentEvent(
                        massMessage.Id,
                        recipient.Id,
                        _service.ParseTemplate(recipientContent.OnSiteSubject, model),
                        _service.ParseTemplate(recipientContent.OnSiteContent, model));

                    _eventBus.Publish(onSiteMessageEvent);
                }
            }

            massMessage.DateSent = DateTimeOffset.UtcNow;

            _repository.SaveChanges();
        }

        private void RemoveBrandLanguagesAndMessageTemplates(Data.Brand brand, BrandLanguagesAssigned message)
        {
            var oldLanguages = brand.Languages.Where(x => message.Languages.All(y => y.Code != x.Code)).ToArray();

            oldLanguages.ForEach(oldLanguage =>
            {
                var messageTemplates = _repository.MessageTemplates.Where(x =>
                    x.BrandId == brand.Id &&
                    x.LanguageCode == oldLanguage.Code);

                messageTemplates.ForEach(messageTemplate => _repository.MessageTemplates.Remove(messageTemplate));

                brand.Languages.Remove(oldLanguage);
            });
        }

        private void AddBrandLanguagesAndMessageTemplates(Data.Brand brand, BrandLanguagesAssigned message)
        {
            brand.DefaultLanguageCode = message.DefaultLanguageCode;

            var newLanguages = message.Languages.Where(x => brand.Languages.All(y => y.Code != x.Code));

            newLanguages.ForEach(newLanguage =>
            {
                var language = _repository.Languages.Single(x => x.Code == newLanguage.Code);

                brand.Languages.Add(language);

                AddDefaultMessageTemplates(brand, language);
            });
        }

        private void AddDefaultMessageTemplates(Data.Brand brand, Language language)
        {
            var culture = CultureInfo.GetCultureInfo(language.Code);

            var resourceSet = DefaultMessageTemplates.ResourceManager.GetResourceSet(culture, true, true);

            foreach (DictionaryEntry resource in resourceSet)
            {
                var messageTemplate = JsonConvert.DeserializeObject<MessageTemplate>((string)resource.Value);

                messageTemplate.BrandId = brand.Id;
                messageTemplate.Brand = brand;
                messageTemplate.LanguageCode = language.Code;
                messageTemplate.Language = language;
                messageTemplate.Status = Status.Active;
                messageTemplate.Activated = messageTemplate.Created = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId);
                messageTemplate.ActivatedBy = messageTemplate.CreatedBy = "System";

                _repository.MessageTemplates.Add(messageTemplate);
            }
        }

        public void Consume(PlayerVipLevelChanged message)
        {
            var player = _repository.Players.Single(x => x.Id == message.PlayerId);
            player.VipLevelId = message.VipLevelId;
            _repository.SaveChanges();
        }

        public void Consume(SendBrandSms command)
        {
            _service.TrySendBrandSms(
                string.Empty,
                command.RecipientNumber,
                command.BrandId,
                command.MessageType,
                command.Model);
        }

        public void Consume(SendPlayerAMessage command)
        {
            var foramttedModel = FormatModel(command.MessageType, command.Model);
            _service.TrySendPlayerMessage(
                command.PlayerId,
                command.MessageType,
                command.MessageDeliveryMethod,
                foramttedModel);
        }

        private IPlayerMessageTemplateModel FormatModel(MessageType messageType, IPlayerMessageTemplateModel model)
        {
            switch (messageType)
            {
                case MessageType.BonusIssued:
                    var bonusIssuedModel = (BonusIssuedModel)model;
                    return Mapper.Map<BonusIssuedFormattedModel>(bonusIssuedModel);
                case MessageType.BonusWageringRequirement:
                    var wageringModel = (BonusWageringRequirementModel)model;
                    return Mapper.Map<BonusWageringRequirementFormattedModel>(wageringModel);
                case MessageType.HighDepositReminder:
                    var highDepositModel = (HighDepositReminderModel)model;
                    return Mapper.Map<HighDepositReminderFormattedModel>(highDepositModel);
                default:
                    return model;
            }
        }
    }
}