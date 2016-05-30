using System.Data.Entity.ModelConfiguration;

using FakeUGS.Core.Data;

namespace FakeUGS.Core.DataAccess.Mappings
{
    public class BrandMap : EntityTypeConfiguration<Brand>
    {
        public BrandMap(string schema)
        {
            ToTable("Brands", schema);
        }
    }
}
