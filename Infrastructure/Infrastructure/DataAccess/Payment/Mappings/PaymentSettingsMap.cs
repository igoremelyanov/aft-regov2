using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class PaymentSettingsMap : EntityTypeConfiguration<PaymentSettings>
    {
        public PaymentSettingsMap()
        {
            ToTable("PaymentSettings", Configuration.Schema);
            HasKey(pl => pl.Id);
            Property(p => p.BrandId).HasColumnName("Brand_Id");
            HasRequired(pl => pl.Brand).WithMany().WillCascadeOnDelete(false);
            Property(x => x.PaymentType);
            Property(x => x.VipLevel);
            Property(x => x.CurrencyCode);
            Property(x => x.MinAmountPerTransaction);
            Property(x => x.MaxAmountPerTransaction);
            Property(x => x.MaxAmountPerDay);
            Property(x => x.MaxTransactionPerDay);
            Property(x => x.MaxTransactionPerWeek);
            Property(x => x.MaxTransactionPerMonth);
            Property(x => x.Enabled);
            Property(x => x.CreatedBy);
            Property(x => x.CreatedDate);
            Property(x => x.UpdatedBy);
            Property(x => x.UpdatedDate);
            Property(x => x.EnabledBy);
            Property(x => x.EnabledDate);
            Property(x => x.DisabledBy);
            Property(x => x.DisabledDate);
        }
    }
}