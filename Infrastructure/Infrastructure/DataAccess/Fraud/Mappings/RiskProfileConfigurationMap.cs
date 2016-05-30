using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class RiskProfileConfigurationMap : EntityTypeConfiguration<RiskProfileConfiguration>
    {
        public RiskProfileConfigurationMap()
        {
            ToTable("RiskProfileConfigurations", Configuration.Schema);
            HasKey(x => x.Id);
            HasRequired(x => x.Brand).WithMany();

            HasMany(x => x.AllowedRiskLevels).WithMany().Map(x =>
            {
                x.MapLeftKey("RiskProfileConfigurationId");
                x.MapRightKey("RiskLevelId");
                x.ToTable("RiskProfileConfigurationsRiskLevels", Configuration.Schema);
            });

            HasMany(x => x.AllowedBonuses).WithMany().Map(x =>
            {
                x.MapLeftKey("RiskProfileConfigurationId");
                x.MapRightKey("BonusId");
                x.ToTable("RiskProfileConfigurationsBonuses", Configuration.Schema);
            });

            HasMany(x => x.AllowedPaymentMethods).WithMany().Map(x =>
            {
                x.MapLeftKey("RiskProfileConfigurationId");
                x.MapRightKey("PaymentMethodId");
                x.ToTable("RiskProfileConfigurationsPaymentMethods", Configuration.Schema);
            });
        }
    }
}
