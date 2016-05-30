using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Interface.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Mappings
{
    public class SignUpFraudTypeMap : EntityTypeConfiguration<SignUpFraudType>
    {
        public SignUpFraudTypeMap()
        {
            ToTable("SignUpFraudTypes", Configuration.Schema);
            HasKey(x => x.Id);
            HasMany(o=>o.RiskLevels)
                .WithMany(o=>o.SignUpFraudTypes)
                .Map(cs =>
                        {
                            cs.MapLeftKey("SignUpFraudTypeId");
                            cs.MapRightKey("RiskLevelId");
                            cs.ToTable("SignUpFraudTypesRiskLevels", Configuration.Schema);
                        });
        }
    }
}
