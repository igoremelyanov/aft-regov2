using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Fraud.Interface.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Core.Fraud.Validations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using WagerConfigurationDTO = AFT.RegoV2.Core.Fraud.Interface.Data.WagerConfigurationDTO;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class WagerConfigurationCommands : MarshalByRefObject, IWagerConfigurationCommands, IApplicationService
    {
        #region Fields

        private readonly IFraudRepository _fraudRepository;

        #endregion

        #region Constructors

        public WagerConfigurationCommands(IFraudRepository fraudRepository)
        {
            _fraudRepository = fraudRepository;
        }

        #endregion

        #region Methods

        [Permission(Permissions.Activate, Module = Modules.WagerConfiguration)]
        public void ActivateWagerConfiguration(WagerConfigurationId wagerId, Guid userId)
        {
            var wagerConfiguration = _fraudRepository.WagerConfigurations.FirstOrDefault(x => x.Id == wagerId);
            if (wagerConfiguration == null)
                return;
            if (wagerConfiguration.IsActive)
                return;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                wagerConfiguration.IsActive = true;
                wagerConfiguration.ActivatedBy = userId;
                wagerConfiguration.DateActivated = DateTimeOffset.UtcNow;

                _fraudRepository.SaveChanges();
                scope.Complete();
            }
        }

        [Permission(Permissions.Create, Module = Modules.WagerConfiguration)]
        public Guid CreateWagerConfiguration(WagerConfigurationDTO wagerConfigurationDTO, Guid userId)
        {
            var validationResult = new CreateWageringConfigurationValidator(_fraudRepository)
                .Validate(wagerConfigurationDTO);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var wagerConfiguration = new WagerConfiguration();
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {

                wagerConfiguration.Id = Guid.NewGuid();
                wagerConfiguration.IsActive = false;
                wagerConfiguration.BrandId = wagerConfigurationDTO.BrandId;
                wagerConfiguration.IsDepositWageringCheck = wagerConfigurationDTO.IsDepositWageringCheck;
                wagerConfiguration.IsRebateWageringCheck = wagerConfigurationDTO.IsRebateWageringCheck;
                wagerConfiguration.IsManualAdjustmentWageringCheck = wagerConfigurationDTO.IsManualAdjustmentWageringCheck;
                wagerConfiguration.CurrencyCode = wagerConfigurationDTO.Currency;

                wagerConfiguration.IsServeAllCurrencies = wagerConfiguration.CurrencyCode == "All";
                wagerConfiguration.CreatedBy = userId;
                wagerConfiguration.DateCreated = DateTimeOffset.UtcNow;

                _fraudRepository.WagerConfigurations.Add(wagerConfiguration);
                _fraudRepository.SaveChanges();
                scope.Complete();
            }
            return wagerConfiguration.Id;
        }

        [Permission(Permissions.Update, Module = Modules.WagerConfiguration)]
        public Guid UpdateWagerConfiguration(WagerConfigurationDTO wagerConfigurationDTO, Guid userId)
        {
            var validationResult = new UpdateWageringConfigurationValidator(_fraudRepository)
                .Validate(wagerConfigurationDTO);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            var wagerConfiguration = _fraudRepository.WagerConfigurations.First(x => x.Id == wagerConfigurationDTO.Id);
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {

                wagerConfiguration.IsActive = false;
                wagerConfiguration.BrandId = wagerConfigurationDTO.BrandId;
                wagerConfiguration.IsDepositWageringCheck = wagerConfigurationDTO.IsDepositWageringCheck;
                wagerConfiguration.IsRebateWageringCheck = wagerConfigurationDTO.IsRebateWageringCheck;
                wagerConfiguration.IsManualAdjustmentWageringCheck = wagerConfigurationDTO.IsManualAdjustmentWageringCheck;
                wagerConfiguration.CurrencyCode = wagerConfigurationDTO.Currency;
                wagerConfiguration.IsServeAllCurrencies = wagerConfiguration.CurrencyCode == "All";
                wagerConfiguration.UpdatedBy = userId;
                wagerConfiguration.DateUpdated = DateTimeOffset.UtcNow;

                _fraudRepository.SaveChanges();
                scope.Complete();
            }
            return wagerConfiguration.Id;
        }

        [Permission(Permissions.Deactivate, Module = Modules.WagerConfiguration)]
        public void DeactivateWagerConfiguration(WagerConfigurationId wagerId, Guid userId)
        {
            var wagerConfiguration = _fraudRepository.WagerConfigurations.FirstOrDefault(x => x.Id == wagerId);
            if (wagerConfiguration == null)
                return;
            if (!wagerConfiguration.IsActive)
                return;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                wagerConfiguration.IsActive = false;
                wagerConfiguration.DeactivatedBy = userId;
                wagerConfiguration.DateDeactivated = DateTimeOffset.UtcNow;

                _fraudRepository.SaveChanges();
                scope.Complete();
            }
        }

        #endregion
    }
}