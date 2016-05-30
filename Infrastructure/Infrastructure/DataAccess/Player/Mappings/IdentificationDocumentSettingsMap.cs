using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Player.Mappings
{
    public class IdentificationDocumentSettingsMap : EntityTypeConfiguration<IdentificationDocumentSettings>
    {
        public IdentificationDocumentSettingsMap()
        {
            HasKey(p => p.Id);
            HasRequired(o => o.Brand)
                .WithMany()
                .HasForeignKey(o => o.BrandId);

            HasRequired(o => o.PaymentGatewayBankAccount)
                .WithMany()
                .HasForeignKey(o => o.PaymentGatewayBankAccountId);
        }
    }
}
