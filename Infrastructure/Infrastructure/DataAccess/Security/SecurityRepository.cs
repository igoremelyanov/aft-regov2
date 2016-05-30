using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Data.Users;
using AFT.RegoV2.Core.Security.Interface.Data;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Security.Mappings;
using AFT.RegoV2.Infrastructure.DataAccess.Security.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security
{
    public class SecurityRepository : DbContext, ISecurityRepository
    {
        private const string Schema = "security";

        static SecurityRepository()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<SecurityRepository, Configuration>());
        }

        public SecurityRepository() : base("name=Default") { }

        public IDbSet<Role> Roles { get; set; }
        public IDbSet<Admin> Admins { get; set; }
        public IDbSet<AdminIpRegulation> AdminIpRegulations { get; set; }
        public IDbSet<AdminIpRegulationSetting> AdminIpRegulationSettings { get; set; }
        public IDbSet<BrandIpRegulation> BrandIpRegulations { get; set; }
        public IDbSet<Error> Errors { get; set; }
        public IDbSet<Core.Security.Data.Brand> Brands { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Configurations.Add(new RoleMap(Schema));
            modelBuilder.Configurations.Add(new AdminMap(Schema));
            modelBuilder.Configurations.Add(new BrandIdMap(Schema));
            modelBuilder.Configurations.Add(new RoleLicenseeIdMap(Schema));
            modelBuilder.Configurations.Add(new LicenseeIdMap(Schema));
            modelBuilder.Configurations.Add(new CurrencyCodeMap(Schema));
            modelBuilder.Configurations.Add(new ErrorMap(Schema));
            modelBuilder.Configurations.Add(new BrandFilterSelectionMap(Schema));
            modelBuilder.Configurations.Add(new LicenseeFilterSelectionMap(Schema));
            modelBuilder.Configurations.Add(new BrandMap(Schema));
            modelBuilder.Entity<AdminIpRegulation>().ToTable("AdminIpRegulation", Schema);
            modelBuilder.Entity<BrandIpRegulation>().ToTable("BrandIpRegulation", Schema);
            modelBuilder.Entity<AdminIpRegulationSetting>().ToTable("AdminIpRegulationSettings", Schema);
        }

        public Admin GetAdminById(Guid userId)
        {
            return Admins
                .Include(u => u.Role)
                .Include(u => u.Licensees)
                .Include(u => u.AllowedBrands)
                .Include(u => u.BrandFilterSelections)
                .Include(u => u.Currencies)
                .SingleOrDefault(u => u.Id == userId);
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }
    }
}
