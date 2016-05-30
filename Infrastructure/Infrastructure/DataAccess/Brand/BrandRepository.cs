using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.Interface.Data;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Brand.Mappings;
using VipLevel = AFT.RegoV2.Core.Brand.Interface.Data.VipLevel;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand
{
    public class BrandRepository : DbContext, IBrandRepository
    {
        public const string Schema = "brand";

        static BrandRepository()
        {
            Database.SetInitializer(new BrandRepositoryInitializer());
        }

        public BrandRepository(): base("name=Default") { }

        public IDbSet<Country>                  Countries { get; set; }
        public IDbSet<Currency>                 Currencies { get; set; }
        public IDbSet<Culture>                  Cultures { get; set; }
        public IDbSet<Licensee>                 Licensees { get; set; }
        public IDbSet<Core.Brand.Interface.Data.Brand>    Brands { get; set; }
        public IDbSet<VipLevel>                 VipLevels { get; set; }
        public IDbSet<VipLevelGameProviderBetLimit>         VipLevelLimits { get; set; }
        public IDbSet<RiskLevel>                RiskLevels { get; set; }
        
        public IDbSet<WalletTemplate>           WalletTemplates { get; set; }
        public IDbSet<WalletTemplateProduct>    WalletTemplateProducts { get; set; }

        public IDbSet<ContentTranslation>       ContentTranslations { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new LicenseeMap(Schema));
            modelBuilder.Configurations.Add(new BrandMap(Schema));
            modelBuilder.Configurations.Add(new CurrencyMap(Schema));
            modelBuilder.Configurations.Add(new CountryMap(Schema));
            modelBuilder.Configurations.Add(new CultureCodeMap(Schema));
            modelBuilder.Configurations.Add(new VipLevelMap(Schema));
            modelBuilder.Configurations.Add(new WalletTemplateMap(Schema));
            modelBuilder.Configurations.Add(new WalletTemplateProductMap(Schema));
            modelBuilder.Configurations.Add(new ContractMap(Schema));
            modelBuilder.Configurations.Add(new LicenseeProductMap(Schema));
            modelBuilder.Configurations.Add(new BrandProductMap(Schema));
            modelBuilder.Configurations.Add(new ContentTranslationMap(Schema));
            modelBuilder.Configurations.Add(new BrandCountryMap(Schema));
            modelBuilder.Configurations.Add(new BrandCultureMap(Schema));
            modelBuilder.Configurations.Add(new BrandCurrencyMap(Schema));
            modelBuilder.Configurations.Add(new VipLevelGameProviderBetLimitMap(Schema));
            modelBuilder.Configurations.Add(new RiskLevelMap(Schema));
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