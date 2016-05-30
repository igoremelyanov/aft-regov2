using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class AutoVerificationCheckConfigurationMap : EntityTypeConfiguration<AutoVerificationCheckConfiguration>
    {
        public AutoVerificationCheckConfigurationMap()
        {
            ToTable("AutoVerificationCheckConfigurations", Configuration.Schema);
            HasKey(x => x.Id);
            HasRequired(x => x.Brand).WithMany(x => x.AutoVerificationCheckConfigurations);
            HasMany(x => x.AllowedRiskLevels).WithMany(x => x.AutoVerificationCheckConfigurations).Map(x =>
            {
                x.MapLeftKey("AutoVerificationCheckConfigurationId");
                x.MapRightKey("RiskLevelId");
                x.ToTable("AutoVerificationCheckConfigurationsRiskLevels", Configuration.Schema);
            });

            HasMany(x => x.PaymentLevels).WithMany(x => x.AutoVerificationCheckConfigurations).Map(x =>
            {
                x.MapLeftKey("AutoVerificationCheckConfigurationId");
                x.MapRightKey("PaymentLevelId");
                x.ToTable("AutoVerificationCheckConfigurationsPaymentLevels", Configuration.Schema);
            });
            
            HasMany(x => x.WinningRules)/*.WithRequired(rule => rule.AutoVerificationCheckConfiguration).Map(x=>
            {
                x.MapKey("AutoVerificationCheckConfigurationId");
            })*/;
        }
    }
}
