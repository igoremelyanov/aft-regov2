using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Data.Player.Enums;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Commands;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Events;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Core.Player.Resources;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using ServiceStack.Validation;
using ChangePassword = AFT.RegoV2.Core.Auth.Interface.Data.ChangePassword;
using VipLevel = AFT.RegoV2.Core.Common.Data.Player.VipLevel;
using VipLevelId = AFT.RegoV2.Core.Player.Interface.Data.VipLevelId;

namespace AFT.RegoV2.Core.Player.ApplicationServices
{
    public class PlayerCommands : MarshalByRefObject, IApplicationService, IPlayerCommands
    {
        private readonly BrandQueries _brandQueries;
        private readonly IPlayerRepository _repository;
        private readonly IPlayerQueries _playerQueries;
        private readonly IDocumentService _documentsService;
        private readonly IAuthCommands _authCommands;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEventBus _eventBus;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IServiceBus _serviceBus;
        private readonly IAuthQueries _authQueries;

        // TODO: localize
        private readonly string hasVerifiedDocumentOfThisTypeMessage = "Player already has verified document of this type.";

        static PlayerCommands()
        {
            Mapper.CreateMap<RegistrationData, Common.Data.Player.Player>()
                .ForMember(x => x.DateOfBirth, y => y.MapFrom(z => Convert.ToDateTime(z.DateOfBirth)))
                .ForMember(x => x.BrandId, y => y.MapFrom(z => new Guid(z.BrandId)))
                .ForMember(x => x.Gender, y => y.MapFrom(z => Enum.Parse(typeof(Gender), z.Gender)))
                .ForMember(x => x.Title, y => y.MapFrom(z => Enum.Parse(typeof(Title), z.Title)))
                .ForMember(x => x.ContactPreference,
                    y => y.MapFrom(z => Enum.Parse(typeof(ContactMethod), z.ContactPreference)))
                .ForMember(x => x.IdStatus, y => y.MapFrom(z => Enum.Parse(typeof(IdStatus), z.IdStatus)))
                .ForMember(x => x.IsInactive, y => y.MapFrom(z => z.IsInactive))
                .ForMember(x => x.SecurityQuestionId,
                    y => y.MapFrom(z => string.IsNullOrEmpty(z.SecurityQuestionId) ? (Guid?)null : new Guid(z.SecurityQuestionId)))
                .ForMember(x => x.ReferralId,
                    y => y.MapFrom(z => string.IsNullOrEmpty(z.ReferralId) ? (Guid?)null : new Guid(z.ReferralId)));

            Mapper.CreateMap<EditPlayerData, Common.Data.Player.Player>()
                .ForMember(x => x.DateOfBirth, y => y.MapFrom(z => Convert.ToDateTime(z.DateOfBirth)));

            Mapper.CreateMap<Common.Data.Player.Player, PlayerInfoLog>();
        }

        public PlayerCommands(
            IPlayerRepository repository,
            IEventBus eventBus,
            IMessageTemplateService messageTemplateService,
            BrandQueries brandQueries,
            IActorInfoProvider actorInfoProvider,
            IPlayerQueries playerQueries,
            IDocumentService documentsService,
            IAuthCommands authCommands,
            IAuthQueries authQueries,
            IPaymentQueries paymentQueries,
            IServiceBus serviceBus)
        {
            _repository = repository;
            _eventBus = eventBus;
            _brandQueries = brandQueries;
            _messageTemplateService = messageTemplateService;
            _actorInfoProvider = actorInfoProvider;
            _playerQueries = playerQueries;
            _documentsService = documentsService;
            _authCommands = authCommands;
            _authQueries = authQueries;
            _paymentQueries = paymentQueries;
            _serviceBus = serviceBus;
        }

        public ValidationResult ValidateThatNewPasswordCanBeSent(SendNewPasswordData data)
        {
            if (string.IsNullOrWhiteSpace(data.NewPassword))
                data.NewPassword = PasswordGenerator.Create();

            var validator = new SendNewPasswordValidator(_repository, _authQueries);

            return validator.Validate(data);
        }

        public ValidationResult ValidateThatPlayerCanBeRegistered(RegistrationData data)
        {
            var validator = new RegisterValidator(_repository, _brandQueries);
            return validator.Validate(data);
        }

        public ValidationResult ValidateThatPlayerInfoCanBeEdited(EditPlayerData data)
        {
            var validator = new EditPlayerValidator(_repository, _brandQueries);
            return validator.Validate(data);
        }

        public void SendNewPassword(SendNewPasswordData request)
        {
            var validationResult = ValidateThatNewPasswordCanBeSent(request);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var newPassword = request.NewPassword;

            if (string.IsNullOrWhiteSpace(newPassword))
                newPassword = PasswordGenerator.Create();

            var player = _repository.Players.Single(p => p.Id == request.PlayerId);

            var changePassword = new ChangePassword
            {
                ActorId = player.Id,
                NewPassword = newPassword
            };

            var changePasswordValidationResult = _authQueries.GetValidationResult(changePassword);

            if (!changePasswordValidationResult.IsValid)
                throw new RegoValidationException(changePasswordValidationResult);

            _authCommands.ChangePassword(changePassword);

            if (request.SendBy == SendBy.Email)
                SendEmailNewPassword(request.PlayerId, newPassword);
            else
                SendSmsNewPassword(player.Id, newPassword);
        }

        public void SendResetPasswordUrl(ResetPasswordData request)
        {
            /*
                        var validator = new ResetPasswordValidator(_repository);
                        var validationResult = validator.Validate(request);

                        if (!validationResult.IsValid)
                            throw new RegoValidationException(validationResult);
            */

            var player = _playerQueries.GetPlayerByUsername(request.Id)
                ?? _playerQueries.GetPlayerByEmail(request.Id);

            var brand = _brandQueries.GetBrand(player.BrandId);

            var token = Guid.NewGuid().ToString("N");

            SetPlayerResetPasswordToken(player.Id, token);

            var model = new ForgotPasswordModel
            {
                ResetPasswordUrl = string.Format(
                    "{0}en-US/Home/ForgetPasswordStep2?token={1}",
                    brand.WebsiteUrl,
                    token)
            };

            _messageTemplateService.TrySendPlayerMessage(
                player.Id,
                MessageType.ForgotPassword,
                MessageDeliveryMethod.Email,
                model,
                true);
        }

        private void SetPlayerResetPasswordToken(Guid playerId, string token)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var player = _repository.Players
                    .Single(o => o.Id == playerId);

                player.ResetPasswordToken = token;
                player.ResetPasswordDate = DateTimeOffset.Now;

                _repository.SaveChanges();
                scope.Complete();
            }
        }

        public ValidationResult Login(string username, string password, LoginRequestContext context)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = _playerQueries.GetValidationFailures(username, password);

                if (validationResult.IsValid)
                {
                    var player = _repository.Players.Single(p => p.Username == username);
                    player.FailedLoginAttempts = 0;
                    player.LastActivityDate = DateTimeOffset.Now;

                    _eventBus.Publish(new MemberAuthenticationSucceded
                    {
                        BrandId = context.BrandId,
                        IPAddress = context.IpAddress,
                        Headers = context.BrowserHeaders,
                        EventCreatedBy = username
                    });
                }
                else
                {
                    var loginAttemptedPlayer = _repository.Players
                        .SingleOrDefault(p => p.Username == username);

                    if (loginAttemptedPlayer != null && !loginAttemptedPlayer.IsSelfExcludedOrTimedOut)
                        LockPlayerOnTooManyFailedLoginAttempts(loginAttemptedPlayer);

                    _eventBus.Publish(new MemberAuthenticationFailed
                    {
                        BrandId = context.BrandId,
                        Username = username,
                        IPAddress = context.IpAddress,
                        Headers = context.BrowserHeaders,
                        FailReason = string.Join(";", validationResult.Errors.Select(x => x.ErrorMessage)),
                    });
                }

                _repository.SaveChanges();
                scope.Complete();

                return validationResult;
            }
        }

        private static void LockPlayerOnTooManyFailedLoginAttempts(Common.Data.Player.Player player)
        {
            if (player == null)
                return;

            player.FailedLoginAttempts++;

            var maxFailedLoginAttempts =
                Convert.ToInt32(ConfigurationManager.AppSettings["MaxFailedLoginAttempts"]);

            if (player.FailedLoginAttempts >= maxFailedLoginAttempts)
                player.IsLocked = true;
        }

        [Permission(Permissions.Update, Module = Modules.PlayerManager)]
        public void Edit(EditPlayerData request)
        {
            var validationResult = new EditPlayerValidator(_repository, _brandQueries).Validate(request);

            if (!validationResult.IsValid)
            {
                throw new RegoValidationException(validationResult);
            }

            var player = _repository.Players
                .Include(p => p.Brand)
                .Include(p => p.VipLevel)
                .FirstOrDefault(p => p.Id == request.PlayerId);

            if (player == null)
                throw new RegoException(string.Format("Can't find Player with Id = {0}", request.PlayerId));

            var r = Mapper.Map<Common.Data.Player.Player>(request);

            player.FirstName = r.FirstName;
            player.LastName = r.LastName;
            player.DateOfBirth = r.DateOfBirth;
            player.Title = r.Title;
            player.Gender = r.Gender;
            player.Email = r.Email;
            player.PhoneNumber = r.PhoneNumber;
            player.MailingAddressLine1 = r.MailingAddressLine1;
            player.MailingAddressLine2 = r.MailingAddressLine2;
            player.MailingAddressLine3 = r.MailingAddressLine3;
            player.MailingAddressLine4 = r.MailingAddressLine4;
            player.MailingAddressCity = r.MailingAddressCity;
            player.MailingAddressPostalCode = r.MailingAddressPostalCode;
            player.MailingAddressStateProvince = r.MailingAddressStateProvince;
            player.PhysicalAddressLine1 = r.PhysicalAddressLine1;
            player.PhysicalAddressLine2 = r.PhysicalAddressLine2;
            player.PhysicalAddressLine3 = r.PhysicalAddressLine3;
            player.PhysicalAddressLine4 = r.PhysicalAddressLine4;
            player.PhysicalAddressCity = r.PhysicalAddressCity;
            player.PhysicalAddressPostalCode = r.PhysicalAddressPostalCode;
            player.PhysicalAddressStateProvince = r.PhysicalAddressStateProvince;
            player.CountryCode = r.CountryCode;
            player.ContactPreference = r.ContactPreference;
            player.AccountAlertEmail = r.AccountAlertEmail;
            player.AccountAlertSms = r.AccountAlertSms;
            player.MarketingAlertEmail = r.MarketingAlertEmail;
            player.MarketingAlertPhone = r.MarketingAlertPhone;
            player.MarketingAlertSms = r.MarketingAlertSms;

            _repository.SaveChanges();

            var country = _brandQueries.GetCountry(request.CountryCode);

            _eventBus.Publish(new PlayerUpdated
            {
                Id = player.Id,
                PaymentLevelId = player.PaymentLevelId,
                VipLevel = player.VipLevel != null ? player.VipLevel.Name : null,
                VipLevelId = player.VipLevel != null ? player.VipLevel.Id : Guid.Empty,
                DisplayName = GetFullName(player),
                DateOfBirth = player.DateOfBirth,
                Title = player.Title.ToString(),
                Gender = player.Gender.ToString(),
                Email = player.Email,
                PhoneNumber = player.PhoneNumber,
                CountryName = country.Name,
                AddressLines = new[]
                    {
                        player.MailingAddressLine1,
                        player.MailingAddressLine2,
                        player.MailingAddressLine3,
                        player.MailingAddressLine4
                    },
                ZipCode = player.MailingAddressPostalCode,
                AccountAlertEmail = player.AccountAlertEmail,
                AccountAlertSms = player.AccountAlertSms,
                MarketingAlertEmail = player.MarketingAlertEmail,
                MarketingAlertPhone = player.MarketingAlertPhone,
                MarketingAlertSms = player.MarketingAlertSms,
                EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
            });
        }

        [Permission(Permissions.AssignVipLevel, Module = Modules.PlayerManager)]
        public void ChangePlayerVipLevel(VipLevelId oldVipLevelId, VipLevelId newVipLevelId)
        {
            var players = _repository.Players.Include(p => p.VipLevel)
                .Where(o => o.VipLevel.Id == oldVipLevelId)
                .ToArray();

            var vipLevel = _repository.VipLevels.Single(o => o.Id == newVipLevelId);

            foreach (var player in players)
                player.VipLevel = vipLevel;

            _repository.SaveChanges();

            foreach (var player in players)
            {
                _eventBus.Publish(new PlayerVipLevelChanged(player.Id, newVipLevelId, null)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
                });
            }
        }

        [Permission(Permissions.AssignVipLevel, Module = Modules.PlayerManager)]
        public void AssignVip(Common.Data.Player.Player player, VipLevel vipLevel)
        {
            player.VipLevel = vipLevel;
        }

        public bool ActivateViaEmail(string emailActivationToken)
        {
            return Activate(emailActivationToken, ContactType.Email);
        }

        public bool ActivateViaSms(string smsActivationToken)
        {
            return Activate(smsActivationToken, ContactType.Mobile);
        }

        private bool Activate(string activationToken, ContactType contactType)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var player = contactType == ContactType.Email
                    ? _repository.Players.SingleOrDefault(x => x.AccountActivationEmailToken == activationToken)
                    : _repository.Players.SingleOrDefault(x => x.AccountActivationSmsToken == activationToken);

                if (player == null || !player.IsInactive)
                    return false;

                if (contactType == ContactType.Email && player.AccountActivationEmailToken == string.Empty) return false;
                if (contactType == ContactType.Mobile && player.AccountActivationSmsToken == string.Empty) return false;

                player.IsInactive = false;
                player.AccountActivationEmailToken = string.Empty;
                player.AccountActivationSmsToken = string.Empty;

                if (contactType == ContactType.Email)
                    player.AccountActivationEmailToken = string.Empty;

                _repository.SaveChanges();

                _eventBus.Publish(new PlayerActivated(player.Id)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
                });
                _eventBus.Publish(new PlayerContactVerified(player.Id, contactType)
                {
                    EventCreatedBy = player.Username,
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
                });

                scope.Complete();

                return true;
            }
        }

        public void ReferFriends(ReferralData referralData)
        {
            var validationResult = new ReferalDataValidator(_repository).Validate(referralData);

            if (validationResult.IsValid == false)
            {
                throw new RegoValidationException(validationResult);
            }

            var referrer = _repository.Players.Single(p => p.Id == referralData.ReferrerId);
            var brand = _brandQueries.GetBrand(referrer.BrandId);
            var referralLink = $"{brand.WebsiteUrl}Home/Register?referralId={referrer.RefIdentifier}";

            var model = new ReferFriendsModel
            {
                Referrer = referrer.Username,
                ReferralUrl = referralLink
            };

            referralData.PhoneNumbers.ForEach(phoneNumber => _serviceBus.PublishMessage(new SendBrandSms
            {
                BrandId = referrer.Brand.Id,
                MessageType = MessageType.ReferFriends,
                Model = model,
                RecipientNumber = phoneNumber
            }));
        }

        [Permission(Permissions.Activate, Module = Modules.PlayerManager)]
        [Permission(Permissions.Deactivate, Module = Modules.PlayerManager)]
        public void SetStatus(PlayerId playerId, bool active)
        {
            var player = _repository.Players
                .Include(x => x.Brand)
                .SingleOrDefault(x => x.Id == playerId);

            if (player == null)
                throw new RegoException(string.Format("Can't find Player with Id = {0}", playerId));

            player.IsInactive = !active;

            _repository.SaveChanges();

            if (active)
                _eventBus.Publish(new PlayerActivated(player.Id)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
                });
            else
                _eventBus.Publish(new PlayerDeactivated(player.Id)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
                });
        }

        public void ResendActivationEmail(Guid id)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.RepeatableRead }))
            {
                var player = _repository.Players.Single(x => x.Id == id);
                player.AccountActivationEmailToken = Guid.NewGuid().ToString();
                _repository.SaveChanges();

                SendActivationEmail(player.Id, player.AccountActivationEmailUrl);

                scope.Complete();
            }
        }

        [Permission(Permissions.Create, Module = Modules.PlayerManager)]
        public Guid Register(RegistrationData request)
        {
            if (!string.IsNullOrEmpty(request.PhoneNumber))
                request.PhoneNumber = request.PhoneNumber
                    .Replace(" ", string.Empty)
                    .Replace("+", "00");

            // validate the request
            new RegisterValidator(_repository, _brandQueries).ValidateAndThrow(request);

            // prepare the Player data
            var player = Mapper.Map<Common.Data.Player.Player>(request);
            var playerActivationMethod = _brandQueries.GetPlayerActivationMethod(player.BrandId);

            var defaultVipLevel = _playerQueries.GetDefaultVipLevel(player.BrandId);

            player.Id = Guid.NewGuid();
            player.VipLevelId = defaultVipLevel.Id;

            var defaultPaymentLevelId = _brandQueries.GetDefaultPaymentLevelId(player.BrandId, player.CurrencyCode);
            player.PaymentLevelId = defaultPaymentLevelId ?? Guid.Empty;
            player.IsInactive = !GetInitialIsActiveStatus(request, playerActivationMethod);
            player.IsLocked = request.IsLocked;

            player.IdentityVerifications = new Collection<IdentityVerification>();
            player.Brand = _repository.Brands
                .Single(o => o.Id == player.BrandId);

            player.DateRegistered = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId);

            if (playerActivationMethod == PlayerActivationMethod.Sms ||
                playerActivationMethod == PlayerActivationMethod.EmailOrSms)
                player.AccountActivationSmsToken = new Random().Next(100000, 999999).ToString("D6");

            // Set marketing alerts by default, later could be configured in Player Info
            player.MarketingAlertEmail = true;
            player.MarketingAlertPhone = true;
            player.MarketingAlertSms = true;

            var country = _brandQueries.GetCountry(player.CountryCode);
            var culture = _brandQueries.GetCulture(player.CultureCode);

            // open the scope here
            using (var scope = CustomTransactionScope.GetTransactionScope(IsolationLevel.RepeatableRead))
            {
                _repository.Players.Add(player);
                _repository.SaveChanges();
                _authCommands.CreateActor(new CreateActor
                {
                    ActorId = player.Id,
                    Username = player.Username,
                    Password = request.Password
                });

                _eventBus.Publish(new PlayerRegistered
                {
                    PlayerId = player.Id,
                    BrandId = player.BrandId,
                    CountryCode = player.CountryCode,
                    CountryName = country.Name,
                    VipLevel = defaultVipLevel.Code,
                    VipLevelId = defaultVipLevel.Id,
                    DateRegistered = player.DateRegistered,
                    DisplayName = GetFullName(player),
                    CurrencyCode = player.CurrencyCode,
                    PaymentLevelId = player.PaymentLevelId,
                    UserName = player.Username,
                    Email = player.Email,
                    PhoneNumber = player.PhoneNumber,
                    AccountActivationToken = player.AccountActivationEmailToken,
                    RefIdentifier = player.RefIdentifier,
                    ReferralId = player.ReferralId,
                    IPAddress = player.IpAddress,
                    Title = player.Title.ToString(),
                    Gender = player.Gender.ToString(),
                    //Required for pass Json serialization/deserialization and impact from server's timespan
                    DateOfBirth = new DateTimeOffset(player.DateOfBirth.Date, TimeSpan.Zero),
                    AddressLines = new[]
                        {
                            player.MailingAddressLine1,
                            player.MailingAddressLine2,
                            player.MailingAddressLine3,
                            player.MailingAddressLine4
                        },
                    ZipCode = player.MailingAddressPostalCode,
                    Language = culture.Name,
                    CultureCode = culture.Code,
                    IsActive = !player.IsInactive,
                    FirstName = player.FirstName,
                    LastName = player.LastName,
                    AccountAlertEmail = player.AccountAlertEmail,
                    AccountAlertSms = player.AccountAlertSms,
                    EventCreated = player.DateRegistered,
                });

                SendActivationMessages(player, playerActivationMethod);

                scope.Complete();
            }

            return player.Id;
        }

        public ValidationResult ValidateRegisterInfo(RegistrationData request)
        {
            if (!string.IsNullOrEmpty(request.PhoneNumber))
                request.PhoneNumber = request.PhoneNumber
                    .Replace(" ", string.Empty)
                    .Replace("+", "00");

            return new RegisterValidator(_repository, _brandQueries).Validate(request);
        }

        private bool GetInitialIsActiveStatus(RegistrationData registrationData, PlayerActivationMethod playerActivationMethod)
        {
            if (registrationData.IsRegisteredFromAdminSite)
                return !registrationData.IsInactive;

            return playerActivationMethod == PlayerActivationMethod.Automatic;
        }

        private void SendActivationMessages(Common.Data.Player.Player player, PlayerActivationMethod playerActivationMethod)
        {
            switch (playerActivationMethod)
            {
                case PlayerActivationMethod.Email:
                    SendActivationEmail(player.Id, player.AccountActivationEmailUrl);
                    break;
                case PlayerActivationMethod.Sms:
                    SendActivationSms(player.Id, player.AccountActivationSmsToken);
                    break;
                case PlayerActivationMethod.EmailOrSms:
                    SendActivationEmail(player.Id, player.AccountActivationEmailUrl);
                    SendActivationSms(player.Id, player.AccountActivationSmsToken);
                    break;
                case PlayerActivationMethod.Automatic:
                    var model = new PlayerRegisteredModel();
                    _messageTemplateService.TrySendPlayerMessage(
                        player.Id,
                        MessageType.PlayerRegistered,
                        MessageDeliveryMethod.Email,
                        model,
                        true);
                    _messageTemplateService.TrySendPlayerMessage(
                        player.Id,
                        MessageType.PlayerRegistered,
                        MessageDeliveryMethod.Sms,
                        model);
                    break;
            }
        }

        private void SendActivationSms(Guid playerId, string token)
        {
            var model = new RegistrationVerificationModel
            {
                VerificationCode = token
            };

            _messageTemplateService.TrySendPlayerMessage(
                playerId,
                MessageType.RegistrationVerification,
                MessageDeliveryMethod.Sms,
                model,
                true);
        }

        private void SendEmailNewPassword(Guid playerId, string newPassword)
        {
            var model = new NewPasswordModel
            {
                NewPassword = newPassword
            };

            _messageTemplateService.TrySendPlayerMessage(
                playerId,
                MessageType.NewPassword,
                MessageDeliveryMethod.Email,
                model,
                true);

            var player = _playerQueries.GetPlayer(playerId);
            _eventBus.Publish(new NewPasswordSent(player.Username, player.Email)
            {
                EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
            });
        }

        private void SendActivationEmail(Guid playerId, string activationUrl)
        {
            var model = new RegistrationVerificationModel
            {
                VerificationUrl = activationUrl
            };

            _messageTemplateService.TrySendPlayerMessage(
                playerId,
                MessageType.RegistrationVerification,
                MessageDeliveryMethod.Email,
                model,
                true);
        }

        private void SendSmsNewPassword(
            Guid playerId,
            string newPassword)
        {
            var model = new NewPasswordModel
            {
                NewPassword = newPassword,
            };

            _messageTemplateService.TrySendPlayerMessage(
                playerId,
                MessageType.NewPassword,
                MessageDeliveryMethod.Sms,
                model,
                true);
        }

        [Permission(Permissions.AssignVipLevel, Module = Modules.PlayerManager)]
        public void ChangeVipLevel(PlayerId playerId, VipLevelId vipLevelId, string remarks)
        {
            var player = _repository.Players
                .Include(x => x.Brand)
                .SingleOrDefault(x => x.Id == playerId);

            if (player != null)
            {
                var vipLevel = _repository.VipLevels.SingleOrDefault(x => x.Id == vipLevelId);

                if (vipLevel != null)
                {
                    player.VipLevel = vipLevel;

                    _repository.SaveChanges();

                    _eventBus.Publish(new PlayerVipLevelChanged(player.Id, vipLevel.Id, remarks)
                    {
                        EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId)
                    });
                }
            }
        }

        public ValidationResult ValidatePlayerPaymentLevelCanBeChanged(ChangePaymentLevelData command)
        {
            var validator = new ChangePlayerPaymentLevelValidator(_repository, _paymentQueries);
            var validationResult = validator.Validate(command);
            return validationResult;
        }

        public void ChangePaymentLevel(ChangePaymentLevelData command)
        {
            var player = _repository.Players
                .Include(x => x.Brand)
                .SingleOrDefault(x => x.Id == command.PlayerId);

            if (player != null)
            {
                var oldPaymentLevel = _paymentQueries.GetPaymentLevel(player.PaymentLevelId);
                var newPaymentLevel = _paymentQueries.GetPaymentLevel(command.PaymentLevelId);

                if (newPaymentLevel != null)
                {
                    using (var scope = CustomTransactionScope.GetTransactionScope())
                    {
                        player.PaymentLevelId = newPaymentLevel.Id;

                        _repository.SaveChanges();

                        _eventBus.Publish(new PlayerPaymentLevelChanged
                        {
                            PlayerId = player.Id,
                            NewPaymentLevelName = newPaymentLevel.Name,
                            NewPaymentLevelId = command.PaymentLevelId,
                            OldPaymentLevelId = oldPaymentLevel.Id,
                            OldPaymentLevelName = oldPaymentLevel.Name,
                            EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
                            Remarks = command.Remarks
                        });

                        scope.Complete();
                    }
                }
            }
        }

        public void ChangePassword(ChangePasswordData request)
        {
            var validator = new ChangePasswordValidator(_repository);
            var validationResult = validator.Validate(request);

            if (validationResult.IsValid == false)
                throw new RegoValidationException(validationResult);

            var loginValidationResult = _playerQueries.GetValidationFailures(request.Username, request.OldPassword);

            if (!loginValidationResult.IsValid)
                throw new RegoValidationException(loginValidationResult);

            var player = _repository.Players.Single(pl => pl.Username == request.Username);



            _authCommands.ChangePassword(new ChangePassword
            {
                ActorId = player.Id,
                NewPassword = request.NewPassword
            });
        }

        public void ChangeSecurityQuestion(Guid playerId, Guid questionId, string answer)
        {
            var validationResult = new ChangeSecurityQuestionValidator(_repository).Validate(new ChangeSecurityQuestionData
            {
                Id = playerId.ToString(),
                SecurityAnswer = answer,
                SecurityQuestionId = questionId.ToString()
            });

            if (!validationResult.IsValid)
            {
                throw new RegoValidationException(validationResult);
            }

            var player = _repository.Players.SingleOrDefault(x => x.Id == playerId);
            if (player == null)
                throw new ValidationError(PlayerAccountResponseCode.PlayerDoesNotExist.ToString(), Messages.PlayerDoesNotExist);

            player.SecurityQuestionId = questionId;
            player.SecurityAnswer = answer;
            _repository.SaveChanges();

        }

        public void SendMobileVerificationCode(Guid playerId)
        {
            var player = _repository.Players.SingleOrDefault(p => p.Id == playerId);
            if (player == null)
                throw new RegoException("Player not found.");
            if (string.IsNullOrEmpty(player.PhoneNumber))
                throw new RegoException("No mobile number is available for the player.");
            if (player.IsPhoneNumberVerified)
                throw new RegoException("Mobile number is already verified.");
            player.MobileVerificationCode = new Random().Next(0, 9999);
            _repository.SaveChanges();

            SendSmsMobileVerificationCode(player.Id, player.MobileVerificationCode);
        }

        private void SendSmsMobileVerificationCode(Guid playerId, int mobileVerificationCode)
        {
            var model = new ContactVerificationModel
            {
                VerificationCode = mobileVerificationCode.ToString("D4")
            };

            _messageTemplateService.TrySendPlayerMessage(
                playerId,
                MessageType.ContactVerification,
                MessageDeliveryMethod.Sms,
                model,
                true);

            _eventBus.Publish(new
                MobileVerificationCodeSentSms(playerId, mobileVerificationCode.ToString("D4")));
        }

        public void VerifyMobileNumber(Guid playerId, string verificationCode)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var player = _repository.Players.SingleOrDefault(p => p.Id == playerId);
                if (player == null)
                    throw new RegoException("Player not found.");
                if (player.IsPhoneNumberVerified)
                    throw new ArgumentException("Mobile number is already verified.");

                int code;
                var codeIsValid = int.TryParse(verificationCode, out code);
                if (codeIsValid == false)
                    throw new ArgumentException("Verification code should be 4 digit number.");
                if (player.MobileVerificationCode != code)
                    throw new ArgumentException("Verification code is incorrect.");

                player.IsPhoneNumberVerified = true;
                _repository.SaveChanges();

                _eventBus.Publish(new PlayerContactVerified(player.Id, ContactType.Mobile)
                {
                    EventCreatedBy = player.Username,
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
                });
                scope.Complete();
            }
        }

        public void AddPlayerInfoLogRecord(Guid playerId)
        {
            var player = _repository.Players.Single(p => p.Id == playerId);
            var playerLogRecord = Mapper.Map<PlayerInfoLog>(player);
            playerLogRecord.Id = Guid.NewGuid();
            playerLogRecord.Player = player;
            _repository.PlayerInfoLog.Add(playerLogRecord);
            _repository.SaveChanges();
        }

        public void ResetPlayerActivityStatus(string username)
        {
            var player = _repository.Players.Single(p => p.Username == username);
            player.LastActivityDate = null;

            _repository.SaveChanges();
        }

        public void UpdateLogRemark(Guid id, string remarks)
        {
            var log = _repository.PlayerActivityLog.SingleOrDefault(l => l.Id == id);
            if (log == null)
                throw new RegoException("Activity log record not found");

            log.Remarks = remarks;
            log.UpdatedBy = _actorInfoProvider.Actor.UserName;
            log.DateUpdated = DateTimeOffset.Now;

            _repository.SaveChanges();
        }

        public void UpdatePlayersPaymentLevel(Guid currentPaymentLevelId, Guid newPaymentLevelId)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var players = _repository.Players.Where(x => x.PaymentLevelId == currentPaymentLevelId);

                foreach (var player in players)
                {
                    player.PaymentLevelId = newPaymentLevelId;
                }

                _repository.SaveChanges();

                scope.Complete();
            }
        }

        public string GetFullName(Common.Data.Player.Player player)
        {
            return player.FirstName + " " + player.LastName;
        }

        private Guid? SaveFile(string fileName, byte[] content, Guid playerId)
        {
            if (content != null && content.Length > 0)
            {
                var player = _repository.Players
                    .Include(o => o.Brand)
                    .Single(o => o.Id == playerId);

                return _documentsService.SaveFile(fileName, content, playerId, player.BrandId, player.Brand.LicenseeId);
            }
            return null;
        }

        public IdentityVerification UploadIdentificationDocuments(IdUploadData uploadData, Guid playerId, string userName)
        {
            var player = _repository.Players
                .Include(o => o.IdentityVerifications)
                .Single(o => o.Id == playerId);

            if (player.IdentityVerifications
                .Any(o => o.DocumentType == uploadData.DocumentType
                    && o.VerificationStatus == VerificationStatus.Verified))
                throw new InvalidOperationException(hasVerifiedDocumentOfThisTypeMessage);

            IdentityVerification identity;
            var id = Guid.NewGuid();

            var frontImageId = SaveFile(uploadData.FrontName, uploadData.FrontIdFile, playerId);
            var backImageId = SaveFile(uploadData.BackName, uploadData.BackIdFile, playerId);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                identity = new IdentityVerification
                {
                    Id = id,
                    CardNumber = uploadData.CardNumber,
                    DocumentType = uploadData.DocumentType,
                    ExpirationDate = uploadData.CardExpirationDate,
                    DateUploaded = DateTimeOffset.Now,
                    UploadedBy = userName,
                    VerificationStatus = VerificationStatus.Pending,
                    FrontFile = frontImageId,
                    BackFile = backImageId,
                    Remarks = uploadData.Remarks
                };
                player.IdentityVerifications.Add(identity);

                _repository.SaveChanges();

                scope.Complete();
            }

            _eventBus.Publish(new IdentityDocumentUploaded(identity));

            return identity;
        }

        public void VerifyIdDocument(Guid id, string userName)
        {
            var identification = _repository.Players
                .Include(o => o.IdentityVerifications)
                .SelectMany(o => o.IdentityVerifications)
                .Include(o => o.Player)
                .Include(o => o.Player.IdentityVerifications)
                .Single(o => o.Id == id);

            if (identification.Player.IdentityVerifications
                .Any(o => o.DocumentType == identification.DocumentType
                    && o.VerificationStatus == VerificationStatus.Verified))
                throw new InvalidOperationException(hasVerifiedDocumentOfThisTypeMessage);

            identification.VerificationStatus = VerificationStatus.Verified;
            identification.DateVerified = DateTimeOffset.Now;
            identification.VerifiedBy = userName;

            _repository.SaveChanges();

            _eventBus.Publish(new IdentityDocumentVerified(identification)
            {
                EventCreated = DateTimeOffset.Now.ToBrandOffset(identification.Player.Brand.TimezoneId),
            });
        }

        public void UnverifyIdDocument(Guid id, string userName)
        {
            var identification = _repository.Players
                .Include(o => o.IdentityVerifications)
                .SelectMany(o => o.IdentityVerifications)
                .Include(o => o.Player)
                .Single(o => o.Id == id);

            identification.VerificationStatus = VerificationStatus.Unverified;
            identification.DateUnverified = DateTimeOffset.Now;
            identification.UnverifiedBy = userName;

            _repository.SaveChanges();

            _eventBus.Publish(new IdentityDocumentUnverified(identification)
            {
                EventCreated = DateTimeOffset.Now.ToBrandOffset(identification.Player.Brand.TimezoneId),
            });
        }

        private void SetFreezeStatus(Guid playerId, bool isFrozen)
        {
            var player = _repository.Players.Include(x => x.Brand).SingleOrDefault(x => x.Id == playerId);
            if (player == null)
                throw new RegoException(string.Format("Can't find Player with Id = {0}", playerId));

            if (player.IsFrozen == isFrozen)
                return;

            player.IsFrozen = isFrozen;

            _repository.SaveChanges();

            if (isFrozen)
                _eventBus.Publish(new PlayerFrozen(playerId)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
                });
            else
                _eventBus.Publish(new PlayerUnfrozen(playerId)
                {
                    EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
                });
        }

        public void FreezeAccount(Guid playerId)
        {
            SetFreezeStatus(playerId, isFrozen: true);
        }

        public void UnfreezeAccount(Guid playerId)
        {
            SetFreezeStatus(playerId, isFrozen: false);
        }

        [Permission(Permissions.Activate, Module = Modules.ResponsibleGambling)]
        public void SelfExclude(Guid playerId, PlayerEnums.SelfExclusion exclusionType)
        {
            var player = _repository.Players
                .Include(o => o.Brand)
                .Single(o => o.Id == playerId);

            if (player == null)
                throw new RegoException(string.Format("Can't find Player with Id = {0}", playerId));

            player.TimeOut = null;
            player.TimeOutDate = null;
            player.SelfExclusion = (SelfExclusion?) exclusionType;
            player.SelfExclusionDate = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId);

            _repository.SaveChanges();

            DateTimeOffset selfExcludedEndDate = ExclusionDateHelper.GetSelfExcusionEndDate(player.SelfExclusion.Value,
                player.SelfExclusionDate.Value).ToBrandOffset(player.Brand.TimezoneId);

            _eventBus.Publish(new PlayerSelfExcluded(playerId, player.SelfExclusion.Value, selfExcludedEndDate)
            {
                EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
            });
        }

        [Permission(Permissions.Activate, Module = Modules.ResponsibleGambling)]
        public void TimeOut(Guid playerId, PlayerEnums.TimeOut timeOut)
        {
            var player = _repository.Players
                .Include(o => o.Brand)
                .Single(o => o.Id == playerId);

            if (player == null)
                throw new RegoException(string.Format("Can't find Player with Id = {0}", playerId));

            player.SelfExclusion = null;
            player.SelfExclusionDate = null;
            player.TimeOut = (TimeOut?) timeOut;
            player.TimeOutDate = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId);

            _repository.SaveChanges();

            DateTimeOffset timeoutEndDate = ExclusionDateHelper.GetTimeOutEndDate(player.TimeOut.Value,
                player.TimeOutDate.Value).ToBrandOffset(player.Brand.TimezoneId);

            _eventBus.Publish(new PlayerTimedOut(playerId, player.TimeOut.Value, timeoutEndDate)
            {
                EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
            });
        }

        public void Unlock(Guid playerId)
        {
            var player = _repository.Players
                .SingleOrDefault(x => x.Id == playerId);

            if (player == null)
                throw new RegoException(string.Format("Can't find Player with Id = {0}", playerId));

            player.IsLocked = false;
            player.FailedLoginAttempts = 0;

            _repository.SaveChanges();
        }

        [Permission(Permissions.View, Module = Modules.ResponsibleGambling)]
        public object GetSelfExclusionData(Guid playerId)
        {
            var player = _playerQueries.GetPlayer(playerId);

            return new
            {
                IsTimeOutEnabled = player.TimeOut.HasValue,
                TimeOut = player.TimeOut.HasValue ? (int)player.TimeOut.Value : -1,
                TimeOutStartDate = player.TimeOutDate,
                TimeOutEndDate = player.TimeOut.HasValue
                    ? ExclusionDateHelper.GetTimeOutEndDate(player.TimeOut.Value, player.TimeOutDate.Value)
                    : (DateTimeOffset?)null,
                IsSelfExclusionEnabled = player.SelfExclusion.HasValue,
                SelfExclusion = player.SelfExclusion.HasValue ? (int)player.SelfExclusion.Value : -1,
                SelfExclusionStartDate = player.SelfExclusionDate,
                SelfExclusionEndDate = player.SelfExclusion.HasValue
                    ? ExclusionDateHelper.GetSelfExcusionEndDate(player.SelfExclusion.Value, player.SelfExclusionDate.Value)
                    : (DateTimeOffset?)null,
                timeZoneOffset = player.DateRegistered.Offset,
            };
        }

        [Permission(Permissions.Deactivate, Module = Modules.ResponsibleGambling)]
        public void CancelExclusion(Guid playerId)
        {
            var player = _repository.Players
                .Include(x => x.Brand)
                .SingleOrDefault(x => x.Id == playerId);

            if (player == null)
                throw new RegoException(string.Format("Can't find Player with Id = {0}", playerId));

            player.SelfExclusion = null;
            player.SelfExclusionDate = null;
            player.TimeOut = null;
            player.TimeOutDate = null;

            _repository.SaveChanges();

            _eventBus.Publish(new PlayerCancelExclusion(playerId)
            {
                EventCreated = DateTimeOffset.Now.ToBrandOffset(player.Brand.TimezoneId),
            });
        }

        public Guid CreateSecurityQuestion(SecurityQuestion securityQuestion)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = new AddSecurityQuestionValidator(_repository).Validate(securityQuestion);

                if (!validationResult.IsValid)
                {
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);
                }

                if (securityQuestion.Id.Equals(Guid.Empty))
                {
                    securityQuestion.Id = Guid.NewGuid();
                }

                _repository.SecurityQuestions.Add(securityQuestion);
                _repository.SaveChanges();

                var securityQuestionCreated = new SecurityQuestionCreated(securityQuestion);
                _eventBus.Publish(securityQuestionCreated);

                scope.Complete();

                return securityQuestion.Id;
            }
        }

        public void SetNewPassword(Guid playerId, string newPassword)
        {
            var player = _repository.Players
                .Single(pl => pl.Id == playerId);

            _authCommands.ChangePassword(new ChangePassword
            {
                ActorId = player.Id,
                NewPassword = newPassword
            });

            var model = new ResetPasswordModel();

            _messageTemplateService.TrySendPlayerMessage(
                player.Id,
                MessageType.ResetPassword,
                MessageDeliveryMethod.Email,
                model,
                true);
        }
    }
}