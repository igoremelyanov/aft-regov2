using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings
{
    public class BrandMap : EntityTypeConfiguration<Core.Security.Data.Brand>
    {
        public BrandMap(string schema)
        {
            ToTable("Brands", schema);
            HasKey(u => u.Id);
        }
    }
}
