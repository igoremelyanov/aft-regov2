namespace AFT.RegoV2.Infrastructure.DataAccess.Brand.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "brand.Brands",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        LicenseeId = c.Guid(nullable: false),
                        DefaultVipLevelId = c.Guid(),
                        DefaultCulture = c.String(),
                        DefaultCurrency = c.String(),
                        BaseCurrency = c.String(),
                        Code = c.String(),
                        Name = c.String(),
                        Email = c.String(),
                        SmsNumber = c.String(),
                        WebsiteUrl = c.String(),
                        Type = c.Int(nullable: false),
                        TimezoneId = c.String(),
                        EnablePlayerPrefix = c.Boolean(nullable: false),
                        PlayerPrefix = c.String(),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        DateUpdated = c.DateTimeOffset(precision: 7),
                        ActivatedBy = c.String(),
                        DateActivated = c.DateTimeOffset(precision: 7),
                        DeactivatedBy = c.String(),
                        DateDeactivated = c.DateTimeOffset(precision: 7),
                        Remarks = c.String(),
                        InternalAccountsNumber = c.Int(nullable: false),
                        PlayerActivationMethod = c.Int(nullable: false),
                        CurrencySetCreated = c.DateTime(),
                        CurrencySetCreatedBy = c.String(),
                        CurrencySetUpdated = c.DateTime(),
                        CurrencySetUpdatedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("brand.VipLevel", t => t.DefaultVipLevelId)
                .ForeignKey("brand.Licensees", t => t.LicenseeId)
                .Index(t => t.LicenseeId)
                .Index(t => t.DefaultVipLevelId);
            
            CreateTable(
                "brand.BrandCountry",
                c => new
                    {
                        BrandId = c.Guid(nullable: false),
                        CountryCode = c.String(nullable: false, maxLength: 2),
                        DateAdded = c.DateTimeOffset(nullable: false, precision: 7),
                        AddedBy = c.String(),
                    })
                .PrimaryKey(t => new { t.BrandId, t.CountryCode })
                .ForeignKey("brand.Countries", t => t.CountryCode, cascadeDelete: true)
                .ForeignKey("brand.Brands", t => t.BrandId)
                .Index(t => t.BrandId)
                .Index(t => t.CountryCode);
            
            CreateTable(
                "brand.Countries",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 2),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Code);
            
            CreateTable(
                "brand.BrandCulture",
                c => new
                    {
                        BrandId = c.Guid(nullable: false),
                        CultureCode = c.String(nullable: false, maxLength: 10),
                        DateAdded = c.DateTimeOffset(nullable: false, precision: 7),
                        AddedBy = c.String(),
                    })
                .PrimaryKey(t => new { t.BrandId, t.CultureCode })
                .ForeignKey("brand.CultureCodes", t => t.CultureCode, cascadeDelete: true)
                .ForeignKey("brand.Brands", t => t.BrandId)
                .Index(t => t.BrandId)
                .Index(t => t.CultureCode);
            
            CreateTable(
                "brand.CultureCodes",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 10),
                        Name = c.String(nullable: false, maxLength: 50),
                        NativeName = c.String(nullable: false, maxLength: 50),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        DateUpdated = c.DateTimeOffset(precision: 7),
                        ActivatedBy = c.String(),
                        DateActivated = c.DateTimeOffset(precision: 7),
                        DeactivatedBy = c.String(),
                        DateDeactivated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Code);
            
            CreateTable(
                "brand.BrandCurrency",
                c => new
                    {
                        BrandId = c.Guid(nullable: false),
                        CurrencyCode = c.String(nullable: false, maxLength: 3),
                        DateAdded = c.DateTimeOffset(nullable: false, precision: 7),
                        AddedBy = c.String(),
                        DefaultPaymentLevelId = c.Guid(),
                    })
                .PrimaryKey(t => new { t.BrandId, t.CurrencyCode })
                .ForeignKey("brand.Currencies", t => t.CurrencyCode, cascadeDelete: true)
                .ForeignKey("brand.Brands", t => t.BrandId)
                .Index(t => t.BrandId)
                .Index(t => t.CurrencyCode);
            
            CreateTable(
                "brand.Currencies",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 3),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Code);
            
            CreateTable(
                "brand.VipLevel",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        Code = c.String(nullable: false, maxLength: 20),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(maxLength: 200),
                        Rank = c.Int(nullable: false),
                        ColorCode = c.String(maxLength: 7),
                        CreatedBy = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        UpdatedRemark = c.String(),
                        DateUpdated = c.DateTimeOffset(precision: 7),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("brand.Brands", t => t.BrandId, cascadeDelete: true)
                .Index(t => t.BrandId);
            
            CreateTable(
                "brand.xref_VipLevelGameProviderBetLimitMap",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        VipLevelId = c.Guid(nullable: false),
                        BetLimitId = c.Guid(nullable: false),
                        GameProviderId = c.Guid(nullable: false),
                        CurrencyCode = c.String(nullable: false, maxLength: 3),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("brand.Currencies", t => t.CurrencyCode, cascadeDelete: true)
                .ForeignKey("brand.VipLevel", t => t.VipLevelId)
                .Index(t => t.VipLevelId)
                .Index(t => t.CurrencyCode);
            
            CreateTable(
                "brand.Licensees",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        CompanyName = c.String(),
                        AffiliateSystem = c.Boolean(nullable: false),
                        ContractStart = c.DateTimeOffset(nullable: false, precision: 7),
                        ContractEnd = c.DateTimeOffset(precision: 7),
                        Email = c.String(),
                        AllowedBrandCount = c.Int(nullable: false),
                        AllowedWebsiteCount = c.Int(nullable: false),
                        CreatedBy = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        DateUpdated = c.DateTimeOffset(precision: 7),
                        ActivatedBy = c.String(),
                        DateActivated = c.DateTimeOffset(precision: 7),
                        DeactivatedBy = c.String(),
                        DateDeactivated = c.DateTimeOffset(precision: 7),
                        Remarks = c.String(),
                        TimezoneId = c.String(),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "brand.LicenseeContracts",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        LicenseeId = c.Guid(nullable: false),
                        StartDate = c.DateTimeOffset(nullable: false, precision: 7),
                        EndDate = c.DateTimeOffset(precision: 7),
                        IsCurrentContract = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("brand.Licensees", t => t.LicenseeId, cascadeDelete: true)
                .Index(t => t.LicenseeId);
            
            CreateTable(
                "brand.LicenseeProducts",
                c => new
                    {
                        LicenseeId = c.Guid(nullable: false),
                        ProductId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.LicenseeId, t.ProductId })
                .ForeignKey("brand.Licensees", t => t.LicenseeId, cascadeDelete: true)
                .Index(t => t.LicenseeId);
            
            CreateTable(
                "brand.BrandProducts",
                c => new
                    {
                        BrandId = c.Guid(nullable: false),
                        ProductId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.BrandId, t.ProductId })
                .ForeignKey("brand.Brands", t => t.BrandId, cascadeDelete: true)
                .Index(t => t.BrandId);
            
            CreateTable(
                "brand.WalletTemplates",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        Name = c.String(),
                        IsMain = c.Boolean(nullable: false),
                        CurrencyCode = c.String(),
                        CreatedBy = c.Guid(nullable: false),
                        UpdatedBy = c.Guid(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateUpdated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("brand.Brands", t => t.BrandId, cascadeDelete: true)
                .Index(t => t.BrandId);
            
            CreateTable(
                "brand.WalletTemplateProducts",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        WalletTemplateId = c.Guid(nullable: false),
                        ProductId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("brand.WalletTemplates", t => t.WalletTemplateId, cascadeDelete: true)
                .Index(t => t.WalletTemplateId);
            
            CreateTable(
                "brand.ContentTranslations",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Source = c.String(nullable: false, maxLength: 200),
                        Translation = c.String(nullable: false, maxLength: 200),
                        Language = c.String(nullable: false),
                        Status = c.Int(nullable: false),
                        Remark = c.String(),
                        CreatedBy = c.String(maxLength: 200),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        Updated = c.DateTimeOffset(precision: 7),
                        ActivatedBy = c.String(),
                        Activated = c.DateTimeOffset(precision: 7),
                        DeactivatedBy = c.String(),
                        Deactivated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "brand.RiskLevel",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        Level = c.Int(nullable: false),
                        Name = c.String(),
                        Status = c.Int(nullable: false),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("brand.Brands", t => t.BrandId, cascadeDelete: true)
                .Index(t => t.BrandId);
            
            CreateTable(
                "brand.xref_LicenseeCountries",
                c => new
                    {
                        LicenseeId = c.Guid(nullable: false),
                        CountryCode = c.String(nullable: false, maxLength: 2),
                    })
                .PrimaryKey(t => new { t.LicenseeId, t.CountryCode })
                .ForeignKey("brand.Licensees", t => t.LicenseeId, cascadeDelete: true)
                .ForeignKey("brand.Countries", t => t.CountryCode, cascadeDelete: true)
                .Index(t => t.LicenseeId)
                .Index(t => t.CountryCode);
            
            CreateTable(
                "brand.xref_LicenseeCultures",
                c => new
                    {
                        LicenseeId = c.Guid(nullable: false),
                        CultureCode = c.String(nullable: false, maxLength: 10),
                    })
                .PrimaryKey(t => new { t.LicenseeId, t.CultureCode })
                .ForeignKey("brand.Licensees", t => t.LicenseeId, cascadeDelete: true)
                .ForeignKey("brand.CultureCodes", t => t.CultureCode, cascadeDelete: true)
                .Index(t => t.LicenseeId)
                .Index(t => t.CultureCode);
            
            CreateTable(
                "brand.xref_LicenseeCurrencies",
                c => new
                    {
                        LicenseeId = c.Guid(nullable: false),
                        CurrencyCode = c.String(nullable: false, maxLength: 3),
                    })
                .PrimaryKey(t => new { t.LicenseeId, t.CurrencyCode })
                .ForeignKey("brand.Licensees", t => t.LicenseeId, cascadeDelete: true)
                .ForeignKey("brand.Currencies", t => t.CurrencyCode, cascadeDelete: true)
                .Index(t => t.LicenseeId)
                .Index(t => t.CurrencyCode);
            
        }
        
        public override void Down()
        {
            DropForeignKey("brand.RiskLevel", "BrandId", "brand.Brands");
            DropForeignKey("brand.WalletTemplates", "BrandId", "brand.Brands");
            DropForeignKey("brand.WalletTemplateProducts", "WalletTemplateId", "brand.WalletTemplates");
            DropForeignKey("brand.VipLevel", "BrandId", "brand.Brands");
            DropForeignKey("brand.BrandProducts", "BrandId", "brand.Brands");
            DropForeignKey("brand.Brands", "LicenseeId", "brand.Licensees");
            DropForeignKey("brand.LicenseeProducts", "LicenseeId", "brand.Licensees");
            DropForeignKey("brand.xref_LicenseeCurrencies", "CurrencyCode", "brand.Currencies");
            DropForeignKey("brand.xref_LicenseeCurrencies", "LicenseeId", "brand.Licensees");
            DropForeignKey("brand.xref_LicenseeCultures", "CultureCode", "brand.CultureCodes");
            DropForeignKey("brand.xref_LicenseeCultures", "LicenseeId", "brand.Licensees");
            DropForeignKey("brand.xref_LicenseeCountries", "CountryCode", "brand.Countries");
            DropForeignKey("brand.xref_LicenseeCountries", "LicenseeId", "brand.Licensees");
            DropForeignKey("brand.LicenseeContracts", "LicenseeId", "brand.Licensees");
            DropForeignKey("brand.Brands", "DefaultVipLevelId", "brand.VipLevel");
            DropForeignKey("brand.xref_VipLevelGameProviderBetLimitMap", "VipLevelId", "brand.VipLevel");
            DropForeignKey("brand.xref_VipLevelGameProviderBetLimitMap", "CurrencyCode", "brand.Currencies");
            DropForeignKey("brand.BrandCurrency", "BrandId", "brand.Brands");
            DropForeignKey("brand.BrandCurrency", "CurrencyCode", "brand.Currencies");
            DropForeignKey("brand.BrandCulture", "BrandId", "brand.Brands");
            DropForeignKey("brand.BrandCulture", "CultureCode", "brand.CultureCodes");
            DropForeignKey("brand.BrandCountry", "BrandId", "brand.Brands");
            DropForeignKey("brand.BrandCountry", "CountryCode", "brand.Countries");
            DropIndex("brand.xref_LicenseeCurrencies", new[] { "CurrencyCode" });
            DropIndex("brand.xref_LicenseeCurrencies", new[] { "LicenseeId" });
            DropIndex("brand.xref_LicenseeCultures", new[] { "CultureCode" });
            DropIndex("brand.xref_LicenseeCultures", new[] { "LicenseeId" });
            DropIndex("brand.xref_LicenseeCountries", new[] { "CountryCode" });
            DropIndex("brand.xref_LicenseeCountries", new[] { "LicenseeId" });
            DropIndex("brand.RiskLevel", new[] { "BrandId" });
            DropIndex("brand.WalletTemplateProducts", new[] { "WalletTemplateId" });
            DropIndex("brand.WalletTemplates", new[] { "BrandId" });
            DropIndex("brand.BrandProducts", new[] { "BrandId" });
            DropIndex("brand.LicenseeProducts", new[] { "LicenseeId" });
            DropIndex("brand.LicenseeContracts", new[] { "LicenseeId" });
            DropIndex("brand.xref_VipLevelGameProviderBetLimitMap", new[] { "CurrencyCode" });
            DropIndex("brand.xref_VipLevelGameProviderBetLimitMap", new[] { "VipLevelId" });
            DropIndex("brand.VipLevel", new[] { "BrandId" });
            DropIndex("brand.BrandCurrency", new[] { "CurrencyCode" });
            DropIndex("brand.BrandCurrency", new[] { "BrandId" });
            DropIndex("brand.BrandCulture", new[] { "CultureCode" });
            DropIndex("brand.BrandCulture", new[] { "BrandId" });
            DropIndex("brand.BrandCountry", new[] { "CountryCode" });
            DropIndex("brand.BrandCountry", new[] { "BrandId" });
            DropIndex("brand.Brands", new[] { "DefaultVipLevelId" });
            DropIndex("brand.Brands", new[] { "LicenseeId" });
            DropTable("brand.xref_LicenseeCurrencies");
            DropTable("brand.xref_LicenseeCultures");
            DropTable("brand.xref_LicenseeCountries");
            DropTable("brand.RiskLevel");
            DropTable("brand.ContentTranslations");
            DropTable("brand.WalletTemplateProducts");
            DropTable("brand.WalletTemplates");
            DropTable("brand.BrandProducts");
            DropTable("brand.LicenseeProducts");
            DropTable("brand.LicenseeContracts");
            DropTable("brand.Licensees");
            DropTable("brand.xref_VipLevelGameProviderBetLimitMap");
            DropTable("brand.VipLevel");
            DropTable("brand.Currencies");
            DropTable("brand.BrandCurrency");
            DropTable("brand.CultureCodes");
            DropTable("brand.BrandCulture");
            DropTable("brand.Countries");
            DropTable("brand.BrandCountry");
            DropTable("brand.Brands");
        }
    }
}
