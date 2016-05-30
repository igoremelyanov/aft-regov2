using System.Data.Entity.ModelConfiguration;

namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging.Mappings
{
    public class PaymentLevelMap : EntityTypeConfiguration<Core.Messaging.Data.PaymentLevel>
    {
        public PaymentLevelMap(string schema)
        {
            ToTable("PaymentLevels", schema);
        }
    }
}