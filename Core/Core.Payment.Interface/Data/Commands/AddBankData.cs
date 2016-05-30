using System;

namespace AFT.RegoV2.Core.Payment.Interface.Data.Commands
{
    public class AddBankData
    {
        public Guid? Id { get; set; }
        public Guid BrandId { get; set; }
        public string BankId { get; set; }
        public string BankName { get; set; }
        public string CountryCode { get; set; }
        public string Remarks { get; set; }
    }
}