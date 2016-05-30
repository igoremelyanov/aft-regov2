using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Messaging.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging.Mappings
{
    public class MassMessageMap : EntityTypeConfiguration<MassMessage>
    {
        public MassMessageMap(string schema)
        {
            ToTable("MassMessage", schema);
            Property(x => x.AdminId).IsRequired();

            HasMany(x => x.Recipients)
                .WithMany()
                .Map(x =>
                {
                    x.MapLeftKey("MassMessageId");
                    x.MapRightKey("PlayerId");
                    x.ToTable("xref_MassMessageRecipients", schema);
                });

            HasMany(x => x.Content);
        }
    }
}