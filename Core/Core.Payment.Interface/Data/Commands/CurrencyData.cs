using System;

namespace AFT.RegoV2.Core.Payment.Interface.Data.Commands
{
    public class SaveCurrencyExchangeData
    {
        public Guid BrandId { get; set; }
        public string Currency { get; set; }
        public decimal CurrentRate { get; set; }
        public decimal PreviousRate { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
    }

    public class CurrencyCRUDStatus
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
