using System;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class AddBankModel
    {
        public Guid BrandId { get; set; }
        public string BankId { get; set; }
        public string BankName { get; set; }
        public string CountryCode { get; set; }
        public string Remarks { get; set; }
    }
}