using System.Data.Entity.ModelConfiguration;

namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging.Mappings
{
    public class PlayerMap : EntityTypeConfiguration<Core.Messaging.Data.Player>
    {
        public PlayerMap(string schema)
        {
            ToTable("Players", schema);
            Property(x => x.Username).IsRequired();
            Property(x => x.FirstName).IsRequired();
            Property(x => x.LastName).IsRequired();
            Property(x => x.Email).IsRequired();
            HasRequired(x => x.Language);
            HasRequired(x => x.Brand);
            HasRequired(x => x.VipLevel);
            HasOptional(x => x.PaymentLevel);
        }
    }
}