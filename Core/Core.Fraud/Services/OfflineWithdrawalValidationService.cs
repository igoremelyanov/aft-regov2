using System.Collections.Generic;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Fraud
{
    public class OfflineWithdrawalValidationService : IOfflineWithdrawalValidationService
    {
        private readonly IFundsValidationService _fundsValidationService;
        private readonly List<IWithdrawalValidationService> _withdrawalValidationServices;

        public OfflineWithdrawalValidationService(
            IPaymentSettingsValidationService paymentSettingsValidationService,
            IFundsValidationService fundsValidationService,
            IAWCValidationService awcValidationService,
            IManualAdjustmentWageringValidationService manualAdjustmentWageringValidationService,
            IRebateWageringValidationService rebateWageringValidationService,
            IBonusWageringWithdrawalValidationService bonusWageringWithdrawalValidationService)
        {
            _fundsValidationService = fundsValidationService;
            _withdrawalValidationServices = new List<IWithdrawalValidationService>
            {
                paymentSettingsValidationService,
                awcValidationService,
                manualAdjustmentWageringValidationService,
                rebateWageringValidationService,
                bonusWageringWithdrawalValidationService,
            };
        }

        public async Task Validate(OfflineWithdrawRequest request)
        {
            await _fundsValidationService.Validate(request);
            foreach (var withdrawalValidationService in _withdrawalValidationServices)
                withdrawalValidationService.Validate(request);
        }
    }
}