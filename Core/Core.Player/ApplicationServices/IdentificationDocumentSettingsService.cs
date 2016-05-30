using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player.Events;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using IdentificationDocumentSettings = AFT.RegoV2.Core.Player.Data.IdentificationDocumentSettings;

namespace AFT.RegoV2.Core.Player.ApplicationServices
{
    public class IdentificationDocumentSettingsService : MarshalByRefObject, IApplicationService
    {
        private readonly IPlayerRepository _repository;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IEventBus _eventBus;

        public IdentificationDocumentSettingsService(
            IPlayerRepository repository,
            IActorInfoProvider actorInfoProvider,
            IEventBus eventBus)
        {
            _repository = repository;
            _actorInfoProvider = actorInfoProvider;
            _eventBus = eventBus;
        }

        #region Queries
        public IdentificationDocumentSettings GetSettingById(Guid settingId)
        {
            var setting = _repository.IdentificationDocumentSettings
                .Include(o => o.PaymentGatewayBankAccount)
                .Single(o => o.Id == settingId);

            return setting;
        }

        [Permission(Permissions.View, Module = Modules.IdentificationDocumentSettings)]
        public IQueryable<IdentificationDocumentSettings> GetSettings()
        {
            return _repository.IdentificationDocumentSettings
                .Include(o => o.Brand)
                .Include(o => o.PaymentGatewayBankAccount)
                .AsQueryable();
        }

        #endregion

        [Permission(Permissions.Create, Module = Modules.IdentificationDocumentSettings)]
        public IdentificationDocumentSettings CreateSetting(IdentificationDocumentSettingsData data)
        {
            ValidateData(data, IdentificationAction.Create);
            var setting = AutoMapper.Mapper.DynamicMap<IdentificationDocumentSettings>(data);

            setting.Id = Guid.NewGuid();
            setting.CreatedBy = _actorInfoProvider.Actor.UserName;
            setting.CreatedOn = DateTimeOffset.UtcNow;

            _repository.IdentificationDocumentSettings.Add(setting);

            _repository.SaveChanges();

            _eventBus.Publish(new IdentificationDocumentSettingsCreated(setting));

            return setting;
        }

        [Permission(Permissions.Update, Module = Modules.IdentificationDocumentSettings)]
        public IdentificationDocumentSettings UpdateSetting(IdentificationDocumentSettingsData data)
        {
            var setting = GetSettingById(data.Id);
            if (setting == null)
                throw new RegoException("Setting not found");

            ValidateData(data, IdentificationAction.Update);

            setting.BrandId = data.BrandId;
            setting.LicenseeId = data.LicenseeId;
            setting.PaymentGatewayBankAccountId = data.PaymentGatewayBankAccountId;
            setting.PaymentGatewayMethod = data.PaymentGatewayMethod;
            setting.TransactionType = data.TransactionType;
            setting.IdBack = data.IdBack;
            setting.IdFront = data.IdFront;
            setting.CreditCardBack = data.CreditCardBack;
            setting.CreditCardFront = data.CreditCardFront;
            setting.DCF = data.DCF;
            setting.POA = data.POA;
            setting.Remark = data.Remark;

            setting.UpdatedBy = _actorInfoProvider.Actor.UserName;
            setting.UpdatedOn = DateTimeOffset.UtcNow;

            _repository.SaveChanges();

            _eventBus.Publish(new IdentificationDocumentSettingsUpdated(setting));

            return setting;
        }

        private void ValidateData(IdentificationDocumentSettingsData data, IdentificationAction action)
        {
            var settings = _repository.IdentificationDocumentSettings.AsQueryable();

            if (action == IdentificationAction.Update)
                settings = settings.Where(o => o.Id != data.Id);

            if (settings.Any(
                record =>
                    record.BrandId == data.BrandId &&
                    record.PaymentGatewayBankAccountId == data.PaymentGatewayBankAccountId &&
                    record.TransactionType == data.TransactionType))
                throw new RegoException(
                    "You have already set up Identification Document Setting with the selected Brand, Payment Method and Transaction Type. Please, update the existing one or change your form data.");
        }
    }

    enum IdentificationAction
    {
        Create,
        Update
    }
}
