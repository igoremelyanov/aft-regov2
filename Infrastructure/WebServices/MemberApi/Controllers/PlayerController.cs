using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Brand.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Data.Player.Enums;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.MemberApi.Interface.Common;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.Shared;
using AutoMapper;
using BalanceSet = AFT.RegoV2.MemberApi.Interface.Player.BalanceSet;
using Player = AFT.RegoV2.Core.Common.Data.Player.Player;
using SecurityQuestion = AFT.RegoV2.MemberApi.Interface.Player.SecurityQuestion;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using AFT.RegoV2.Core.Player.Interface.Data;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.MemberApi.Controllers
{
    public class PlayerController : BaseApiController
    {
        private readonly IPlayerRepository _repository;
        private readonly IPlayerCommands _commands;
        private readonly IPlayerQueries _queries;
        private readonly IBrandQueries _brandQueries;
        private readonly IWalletQueries _walletQueries;
        private readonly IBonusApiProxy _bonusApiProxy;
        private readonly IAuthQueries _authQueries;

        private static readonly string UriRootToProfileInfo = "api/Player/Profile?playerId=";

        static PlayerController()
        {
            CreateDataMappings();
        }

        private class SecurityAnswerCheckRequestValidator
        {
            private readonly SecurityCheckRequestAbstractValidator _validator;

            public SecurityAnswerCheckRequestValidator(IPlayerQueries playerQueries)
            {
                _validator = new SecurityCheckRequestAbstractValidator(playerQueries);
            }

            public ValidationResult Validate(SecurityAnswerRequest request)
            {
                var data = Mapper.DynamicMap<SecurityAnswerRequest, SecurityAnswerData>(request);
                var result = _validator.Validate(data);

                return new ValidationResult
                {
                    Errors = result.Errors
                        .GroupBy(o => o.PropertyName)
                        .Select(o => o.First())
                        .ToDictionary(k => k.PropertyName, v => v.ErrorMessage)
                };
            }
        }

        private ProfileResponse ProfileResponse(Guid playerId)
        {
            var player = _queries.GetPlayer(playerId);
            if (player == null)
                throw new RegoValidationException(ErrorMessagesEnum.PlayerWithRequestedIdDoesntExist.ToString());

            var question = _queries.GetSecurityQuestion(playerId);

            var profileResponse = Mapper.Map<ProfileResponse>(player);
            if (question != null)
            {
                profileResponse.SecurityQuestion = question.Question;
            }

            profileResponse.VipLevel = player.VipLevel?.Name;

            return profileResponse;
        }

        private class ResetPasswordRequestValidator
        {
            private readonly ResetPasswordRequestAbstractValidator _validator;
            private readonly IPlayerQueries _playerQueries;

            public ResetPasswordRequestValidator(IPlayerQueries playerQueries)
            {
                _playerQueries = playerQueries;
                _validator = new ResetPasswordRequestAbstractValidator(playerQueries);
            }

            public ValidationResult Validate(ResetPasswordRequest request)
            {
                var data = Mapper.Map<ResetPasswordRequest, ResetPasswordData>(request);
                var result = _validator.Validate(data);

                if (!result.IsValid)
                    return new ValidationResult
                    {
                        Errors = result.Errors
                            .GroupBy(o => o.PropertyName)
                            .Select(o => o.First())
                            .ToDictionary(k => k.PropertyName, v => v.ErrorMessage)
                    };

                var player = _playerQueries.GetPlayerByUsername(request.Id) ?? _playerQueries.GetPlayerByEmail(request.Id);
                var loginValidationResult = _playerQueries.GetValidationFailures(player.Username, null);
                return new ValidationResult
                {
                    Errors = loginValidationResult.Errors
                        .GroupBy(o => o.PropertyName)
                        .Select(o => o.First())
                        .ToDictionary(k => k.PropertyName, v => v.ErrorMessage)
                };
            }
        }

        private class ConfirmResetPasswordRequestValidator
        {
            private readonly IAuthQueries _authQueries;
            private readonly IPlayerQueries _playerQueries;
            private readonly ConfirmResetPasswordRequesAbstractValidator _validator;

            public ConfirmResetPasswordRequestValidator(IAuthQueries authQueries,
                IPlayerRepository playerRepository,
                IPlayerQueries playerQueries)
            {
                _authQueries = authQueries;
                _playerQueries = playerQueries;
                _validator = new ConfirmResetPasswordRequesAbstractValidator(playerRepository);
            }

            public ValidationResult Validate(ConfirmResetPasswordRequest request)
            {
                var player = _playerQueries.GetPlayer(request.PlayerId);
                var loginValidationResult = _playerQueries.GetValidationFailures(player.Username, null);
                if (!loginValidationResult.IsValid)
                    return new ValidationResult
                    {
                        Errors = loginValidationResult.Errors
                        .GroupBy(o => o.PropertyName)
                        .Select(o => o.First())
                        .ToDictionary(k => k.PropertyName, v => v.ErrorMessage)
                    };

                var data = Mapper.DynamicMap<ConfirmResetPasswordRequest, ConfirmResetPasswordData>(request);
                var result2 = _validator.Validate(data);
                if (!result2.IsValid)
                    return new ValidationResult
                    {
                        Errors = result2.Errors
                            .GroupBy(o => o.PropertyName)
                            .Select(o => o.First())
                            .ToDictionary(k => k.PropertyName, v => v.ErrorMessage)
                    };


                var result = _authQueries.GetValidationResult(new ChangePassword
                {
                    ActorId = request.PlayerId,
                    NewPassword = request.NewPassword
                });

                return new ValidationResult
                {
                    Errors = result.Errors
                        .GroupBy(o => o.PropertyName)
                        .Select(o => o.First())
                        .ToDictionary(k => k.PropertyName, v => v.ErrorMessage)
                };
            }
        }

        public PlayerController(IPlayerRepository repository,
            IPlayerCommands commands,
            IPlayerQueries queries,
            IBrandQueries brandQueries,
            IWalletQueries walletQueries,
            IBonusApiProxy bonusApiProxy,
            IAuthQueries authQueries)
        {
            _repository = repository;
            _brandQueries = brandQueries;
            _commands = commands;
            _queries = queries;
            _walletQueries = walletQueries;
            _bonusApiProxy = bonusApiProxy;
            _authQueries = authQueries;
        }

        private static void CreateDataMappings()
        {
            Mapper.CreateMap<Player, EditPlayerData>()
                .ForMember(p => p.DateOfBirth, opt => opt.ResolveUsing(a => a.DateOfBirth.ToString("yyyy'/'MM'/'dd")));

            Mapper.CreateMap<Player, ProfileResponse>()
                .ForMember(p => p.CountryCode, opt => opt.MapFrom(a => a.CountryCode))
                .ForMember(p => p.CurrencyCode, opt => opt.MapFrom(a => a.CurrencyCode));

            Mapper.CreateMap<ChangePasswordRequest, ChangePasswordData>();

            Mapper.CreateMap<RegisterRequest, RegistrationData>()
                .ForMember(x => x.IdStatus, y => y.Ignore())
                .ForMember(x => x.IsRegisteredFromAdminSite, y => y.Ignore());
            Mapper.CreateMap<Core.Payment.Interface.Data.PlayerBalance, BalancesResponse>()
                .ForMember(
                    dest => dest.MainFormatted,
                    opt => opt.MapFrom(src => src.Main.Format(src.CurrencyCode, false)))
                .ForMember(
                    dest => dest.PlayableFormatted,
                    opt => opt.MapFrom(src => src.Playable.Format(src.CurrencyCode, false)))
                .ForMember(
                    dest => dest.PlayableFormattedShort,
                    opt => opt.MapFrom(src => src.Playable.Format(src.CurrencyCode, false, DecimalDisplay.ShowNonZeroOnly)))
                .ForMember(
                    dest => dest.BonusFormatted,
                    opt => opt.MapFrom(src => src.Bonus.Format(src.CurrencyCode, false)))
                .ForMember(
                    dest => dest.FreeFormatted,
                    opt => opt.MapFrom(src => src.Free.Format(src.CurrencyCode, false)))
                .ForMember(
                    dest => dest.CurrencySymbol, 
                    opt => opt.MapFrom(src => CurrencyHelper.GetCurrencySymbol(src.CurrencyCode))
                );
                
            Mapper.CreateMap<PlayerWagering, WageringResponse>();

            Mapper.CreateMap<ChangePersonalInfoRequest, EditPlayerData>();
            Mapper.CreateMap<ChangeContactInfoRequest, EditPlayerData>();
            Mapper.CreateMap<ResetPasswordRequest, ResetPasswordData>();
            Mapper.CreateMap<SecurityAnswerRequest, ConfirmResetPasswordData>();

            Mapper.CreateMap<Core.Common.Data.SecurityQuestion, SecurityQuestion>();

            Mapper.CreateMap<Core.Player.Interface.Data.OnSiteMessage, Interface.Player.OnSiteMessage>();
            Mapper.CreateMap<Core.Common.Data.Player.OnSiteMessage, Interface.Player.OnSiteMessage>();
        }

        [AllowAnonymous]
        public RegisterResponse Register(RegisterRequest request)
        {
            var registerData = Mapper.Map<RegisterRequest, RegistrationData>(request);
            registerData.AccountAlertEmail = request.ContactPreference.Equals("email",
                StringComparison.InvariantCultureIgnoreCase);
            registerData.AccountAlertSms = request.ContactPreference.Equals("sms",
                StringComparison.InvariantCultureIgnoreCase);
            var userId = _commands.Register(registerData);

            return new RegisterResponse
            {
                UserId = userId
            };
        }

/*        public void ConfirmOfflineDeposit()
        {
            var player = _queries.GetPlayer(PlayerId);
            _commands.UploadIdentificationDocuments(new IdUploadData
            {
                
            }, PlayerId, player.FirstName + " " + player.LastName);
        }*/

        [AllowAnonymous]
        public ActivationResponse Activate(ActivationRequest request)
        {
            var token = request.Token;
            var activated = _commands.ActivateViaEmail(token);
            return new ActivationResponse
            {
                Activated = activated,
                Token = token
            };
        }

        [AllowAnonymous]
        public ValidationResult ValidateResetPasswordRequest(ResetPasswordRequest request)
        {
            var validator = new ResetPasswordRequestValidator(_queries);
            var result = validator.Validate(request);

            var errors = new Dictionary<string, string>();

            if (result.Errors.Any())
                errors.Add("Id", result.Errors.First().Value);

            return new ValidationResult
            {
                Errors = errors
            };
        }

        [AllowAnonymous]
        public ValidationResult ValidateSecurityAnswerRequest(SecurityAnswerRequest request)
        {
            var validator = new SecurityAnswerCheckRequestValidator(_queries);
            var result = validator.Validate(request);
            return result;
        }

        [AllowAnonymous]
        public ValidationResult ValidateConfirmResetPasswordRequest(ConfirmResetPasswordRequest request)
        {
            var validator = new ConfirmResetPasswordRequestValidator(_authQueries, _repository, _queries);
            var result = validator.Validate(request);

            var errors = new Dictionary<string, string>();

            if (result.Errors.Any())
                errors.Add("ConfirmPassword", result.Errors.First().Value);

            return new ValidationResult
            {
                Errors = errors
            };
        }

        [AllowAnonymous]
        public ResetPasswordResponse ConfirmResetPassword(ConfirmResetPasswordRequest request)
        {
            _commands.SetNewPassword(request.PlayerId, request.NewPassword);

            return new ResetPasswordResponse();
        }

        [AllowAnonymous]
        public ResetPasswordResponse ResetPassword(ResetPasswordRequest request)
        {
            var resetPasswordData = Mapper.Map<ResetPasswordRequest, ResetPasswordData>(request);
            _commands.SendResetPasswordUrl(resetPasswordData);
            return new ResetPasswordResponse();
        }

        [AllowAnonymous]
        [HttpGet]
        public GetSecurityQuestionResponse GetSecurityQuestion([FromUri]GetSecurityQuestionRequest request)
        {
            var securityQuestion = _queries.GetSecurityQuestion(request.PlayerId);
            if (securityQuestion == null)
                throw new RegoValidationException(
                    string.Format(ErrorMessagesEnum.SecurityQuestionNotAvailableForThisPlayer.ToString()));

            return new GetSecurityQuestionResponse
            {
                SecurityQuestion = securityQuestion.Question
            };
        }

        [AllowAnonymous]
        [HttpPost]
        public PlayerByResetPasswordTokenResponse GetPlayerByResetPasswordToken(
            PlayerByResetPasswordTokenRequest request)
        {
            var player = _queries.GetPlayerByResetPasswordToken(request.Token);

            if (player == null)
                throw new RegoValidationException(ErrorMessagesEnum.TokenForPlayerWasNotValid.ToString());

            if (player.ResetPasswordDate < DateTimeOffset.Now.AddDays(-1))
                throw new RegoValidationException(ErrorMessagesEnum.TokenExpired.ToString());

            return new PlayerByResetPasswordTokenResponse
            {
                PlayerId = player.Id
            };
        }

        /*        [AllowAnonymous]

                {
                    var resetPasswordData = Mapper.Map<SecurityAnswerRequest, ConfirmResetPasswordData>(request);
                    _commands.ConfirmResetPassword(resetPasswordData);
                    return new SecurityAnswerCheckResponse();
                }*/

        [HttpPost]
        //todo: explain in comments what is going on here and how this method is related to AuthServerProvider.GrantResourceOwnerCredentials
        public LogoutResponse Logout(LogoutRequest request)
        {
            _commands.ResetPlayerActivityStatus(Username);

            Request.GetOwinContext().Authentication.SignOut(); // this doesn't remove cookies, so it's useless. :((

            return new LogoutResponse
            {
                UserName = Username
            };
        }

        [HttpGet]
        public ProfileResponse Profile()
        {
            return ProfileResponse(PlayerId);
        }

        [HttpGet]
        public ProfileResponse Profile(Guid playerId)
        {
            return ProfileResponse(playerId);
        }

        [HttpGet]
        public SecurityQuestionsResponse SecurityQuestions()
        {
            var questions = _repository.SecurityQuestions.AsQueryable().AsNoTracking().ToList();
            var questionsMapped = Mapper.Map<List<Core.Common.Data.SecurityQuestion>, List<SecurityQuestion>>(questions);

            return new SecurityQuestionsResponse
            {
                SecurityQuestions = questionsMapped,
            };
        }

        [HttpPost]
        [ResponseType(typeof(ChangePasswordResponse))]
        public IHttpActionResult ChangePassword(ChangePasswordRequest request)
        {
            var requestData = Mapper.Map<ChangePasswordRequest, ChangePasswordData>(request);
            _commands.ChangePassword(requestData);

            var playerWhosePasswordWasChanged = _repository.Players.Single(pl => pl.Username == request.Username);

            var uri = UriRootToProfileInfo + playerWhosePasswordWasChanged.Id;
            return Created(uri, new ChangePasswordResponse()
            {
                UriToUserWithUpdatedPassword = uri
            });
        }


        [HttpPost]
        [ResponseType(typeof(ChangePersonalInfoResponse))]
        public IHttpActionResult ChangePersonalInfo(ChangePersonalInfoRequest request)
        {
            var player = _queries.GetPlayer(request.PlayerId);
            if (player == null)
                throw new RegoValidationException(ErrorMessagesEnum.PlayerWithRequestedIdDoesntExist.ToString());

            var playerData = Mapper.Map<EditPlayerData>(player);
            var newData = Mapper.Map<EditPlayerData>(request);
            playerData.PlayerId = request.PlayerId;
            playerData.Title = newData.Title;
            playerData.FirstName = newData.FirstName;
            playerData.LastName = newData.LastName;
            playerData.Email = newData.Email;
            playerData.DateOfBirth = newData.DateOfBirth;
            playerData.Gender = newData.Gender;
            playerData.CurrencyCode = newData.CurrencyCode;
            playerData.AccountAlertEmail = newData.AccountAlertEmail;
            playerData.AccountAlertSms = newData.AccountAlertSms;
            _commands.Edit(playerData);

            var uri = UriRootToProfileInfo + playerData.PlayerId;

            return Created(uri, new ChangePersonalInfoResponse()
            {
                UriToUserWithPersonalInfoUpdated = uri
            });
        }

        [HttpPost]
        [ResponseType(typeof(ChangeContactInfoResponse))]
        public IHttpActionResult ChangeContactInfo(ChangeContactInfoRequest request)
        {
            var player = _queries.GetPlayer(request.PlayerId);
            if (player == null)
                throw new RegoValidationException(ErrorMessagesEnum.PlayerWithRequestedIdDoesntExist.ToString());

            var playerData = Mapper.Map<EditPlayerData>(player);
            var newData = Mapper.Map<EditPlayerData>(request);
            playerData.PlayerId = request.PlayerId;
            playerData.PhoneNumber = newData.PhoneNumber;
            playerData.MailingAddressLine1 = newData.MailingAddressLine1;
            playerData.MailingAddressLine2 = newData.MailingAddressLine2;
            playerData.MailingAddressLine3 = newData.MailingAddressLine3;
            playerData.MailingAddressLine4 = newData.MailingAddressLine4;
            playerData.MailingAddressCity = newData.MailingAddressCity;
            playerData.MailingAddressPostalCode = newData.MailingAddressPostalCode;
            playerData.CountryCode = newData.CountryCode;
            playerData.ContactPreference = newData.ContactPreference;
            _commands.Edit(playerData);

            var uri = UriRootToProfileInfo + playerData.PlayerId;
            return Created(uri, new ChangeContactInfoResponse() { UriToProfileWithUpdatedContactInfo = uri });
        }

        [HttpPost]
        [ResponseType(typeof(SelfExclusionResponse))]
        public IHttpActionResult SelfExclude(SelfExclusionRequest request)
        {
            var type = (SelfExclusion)request.Option;
            _commands.SelfExclude(request.PlayerId, (PlayerEnums.SelfExclusion) type);

            var uri = UriRootToProfileInfo + request.PlayerId;
            return Created(uri, new SelfExclusionResponse() { UriToPlayerThatSelfExclusionWasAppliedTo = uri });
        }

        [ResponseType(typeof(TimeOutResponse))]
        public IHttpActionResult TimeOut(TimeOutRequest request)
        {
            TimeOut type = (TimeOut)request.Option;
            _commands.TimeOut(request.PlayerId, (PlayerEnums.TimeOut) type);

            var uri = UriRootToProfileInfo + request.PlayerId;
            return Created(uri, new TimeOutResponse() { UriToPlayerWhoWasTimeOuted = uri });
        }

        [HttpPost]
        [ResponseType(typeof(ChangeSecurityQuestionResponse))]
        public IHttpActionResult ChangeSecurityQuestion(ChangeSecurityQuestionRequest request)
        {
            var questionId = new Guid(request.SecurityQuestionId);
            _commands.ChangeSecurityQuestion(Guid.Parse(request.Id), questionId, request.SecurityAnswer);

            var uri = UriRootToProfileInfo + Guid.Parse(request.Id);

            return Created(uri, new ChangeSecurityQuestionResponse()
            {
                UriToPlayerWhoseSecurityQuestionWasChanged = uri
            });
        }

        [AllowAnonymous]
        [HttpGet]
        public RegistrationFormDataResponse RegistrationFormData([FromUri]RegistrationFormDataRequest request)
        {
            var countries = _brandQueries.GetCountriesByBrand(request.BrandId).Select(a => a.Code).ToArray();
            if (countries == null || !countries.Any())
                throw new RegoException("Brand countries are missing");

            var currencies = _brandQueries.GetCurrenciesByBrand(request.BrandId).Select(a => a.Code).ToArray();
            if (currencies == null || !currencies.Any())
                throw new RegoException("Brand currencies are missing");

            var genders = Enum.GetNames(typeof(Gender));
            var titles = Enum.GetNames(typeof(Title));
            var contactMethods = Enum.GetNames(typeof(ContactMethod));

            var questions = _repository.SecurityQuestions.AsQueryable().AsNoTracking().ToList();
            if (questions == null || !questions.Any())
                throw new RegoException("Brand security questions are missing");

            var questionsMapped = Mapper.Map<List<Core.Common.Data.SecurityQuestion>, List<SecurityQuestion>>(questions);

            return new RegistrationFormDataResponse
            {
                CountryCodes = countries,
                CurrencyCodes = currencies,
                Genders = genders,
                Titles = titles,
                ContactMethods = contactMethods,
                SecurityQuestions = questionsMapped
            };
        }

        [AllowAnonymous]
        [HttpGet]
        public LanguagesResponse Languages([FromUri]LanguagesRequest request)
        {
            var languages = _brandQueries.GetCulturesByBrand(request.BrandId);
            if (languages == null)
                throw new RegoException("Languages are missing for this brand.");

            return new LanguagesResponse
            {
                Languages = languages.Select(c => new Language
                {
                    Culture = c.Code,
                    NativeName = c.NativeName
                }).ToList()
            };
        }

        [HttpPost]
        public ReferFriendsResponse ReferFriends(ReferFriendsRequest request)
        {
            _commands.ReferFriends(new ReferralData { ReferrerId = PlayerId, PhoneNumbers = request.PhoneNumbers });
            return new ReferFriendsResponse();
        }

        [HttpPost]
        [ResponseType(typeof(VerificationCodeResponse))]
        public IHttpActionResult VerificationCode(VerificationCodeRequest request)
        {
            _commands.SendMobileVerificationCode(PlayerId);

            var uri = UriRootToProfileInfo + PlayerId;

            return Created(uri, new VerificationCodeResponse() { UriToPlayerWhomMobileVerificationCodeWasSent = uri });
        }

        [HttpPost]
        [ResponseType(typeof(VerifyMobileResponse))]
        public IHttpActionResult VerifyMobile(VerifyMobileRequest request)
        {
            _commands.VerifyMobileNumber(PlayerId, request.VerificationCode);

            var uri = UriRootToProfileInfo + PlayerId;
            return Created(uri, new VerifyMobileResponse() { UriToPlayerWhoseMobileWasVerified = uri });
        }

        [HttpGet]
        public WalletsDataResponse GetWallets([FromUri]WalletsDataRequest request)
        {
            var wallets = _brandQueries.GetWalletTemplates(request.BrandId);
            if (wallets == null)
                throw new RegoValidationException(string.Format(ErrorMessagesEnum.WalletTemplatesMissingForThisBrand.ToString()));

            var walletsToDictionary = _brandQueries.GetWalletTemplates(request.BrandId)
                .ToDictionary(w => w.Id, w => w.Name);

            return new WalletsDataResponse
            {
                Wallets = walletsToDictionary
            };
        }

        [HttpGet]
        public async Task<BalancesResponse> Balances([FromUri]BalancesRequest request)
        {
            var balance = await _walletQueries.GetPlayerBalance(PlayerId, request.WalletId);

            return Mapper.Map<BalancesResponse>(balance);
        }

        [HttpGet]
        public async Task<BalanceSetResponse> BalancesSet()
        {
            var balanceSetResponse = new BalanceSetResponse { WalletsBalanceSet = new List<BalanceSet>() };

            var balance = await _walletQueries.GetPlayerBalance(PlayerId);
            var wagering = await _bonusApiProxy.GetWageringBalancesAsync(PlayerId);

            if (balance == null || wagering == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            var balanceSet = new BalanceSet
            {
                Main = balance.Main,
                Bonus = balance.Bonus,
                Free = balance.Free,
                Playable = balance.Playable,
                WageringCompleted = wagering.Completed,
                WageringRemaining = wagering.Remaining,
                WageringRequirement = wagering.Requirement
            };

            var balances = Mapper.Map<BalanceSet>(balanceSet);

            balanceSetResponse.WalletsBalanceSet.Add(balances);


            return balanceSetResponse;
        }

        [HttpGet]
        public PlayerData GetPlayerData([FromUri]GetPlayerDataRequest request)
        {
            var player = _queries.GetPlayerByUsername(request.PlayerName);
            if (player == null)
                throw new RegoValidationException(string.Format(ErrorMessagesEnum.PlayerWithRequestedUsernameDoesntExist.ToString()));

            return new PlayerData
            {
                FirstName = player.FirstName,
                LastName = player.LastName,
                IsFrozen = player.IsFrozen,
                CurrencyCode = player.CurrencyCode
            };
        }

        [HttpGet]
        public bool ArePlayersIdDocumentsValid()
        {
            var player = _queries.GetPlayer(PlayerId);
            if(player == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            return player.IdentityVerifications
                .Any(o => o.ExpirationDate >= DateTime.Now && o.VerificationStatus == VerificationStatus.Verified);
        }

        [AllowAnonymous]
        [HttpGet]
        public AcknowledgementData GetAcknowledgementData(Guid id)
        {
            var player = _queries.GetPlayer(id);
            if (player == null)
                throw new RegoValidationException(ErrorMessagesEnum.PlayerWithRequestedIdDoesntExist.ToString());

            var date = player.SelfExclusion.HasValue
                ? ExclusionDateHelper.GetSelfExcusionEndDate(player.SelfExclusion.Value, player.SelfExclusionDate.Value)
                : player.TimeOut.HasValue
                    ? ExclusionDateHelper.GetTimeOutEndDate(player.TimeOut.Value, player.TimeOutDate.Value)
                    : new DateTimeOffset();

            return new AcknowledgementData
            {
                Date = date.ToString("yyyy/MM/dd")
            };

        }

        [HttpGet]
        public OnSiteMessageResponse GetOnSiteMessage([FromUri]OnSiteMessageRequest request)
        {
            var message = _queries.GetOnSiteMessage(request.OnSiteMessageId);
            if (message == null)
                throw new RegoValidationException(ErrorMessagesEnum.OnSiteMessageWithRequestedIdDoesntExist.ToString());

            return new OnSiteMessageResponse
            {
                OnSiteMessage = Mapper.Map<Interface.Player.OnSiteMessage>(message)
            };
        }

        [HttpGet]
        public OnSiteMessagesResponse GetOnSiteMessages()
        {
            var messages = _queries.GetOnSiteMessages(PlayerId);
            if (messages == null)
                throw new RegoException(ErrorMessagesEnum.ServiceUnavailable.ToString());

            return new OnSiteMessagesResponse
            {
                OnSiteMessages = Mapper.Map<List<Interface.Player.OnSiteMessage>>(messages)
            };
        }

        [HttpGet]
        public OnSiteMessagesCountResponse GetOnSiteMessagesCount()
        {
            var count = _queries.GetOnSiteMessagesCount(PlayerId);

            return new OnSiteMessagesCountResponse
            {
                Count = count
            };
        }

        [HttpPost]
        [AllowAnonymous]
        public ValidationResult ValidateRegisterInfo(RegistrationData request)
        {
            var result = new ValidationResult
            {
                Errors = new Dictionary<string, string>()
            };

            var fluentValidationResult = _commands.ValidateRegisterInfo(request);
            fluentValidationResult.Errors.ForEach(o =>
            {
                result.Errors.Add(o.PropertyName, o.ErrorMessage);
            });

            return result;
        }
    }
}