using System;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class EditBankModel
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string BankName { get; set; }
        public string CountryCode { get; set; }
        public string Remarks { get; set; }
    }
}