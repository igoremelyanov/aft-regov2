using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{  
    public class PaymentSettingsCommands : IApplicationService, IPaymentSettingsCommands
    {
        private readonly IPaymentRepository _repository;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IEventBus _eventBus;
        private readonly IPaymentQueries _paymentQueries;
        public PaymentSettingsCommands(
            IPaymentRepository repository, 
            IActorInfoProvider actorInfoProvider, 
            IEventBus eventBus,
            IPaymentQueries paymentQueries
            )
        {
            _repository = repository;
            _actorInfoProvider = actorInfoProvider;
            _eventBus = eventBus;
            _paymentQueries = paymentQueries;
        }

        public Guid AddSettings(SavePaymentSettingsCommand model)
        {
            var validationResult = new SavePaymentSettingsValidator().Validate(model);
            if (!validationResult.IsValid)
            {
                throw new RegoException(validationResult.Errors.First().ErrorMessage);
            }

            if (_repository.PaymentSettings.Any(x => x.Id == model.Id))
            {
                throw new RegoException(PaymentSettingsErrors.AlreadyExistsError.ToString());
            }

            var paymentSettings = new PaymentSettings();
            paymentSettings.Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id;
            paymentSettings.BrandId = model.Brand;
            paymentSettings.CurrencyCode = model.Currency;
            paymentSettings.VipLevel = model.VipLevel;
            paymentSettings.PaymentType = model.PaymentType;
            paymentSettings.PaymentMethod = model.PaymentMethod;
            paymentSettings.PaymentGatewayMethod = model.PaymentGatewayMethod;
            paymentSettings.CreatedDate = DateTime.Now;
            paymentSettings.CreatedBy = _actorInfoProvider.Actor.UserName;
            paymentSettings.MinAmountPerTransaction = model.MinAmountPerTransaction;
            paymentSettings.MaxAmountPerTransaction = model.MaxAmountPerTransaction;
            paymentSettings.MaxAmountPerDay = model.MaxAmountPerDay;
            paymentSettings.MaxTransactionPerDay = model.MaxTransactionPerDay;
            paymentSettings.MaxTransactionPerWeek = model.MaxTransactionPerWeek;
            paymentSettings.MaxTransactionPerMonth = model.MaxTransactionPerMonth;

            paymentSettings.Enabled = Status.Inactive;

            _repository.PaymentSettings.Add(paymentSettings);
            _repository.SaveChanges();

            _eventBus.Publish(new PaymentSettingCreated
            {
                Id = paymentSettings.Id,
                CreatedBy = paymentSettings.CreatedBy,
                CreatedDate = paymentSettings.CreatedDate,
                VipLevel = paymentSettings.VipLevel,
                CurrencyCode = paymentSettings.CurrencyCode,
                BrandId = paymentSettings.BrandId
            });

            return paymentSettings.Id;
        }

        public void UpdateSettings(SavePaymentSettingsCommand model)
        {
            var validationResult = new SavePaymentSettingsValidator().Validate(model);
            if (!validationResult.IsValid)
            {
                throw new RegoException(validationResult.Errors.First().ErrorMessage);
            }

            var paymentSettings = _repository.PaymentSettings
                .SingleOrDefault(x => x.Id == model.Id);
            if (paymentSettings == null)
                throw new RegoException(string.Format("Payment settings with id '{0}' were not found", model.Id));

            paymentSettings.MinAmountPerTransaction = model.MinAmountPerTransaction;
            paymentSettings.MaxAmountPerTransaction = model.MaxAmountPerTransaction;
            paymentSettings.MaxAmountPerDay = model.MaxAmountPerDay;
            paymentSettings.MaxTransactionPerDay = model.MaxTransactionPerDay;
            paymentSettings.MaxTransactionPerWeek = model.MaxTransactionPerWeek;
            paymentSettings.MaxTransactionPerMonth = model.MaxTransactionPerMonth;
            paymentSettings.UpdatedDate = DateTime.Now;
            paymentSettings.UpdatedBy = _actorInfoProvider.Actor.UserName;

            _eventBus.Publish(new PaymentSettingUpdated
            {
                Id = paymentSettings.Id,
                UpdatedBy = paymentSettings.UpdatedBy,
                UpdatedDate = paymentSettings.UpdatedDate.GetValueOrDefault(),
                VipLevel = paymentSettings.VipLevel,
                CurrencyCode = paymentSettings.CurrencyCode,
                BrandId = paymentSettings.BrandId
            });

            _repository.SaveChanges();
        }

        [Permission(Permissions.Activate, Module = Modules.PaymentSettings)]
        public void Enable(PaymentSettingsId id, string remarks)
        {
            var paymentSettings = _repository.PaymentSettings
                .Single(x => x.Id == id);
            //TODO:AFTREGO-4143:should be mvoe to Validator
            bool isTheSameSettingsActivated;
            if (paymentSettings.PaymentGatewayMethod == PaymentMethod.Online)
            {
                isTheSameSettingsActivated = _paymentQueries.GetOnlinePaymentSettings(
                    paymentSettings.BrandId, paymentSettings.PaymentType,
                    paymentSettings.VipLevel, paymentSettings.PaymentMethod,
                    paymentSettings.CurrencyCode) !=null ;
            }
            else
            {
                isTheSameSettingsActivated = _paymentQueries.GetOfflinePaymentSettings(
                    paymentSettings.BrandId, paymentSettings.PaymentType,
                    paymentSettings.VipLevel, paymentSettings.CurrencyCode) != null;
            }
            if (isTheSameSettingsActivated)
                throw new RegoException(PaymentSettingsErrors.TheSameSettingsActivated.ToString());

            paymentSettings.Enabled = Status.Active;
            paymentSettings.EnabledDate = DateTime.Now;
            paymentSettings.EnabledBy = _actorInfoProvider.Actor.UserName;
            _repository.SaveChanges();

            _eventBus.Publish(new PaymentSettingActivated
            {
                Id = paymentSettings.Id,
                ActivatedBy = paymentSettings.UpdatedBy,
                ActivatedDate = paymentSettings.EnabledDate.GetValueOrDefault(),
                VipLevel = paymentSettings.VipLevel,
                CurrencyCode = paymentSettings.CurrencyCode,
                BrandId = paymentSettings.BrandId,
                Remarks = remarks
            });
        }

        [Permission(Permissions.Deactivate, Module = Modules.PaymentSettings)]
        public void Disable(PaymentSettingsId id, string remarks)
        {
            var paymentSettings = _repository.PaymentSettings
                .Single(x => x.Id == id);
            paymentSettings.Enabled = Status.Inactive;
            paymentSettings.DisabledDate = DateTime.Now;
            paymentSettings.DisabledBy = _actorInfoProvider.Actor.UserName;
            _repository.SaveChanges();

            _eventBus.Publish(new PaymentSettingDeactivated
            {
                Id = paymentSettings.Id,
                DeactivatedBy = paymentSettings.UpdatedBy,
                DeactivatedDate = paymentSettings.DisabledDate.GetValueOrDefault(),
                VipLevel = paymentSettings.VipLevel,
                CurrencyCode = paymentSettings.CurrencyCode,
                BrandId = paymentSettings.BrandId,
                Remarks = remarks
            });

        }
    }
}
