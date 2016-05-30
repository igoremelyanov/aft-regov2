using System;

namespace AFT.RegoV2.Core.Payment.Interface.Data.Commands
{
    public class EditBankAccountData
    {
        public Guid Id { get; set; }
        public Guid Bank { get; set; }
        public string Currency { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public Guid AccountType { get; set; }
        public string Province { get; set; }
        public string Branch { get; set; }
        public string Remarks { get; set; }

        public string SupplierName { get; set; }
        public string ContactNumber { get; set; }
        public string USBCode { get; set; }

        public string PurchasedDate { get; set; }
        public string UtilizationDate { get; set; }
        public string ExpirationDate { get; set; }

        public string IdFrontImage { get; set; }
        public string IdBackImage { get; set; }
        public string AtmCardImage { get; set; }
    }
}