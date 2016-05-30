using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data.Users;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings
{
    public class AdminMap : EntityTypeConfiguration<Admin>
    {
        public AdminMap(string schema)
        {
            ToTable("Admins", schema);
            HasKey(u => u.Id);
            Property(u => u.Username).IsRequired().HasMaxLength(255)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Username") { IsUnique = true })); // making it unique
            HasRequired(u => u.Role);
        }
    }
}
