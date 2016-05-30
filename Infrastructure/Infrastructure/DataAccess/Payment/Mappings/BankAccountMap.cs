using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class BankAccountMap : EntityTypeConfiguration<BankAccount>
    {
        public BankAccountMap()
        {
            ToTable("BankAccounts", Configuration.Schema);
            HasKey(p => p.Id);
            Property(p => p.AccountId);
            Property(p => p.AccountName);
            Property(p => p.AccountNumber);
            Property(p => p.CurrencyCode);
            Property(p => p.Province);
            Property(p => p.Branch);
            Property(p => p.Created);
            Property(p => p.CreatedBy);
            Property(p => p.Status);
            Property(p => p.Updated);
            Property(p => p.UpdatedBy);
            Property(p => p.SupplierName);
            Property(p => p.ContactNumber);
            Property(p => p.USBCode);
            Property(p => p.PurchasedDate);
            Property(p => p.UtilizationDate);
            Property(p => p.ExpirationDate);
            Property(p => p.IdFrontImage);
            Property(p => p.IdBackImage);
            Property(p => p.ATMCardImage);
            HasRequired(p => p.Bank).WithMany().WillCascadeOnDelete(false);
            HasRequired(p => p.AccountType).WithMany().WillCascadeOnDelete(false);
        }
    }
}
