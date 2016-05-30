using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings
{
    public class PaymentGatewaySettingsMap : EntityTypeConfiguration<PaymentGatewaySettings>
    {
        public PaymentGatewaySettingsMap()
        {
            ToTable("PaymentGatewaySettings", Configuration.Schema);
            HasKey(pl => pl.Id);            
            HasRequired(pl => pl.Brand).WithMany().WillCascadeOnDelete(false);        
        }
    }
}