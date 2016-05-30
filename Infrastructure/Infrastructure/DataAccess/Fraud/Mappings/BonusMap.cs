using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class BonusMap : EntityTypeConfiguration<Core.Fraud.Interface.Data.Bonus>
    {
        public BonusMap()
        {
            ToTable("Bonuses", Configuration.Schema);
            HasKey(x => x.Id);
            Property(x => x.Code).HasMaxLength(50);
            Property(x => x.Name).HasMaxLength(50).IsRequired();
        }
    }
}
