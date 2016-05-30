using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class PaymentMethodMap : EntityTypeConfiguration<PaymentMethod>
    {
        public PaymentMethodMap()
        {
            ToTable("PaymentMethods", Configuration.Schema);
            HasKey(x => x.Key);
            Property(x => x.Code).HasMaxLength(50).IsRequired();
        }
    }
}
