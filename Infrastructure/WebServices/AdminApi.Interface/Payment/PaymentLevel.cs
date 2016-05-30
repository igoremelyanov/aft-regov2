using System;
using System.Collections.Generic;
namespace AFT.RegoV2.AdminApi.Interface.Payment
{
    #region Dto

    public class PaymentLevel
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string CurrencyCode { get; set; }
    }

    #endregion
    
    #region Request/Response

    public class GetPaymentLevelsResponse
    {
        public IEnumerable<PaymentLevel> PaymentLevels { get; set; }
    }

    #endregion

}
