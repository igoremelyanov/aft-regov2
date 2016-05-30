using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Bonus.Api.Interface;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player.Enums;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Entities;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Helpers;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Player.Interface.ApplicationServices;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class OnlineDepositCommands : IOnlineDepositCommands
    {
        private readonly IPaymentRepository _repository;
        private readonly IEventBus _eventBus;
        private readonly IOnlineDepositValidator _validator;
        private readonly IPaymentGatewaySettingsQueries _paymentGatewaySettingsQueries;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IServiceBus _serviceBus;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IPlayerQueries _playerQueries;
        private readonly IBonusApiProxy _bonusApiProxy;
        private const string Acknowledge = "SUCCESS";

        public OnlineDepositCommands(
            IPaymentRepository repository,
            IActorInfoProvider actorInfoProvider,
            IEventBus eventBus,
            IOnlineDepositValidator validator,
            IServiceBus serviceBus,
            IPaymentGatewaySettingsQueries paymentGatewaySettingsQueries,
            IPaymentQueries paymentQueries,
            IPlayerQueries playerQueries,
            IBonusApiProxy bonusApiProxy
            )
        {
            _repository = repository;
            _eventBus = eventBus;
            _validator = validator;
            _actorInfoProvider = actorInfoProvider;
            _paymentGatewaySettingsQueries = paymentGatewaySettingsQueries;
            _serviceBus = serviceBus;
            _paymentQueries = paymentQueries;
            _playerQueries = playerQueries;
            _bonusApiProxy = bonusApiProxy;
        }

        public async Task<SubmitOnlineDepositRequestResult> SubmitOnlineDepositRequest(OnlineDepositRequest request)
        {
            var validationResult = new OnlineDepositRequestValidator(_repository).Validate(request);

            if (!validationResult.IsValid)
            {
                throw new RegoValidationException(validationResult);
            }

            var player = _repository.Players.FirstOrDefault(x => x.Id == request.PlayerId);

            var paymentGetewaySettings = _paymentGatewaySettingsQueries.GetOnePaymentGatewaySettingsByPlayerId(player.Id);
            if (paymentGetewaySettings == null)
                throw new RegoException("PaymentGatewaySettings not found");
        
            var method = paymentGetewaySettings.OnlinePaymentMethodName;
            var channel = paymentGetewaySettings.Channel;
            var paymentUrl = paymentGetewaySettings.EntryPoint;
           
            _validator.ValidatePaymentSetting(request.PlayerId, method, request.Amount,player.CurrencyCode);

            var number = GenerateOrderId(method, channel);
            
            var merchantId = GetDepositMerchantId();

            var hashKey = GetDepositHashKey();

            var depositParams = new OnlineDepositParams();
            depositParams.Method = method;
            depositParams.Channel = channel;
            depositParams.Amount = request.Amount;
            depositParams.MerchantId = merchantId;
            depositParams.OrderId = number;
            depositParams.Currency = player.CurrencyCode;
            depositParams.Language = request.CultureCode;
            depositParams.ReturnUrl = request.ReturnUrl;
            depositParams.NotifyUrl = request.NotifyUrl;
            depositParams.Signature = EncryptHelper.GetMD5HashInHexadecimalFormat(depositParams.SignParams + hashKey);

            var id = Guid.NewGuid();

            var brandId = player.BrandId;

            var now = _paymentQueries.GetBrandDateTimeOffset(brandId);

            var onlineDepositEntity = new Entities.OnlineDeposit(id, number, request, depositParams, brandId, now);
           
            var submitEvent = onlineDepositEntity.Submit();

            using (var scope = CustomTransactionScope.GetTransactionScopeAsync())
            {
                if (!string.IsNullOrWhiteSpace(request.BonusCode) || (request.BonusId.HasValue && request.BonusId != Guid.Empty))
                {
                    var bonusRedemptionId = await _bonusApiProxy.ApplyForBonusAsync(new DepositBonusApplication
                    {
                        PlayerId = request.PlayerId,
                        BonusId = request.BonusId,
                        Amount = request.Amount,
                        DepositId = onlineDepositEntity.Data.Id,
                        BonusCode = onlineDepositEntity.Data.BonusCode
                    });
                    onlineDepositEntity.SetBonusRedemption(bonusRedemptionId);
                } 
                _repository.OnlineDeposits.Add(onlineDepositEntity.Data);

                _eventBus.Publish(submitEvent);

                _repository.SaveChanges();

                scope.Complete();
            }

            return new SubmitOnlineDepositRequestResult
            {
                DepositId = id,
                RedirectUrl = new Uri(paymentUrl),
                RedirectParams = depositParams
            };
        }

        public void ValidateOnlineDepositAmount(ValidateOnlineDepositAmountRequest request)
        {
	        var player = _playerQueries.GetPlayer(request.PlayerId);
            new OnlineDepositValidator(_paymentQueries, _repository).ValidatePaymentSetting(player.Id, _paymentQueries.GetPaymentLevel(player.PaymentLevelId).PaymentGatewaySettings.First().OnlinePaymentMethodName, request.Amount, player.CurrencyCode);
        }

        private static string GenerateOrderId(string payMethod, int channelId)
        {
            return String.Format("{0}-{1}-{2}", payMethod.Substring(0, 2), channelId.ToString(), DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
        }

        public string PayNotify(OnlineDepositPayNotifyRequest request)
        {
            var transNo = request.OrderIdOfMerchant;
            var response = "Failed";

            var hashKey = GetDepositHashKey();

            //validation
            var validationResult = new OnlineDepositPayNotifyRequestValidator(_repository, hashKey).Validate(request);

            if (!validationResult.IsValid)
            {
                throw new RegoValidationException(validationResult);
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.LockOnlineDeposit(transNo);//lock data to prevent deposit multi times

                var onlineDeposit = _repository.OnlineDeposits.First(x => x.TransactionNumber == transNo);

                var onlineDepositEntity = new Entities.OnlineDeposit(onlineDeposit);

                if (onlineDepositEntity.IsApproved() == false) //prevent approve again
                {
                    var now = _paymentQueries.GetBrandDateTimeOffset(onlineDeposit.BrandId); 
                    var depositCommand = onlineDepositEntity.Approve(request, now);

                    _repository.SaveChanges();

                    _serviceBus.PublishMessage(depositCommand);
                    scope.Complete();
                }

                response = Acknowledge;
            }

            return response;
        }

        public void Approve(ApproveOnlineDepositRequest request)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                //lock data to prevent deposit multi times
                _repository.LockOnlineDeposit(request.Id);

                var onlineDeposit = _repository.OnlineDeposits.First(x => x.Id == request.Id);

                if (onlineDeposit == null)
                    throw new RegoException(DepositErrors.NotFound.ToString());

                if (onlineDeposit.Status != OnlineDepositStatus.Verified)
                    throw new RegoException(string.Format("The deposit has '{0}' status, so it can't be Approved", onlineDeposit.Status));

                var onlineDepositEntity = new Entities.OnlineDeposit(onlineDeposit);
                var now = _paymentQueries.GetBrandDateTimeOffset(onlineDeposit.BrandId);

                var depositCommand = onlineDepositEntity.Approve(_actorInfoProvider.Actor.UserName, request.Remarks, now);
                _repository.SaveChanges();

                _serviceBus.PublishMessage(depositCommand);

                scope.Complete();
            }            
        }

        public void Verify(VerifyOnlineDepositRequest request)
        {
            var onlineDeposit = _repository.OnlineDeposits.First(x => x.Id == request.Id);

            UpdateVerificationStatus(
                onlineDeposit,
                VerificationStatus.Verified,
                null,
                request.Remarks);
        }

        public void Unverify(UnverifyOnlineDepositRequest request)
        {
            var onlineDeposit = _repository.OnlineDeposits.First(x => x.Id == request.Id);

            UpdateVerificationStatus(
                onlineDeposit, 
                VerificationStatus.Unverified,
                request.UnverifyReason, 
                request.Remarks);
        }

        private void UpdateVerificationStatus(Data.OnlineDeposit onlineDeposit, VerificationStatus status, UnverifyReasons? unverifyReason, string remarks)
        {
            if (onlineDeposit == null)
                throw new RegoException(DepositErrors.NotFound.ToString());

            var onlineDepositEntity = new Entities.OnlineDeposit(onlineDeposit);

            var now = _paymentQueries.GetBrandDateTimeOffset(onlineDeposit.BrandId);

            DomainEventBase eventCreated = null;

            if (status == VerificationStatus.Unverified)
                eventCreated = onlineDepositEntity.Unverify(_actorInfoProvider.Actor.UserName, remarks, now, unverifyReason.Value);
            if(status == VerificationStatus.Verified)
                eventCreated = onlineDepositEntity.Verify(_actorInfoProvider.Actor.UserName, remarks, now);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.SaveChanges();
                _eventBus.Publish(eventCreated);
                scope.Complete();
            }
        }

        public void Reject(RejectOnlineDepositRequest request)
        {

            var onlineDeposit = _repository.OnlineDeposits.First(x => x.Id == request.Id);

            if (onlineDeposit == null)
                throw new RegoException(DepositErrors.NotFound.ToString());

            var onlineDepositEntity = new Entities.OnlineDeposit(onlineDeposit);

            var now = _paymentQueries.GetBrandDateTimeOffset(onlineDeposit.BrandId);

            var rejectedEvent = onlineDepositEntity.Reject(_actorInfoProvider.Actor.UserName, request.Remarks, now);
            
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.SaveChanges();

                _eventBus.Publish(rejectedEvent);

                scope.Complete();
            }
        }

        #region private Methods
        private string GetDepositMerchantId()
        {
            var merchantId = "merchantId";
            if (ConfigurationManager.AppSettings["OnlineDepositMerchantId"] != null)
                merchantId = ConfigurationManager.AppSettings["OnlineDepositMerchantId"];
            return merchantId;
        }

        private string GetDepositHashKey()
        {
            var hashKey = "testkey";
            if (ConfigurationManager.AppSettings["OnlineDepositKey"] != null)
                hashKey = ConfigurationManager.AppSettings["OnlineDepositKey"];
            return hashKey;
        }
        #endregion 
    }
}