using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Mappings
{
    public class VipLevelBetLimitGroupMap : EntityTypeConfiguration<Core.Game.Interface.Data.VipLevelBetLimitGroup>
    {
        public VipLevelBetLimitGroupMap(string schema)
        {
            ToTable("VipLevelBetLimitGroups", schema);

            HasKey(x => new { x.VipLevelId, x.BetLimitGroupId });

            Property(e => e.VipLevelId)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute(schema + "UX_BrandLobbies_VipLevelId_BetLimitGroupId", 1)
                        {
                            IsUnique = true
                        }));

            Property(e => e.BetLimitGroupId)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    new IndexAnnotation(
                        new IndexAttribute(schema + "UX_BrandLobbies_VipLevelId_BetLimitGroupId", 2)
                        {
                            IsUnique = true
                        }));

        }
    }
}
