using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IPaymentSettingsQueries
    {
        object GetBankAccounts(Guid brandId, string currencyCode);

        object GetVipLevels(Guid? brandId);

        PaymentSettingSaveResult SaveSetting(SavePaymentSettingsCommand model);

        IEnumerable<PaymentMethodDto> GetPaymentMethods(Guid? brandId = null);
    }
}
