using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Payment.Interface.Data;
using AFT.RegoV2.Core.Payment.Interface.Data.Commands;

namespace AFT.RegoV2.Core.Payment.Interface.ApplicationServices
{
    public interface IPaymentLevelQueries
    {
        IEnumerable<string> GetBrandCurrencies(Guid brandId);
        PaymentLevelTransferObj GetPaymentLevelById(Guid id);
        IQueryable<PaymentLevel> GetReplacementPaymentLevels(Guid id);
        DeactivatePaymentLevelStatus GetDeactivatePaymentLevelStatus(Guid id);
        IQueryable<PaymentLevel> GetPaymentLevelsByBrandAndCurrency(Guid brandId, string currencyCode);
    }
}
