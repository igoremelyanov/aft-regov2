using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Brand.Interface.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings
{
    public class VipLevelGameProviderBetLimitMap : EntityTypeConfiguration<VipLevelGameProviderBetLimit>
    {
        public VipLevelGameProviderBetLimitMap(string schema)
        {
            ToTable("xref_VipLevelGameProviderBetLimitMap", schema);
            HasKey(x => x.Id);
            HasRequired(x => x.VipLevel).WithMany(x => x.VipLevelGameProviderBetLimits).WillCascadeOnDelete(false);
            HasRequired(x => x.Currency).WithMany().HasForeignKey(x => x.CurrencyCode);
            Property(x => x.GameProviderId).IsRequired();
            Property(x => x.BetLimitId).IsRequired();
        }
    }
}