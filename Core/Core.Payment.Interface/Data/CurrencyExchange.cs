using System;

namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class CurrencyExchange
    {
        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }
        
        public string CurrencyToCode { get; set; }
        public Currency CurrencyTo { get; set; }

        public decimal CurrentRate { get; set; }

        public decimal? PreviousRate { get; set; }

        //commented for R.01
        //[DefaultValue(false)]
        //public bool IsLiveRate { get; set; }

        public bool IsBaseCurrency { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
    }
}