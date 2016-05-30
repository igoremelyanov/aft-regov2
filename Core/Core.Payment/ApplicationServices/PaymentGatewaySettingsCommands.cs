using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using FluentValidation.Results;
using PaymentGatewaySettings = AFT.RegoV2.Core.Payment.Data.PaymentGatewaySettings;
namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class PaymentGatewaySettingsCommands : IApplicationService, IPaymentGatewaySettingsCommands
    {                     
        private readonly IPaymentRepository _repository;        
        private readonly IEventBus _eventBus;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IPaymentQueries _paymentQueries;

        public PaymentGatewaySettingsCommands(IPaymentRepository repository,IActorInfoProvider actorInfoProvider, IEventBus eventBus
            , IPaymentQueries paymentQueries)
        {
            _repository = repository;
            _eventBus = eventBus;            
            _actorInfoProvider = actorInfoProvider;
            _paymentQueries = paymentQueries;
        }
             
        public SavePaymentGatewaysSettingsResult Add(SavePaymentGatewaysSettingsData model)
        {
            var validationResult = new SavePaymentGatewaySettingsValidator(_repository,true).Validate(model);

            if (!validationResult.IsValid)
            {
                throw new RegoValidationException(validationResult);
            }

            var setting = new PaymentGatewaySettings
            {
                Id = Guid.NewGuid(),
                BrandId = model.Brand,
                OnlinePaymentMethodName = model.OnlinePaymentMethodName,
                PaymentGatewayName = model.PaymentGatewayName,
                Channel = model.Channel,
                EntryPoint = model.EntryPoint,
                Remarks = model.Remarks,
                Status = Status.Inactive,
                DateCreated = _paymentQueries.GetBrandDateTimeOffset(model.Brand),
                CreatedBy = _actorInfoProvider.Actor.UserName
            };
            _repository.PaymentGatewaySettings.Add(setting);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.SaveChanges();

                _eventBus.Publish(new PaymentGatewaySettingCreated
                {
                    Id = setting.Id,
                    CreatedBy = setting.CreatedBy,
                    CreatedDate = setting.DateCreated,
                    OnlinePaymentMethodName = setting.OnlinePaymentMethodName,
                    PaymentGatewayName = setting.PaymentGatewayName,
                    Channel = setting.Channel,
                    EntryPoint = setting.EntryPoint,
                    BrandId = setting.BrandId
                });

                scope.Complete();
            }

            return new SavePaymentGatewaysSettingsResult
            {
                PaymentGatewaySettingsId = setting.Id
            };
        }

        public SavePaymentGatewaysSettingsResult Edit(SavePaymentGatewaysSettingsData model)
        {
            var validationResult = new SavePaymentGatewaySettingsValidator(_repository, false).Validate(model);
            if (!validationResult.IsValid)
            {
                throw new RegoValidationException(validationResult);
            }

            var setting = _repository.PaymentGatewaySettings
                .SingleOrDefault(x => x.Id == model.Id);

            if (setting == null)
                throw new RegoException(string.Format("Payment gateway settings with id '{0}' were not found", model.Id));
         
            setting.OnlinePaymentMethodName = model.OnlinePaymentMethodName;
            setting.PaymentGatewayName = model.PaymentGatewayName;
            setting.Channel = model.Channel;
            setting.EntryPoint = model.EntryPoint;
            setting.Remarks = model.Remarks;
            setting.DateUpdated = _paymentQueries.GetBrandDateTimeOffset(model.Brand);
            setting.UpdatedBy = _actorInfoProvider.Actor.UserName;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.SaveChanges();

                _eventBus.Publish(new PaymentGatewaySettingUpdated
                {
                    Id = setting.Id,
                    UpdatedBy = setting.UpdatedBy,
                    UpdatedDate = setting.DateUpdated.GetValueOrDefault(),
                    OnlinePaymentMethodName = setting.OnlinePaymentMethodName,
                    PaymentGatewayName = setting.PaymentGatewayName,
                    Channel = setting.Channel,
                    EntryPoint = setting.EntryPoint,
                    BrandId = setting.BrandId
                });
                
                scope.Complete();
            }
            
            return new SavePaymentGatewaysSettingsResult
            {
                PaymentGatewaySettingsId = setting.Id
            };
        }
        
        public void Activate(ActivatePaymentGatewaySettingsData model)
        {
            var setting = _repository.PaymentGatewaySettings             
             .Single(x => x.Id == model.Id);

            setting.Status = Status.Active;
            setting.DateActivated = _paymentQueries.GetBrandDateTimeOffset(setting.BrandId);
            setting.ActivatedBy = _actorInfoProvider.Actor.UserName;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.SaveChanges();

                _eventBus.Publish(new PaymentGatewaySettingActivated
                {
                    Id = setting.Id,
                    ActivatedBy = setting.ActivatedBy,
                    ActivatedDate = setting.DateActivated.GetValueOrDefault(),
                    OnlinePaymentMethodName = setting.OnlinePaymentMethodName,
                    PaymentGatewayName = setting.PaymentGatewayName,
                    Channel = setting.Channel,
                    EntryPoint = setting.EntryPoint,
                    BrandId = setting.BrandId,
                    Remarks = model.Remarks
                });

                scope.Complete();
            }
        }

        public void Deactivate(DeactivatePaymentGatewaySettingsData model)
        {
            var setting = _repository.PaymentGatewaySettings
            .Single(x => x.Id == model.Id);

            setting.Status = Status.Inactive;
            setting.DateDeactivated = _paymentQueries.GetBrandDateTimeOffset(setting.BrandId);
            setting.DeactivatedBy = _actorInfoProvider.Actor.UserName;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.SaveChanges();

                _eventBus.Publish(new PaymentGatewaySettingDeactivated
                {
                    Id = setting.Id,
                    DeactivatedBy = setting.DeactivatedBy,
                    DeactivatedDate = setting.DateDeactivated.GetValueOrDefault(),
                    OnlinePaymentMethodName = setting.OnlinePaymentMethodName,
                    PaymentGatewayName = setting.PaymentGatewayName,
                    Channel = setting.Channel,
                    EntryPoint = setting.EntryPoint,
                    BrandId = setting.BrandId,
                    Remarks = model.Remarks
                });

                scope.Complete();
            }
        }


        public ValidationResult ValidateThatPaymentGatewaySettingsCanBeAdded(SavePaymentGatewaysSettingsData model)
        {
            var validator = new SavePaymentGatewaySettingsValidator(_repository,true);
            return validator.Validate(model);
        }

        public ValidationResult ValidateThatPaymentGatewaySettingsCanBeEdited(SavePaymentGatewaysSettingsData model)
        {
            var validator = new SavePaymentGatewaySettingsValidator(_repository,false);
            return validator.Validate(model);
        }

        public ValidationResult ValidateThatPaymentGatewaySettingsCanBeActivated(ActivatePaymentGatewaySettingsData model)
        {
            var validator = new ActivatePaymentGatewaySettingsValidator(_repository);
            return validator.Validate(model);
        }

        public ValidationResult ValidateThatPaymentGatewaySettingsCanBeDeactivated(DeactivatePaymentGatewaySettingsData model)
        {
            var validator = new DeactivatePaymentGatewaySettingsValidator(_repository);
            return validator.Validate(model);
        }
    }
}
