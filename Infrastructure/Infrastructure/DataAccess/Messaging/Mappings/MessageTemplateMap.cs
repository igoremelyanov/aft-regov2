using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Messaging.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging.Mappings
{
    public class MessageTemplateMap : EntityTypeConfiguration<MessageTemplate>
    {
        public MessageTemplateMap(string schema)
        {
            ToTable("MessageTemplates", schema);
        }
    }
}