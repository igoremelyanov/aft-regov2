using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Messaging.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging.Mappings
{
    public class LanguageMap : EntityTypeConfiguration<Language>
    {
        public LanguageMap(string schema)
        {
            ToTable("Languages", schema);

            HasKey(x => x.Code);
        }
    }
}