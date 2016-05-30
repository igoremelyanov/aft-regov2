using System;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Payment.Interface.ApplicationServices;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class TransferSettingsCommands : IApplicationService, ITransferSettingsCommands
    {
        private readonly IPaymentRepository _repository;
        private readonly IEventBus _eventBus;
        private readonly IActorInfoProvider _actorInfoProvider;

        public TransferSettingsCommands(
            IPaymentRepository repository,
            IEventBus eventBus,
            IActorInfoProvider actorInfoProvider)
        {
            _repository = repository;
            _eventBus = eventBus;
            _actorInfoProvider = actorInfoProvider;
        }

        [Permission(Permissions.Create, Module = Modules.TransferSettings)]
        public Guid AddSettings(SaveTransferSettingsCommand model)
        {
            var validationResult = new SaveTransferSettingsValidator().Validate(model);
            if (!validationResult.IsValid)
            {
                throw new RegoException(validationResult.Errors.First().ErrorMessage);
            }

            if (_repository.TransferSettings.Any(x => x.Id == model.Id))
            {
                throw new RegoException(TransferFundSettingsErrors.AlreadyExistsError.ToString());
            }

            if (_repository.TransferSettings.Any(
                x => x.BrandId == model.Brand
                    && x.VipLevelId == model.VipLevel
                    && x.TransferType == model.TransferType
                    && x.CurrencyCode == model.Currency
                    && x.WalletId == model.Wallet))
            {
                throw new RegoException(TransferFundSettingsErrors.AlreadyExistsError.ToString());
            }

            var transferSettings = new AFT.RegoV2.Core.Payment.Data.TransferSettings();
            transferSettings.Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id;
            transferSettings.BrandId = model.Brand;
            transferSettings.CurrencyCode = model.Currency;
            transferSettings.VipLevelId = model.VipLevel;
            transferSettings.TransferType = model.TransferType;
            transferSettings.WalletId = model.Wallet;
            transferSettings.CreatedDate = DateTimeOffset.Now.ToBrandOffset(model.TimezoneId);
            transferSettings.CreatedBy = _actorInfoProvider.Actor.UserName;
            transferSettings.MinAmountPerTransaction = model.MinAmountPerTransaction;
            transferSettings.MaxAmountPerTransaction = model.MaxAmountPerTransaction;
            transferSettings.MaxAmountPerDay = model.MaxAmountPerDay;
            transferSettings.MaxTransactionPerDay = model.MaxTransactionPerDay;
            transferSettings.MaxTransactionPerWeek = model.MaxTransactionPerWeek;
            transferSettings.MaxTransactionPerMonth = model.MaxTransactionPerMonth;
            _repository.TransferSettings.Add(transferSettings);
            _repository.SaveChanges();

            _eventBus.Publish(new TransferFundSettingsCreated
            {
                TransferSettingsId = transferSettings.Id,
                CreatedBy = transferSettings.CreatedBy,
                Created = transferSettings.CreatedDate
            });

            return transferSettings.Id;
        }

        [Permission(Permissions.Update, Module = Modules.TransferSettings)]
        public void UpdateSettings(SaveTransferSettingsCommand model)
        {
            var validationResult = new SaveTransferSettingsValidator().Validate(model);
            if (!validationResult.IsValid)
            {
                throw new RegoException(validationResult.Errors.First().ErrorMessage);
            }

            var transferSettings = _repository.TransferSettings.SingleOrDefault(x => x.Id == model.Id);
            if (transferSettings == null)
                throw new RegoException(string.Format("Unable to find Transfer Settings with Id '{0}'", model.Id));

            transferSettings.MinAmountPerTransaction = model.MinAmountPerTransaction;
            transferSettings.MaxAmountPerTransaction = model.MaxAmountPerTransaction;
            transferSettings.MaxAmountPerDay = model.MaxAmountPerDay;
            transferSettings.MaxTransactionPerDay = model.MaxTransactionPerDay;
            transferSettings.MaxTransactionPerWeek = model.MaxTransactionPerWeek;
            transferSettings.MaxTransactionPerMonth = model.MaxTransactionPerMonth;
            transferSettings.UpdatedDate = DateTimeOffset.Now.ToBrandOffset(model.TimezoneId);
            transferSettings.UpdatedBy = _actorInfoProvider.Actor.UserName;
            _repository.SaveChanges();

            _eventBus.Publish(new TransferFundSettingsUpdated
            {
                TransferSettingsId = transferSettings.Id,
                UpdatedBy = transferSettings.UpdatedBy,
                Updated = transferSettings.UpdatedDate
            });
        }

        [Permission(Permissions.Activate, Module = Modules.TransferSettings)]
        public void Enable(TransferSettingsId id, string timezoneId, string remarks)
        {
            var transferSettings = _repository.TransferSettings.Single(x => x.Id == id);
            transferSettings.Enabled = true;
            transferSettings.EnabledDate = DateTimeOffset.Now.ToBrandOffset(timezoneId);
            transferSettings.EnabledBy = _actorInfoProvider.Actor.UserName;
            _repository.SaveChanges();

            _eventBus.Publish(new TransferFundSettingsActivated
            {
                TransferSettingsId = transferSettings.Id,
                EnabledBy = transferSettings.EnabledBy,
                Activated = transferSettings.EnabledDate,
                Remarks = remarks
            });
        }

        [Permission(Permissions.Deactivate, Module = Modules.TransferSettings)]
        public void Disable(TransferSettingsId id, string timezoneId, string remarks)
        {
            var transferSettings = _repository.TransferSettings.Single(x => x.Id == id);
            transferSettings.Enabled = false;
            transferSettings.DisabledDate = DateTimeOffset.Now.ToBrandOffset(timezoneId);
            transferSettings.DisabledBy = _actorInfoProvider.Actor.UserName;
            _repository.SaveChanges();

            _eventBus.Publish(new TransferFundSettingsDeactivated
            {
                TransferSettingsId = transferSettings.Id,
                DisabledBy = transferSettings.DisabledBy,
                Deactivated = transferSettings.DisabledDate,
                Remarks = remarks
            });
        }
    }
}