using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Messaging.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging.Mappings
{
    public class MassMessageContentMap : EntityTypeConfiguration<MassMessageContent>
    {
        public MassMessageContentMap(string schema)
        {
            ToTable("MassMessageContent", schema);

            HasRequired(x => x.MassMessage);
            HasRequired(x => x.Language);
            Property(x => x.MassMessageType).IsRequired();
            Property(x => x.Subject);
            Property(x => x.Content).IsRequired();
        }
    }
}