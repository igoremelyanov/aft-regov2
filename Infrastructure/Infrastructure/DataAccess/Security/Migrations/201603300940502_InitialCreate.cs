namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "security.AdminIpRegulation",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IpAddress = c.String(),
                        Description = c.String(),
                        CreatedDate = c.DateTimeOffset(precision: 7),
                        UpdatedDate = c.DateTimeOffset(precision: 7),
                        CreatedBy_Id = c.Guid(),
                        UpdatedBy_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("security.Admins", t => t.CreatedBy_Id)
                .ForeignKey("security.Admins", t => t.UpdatedBy_Id)
                .Index(t => t.CreatedBy_Id)
                .Index(t => t.UpdatedBy_Id);
            
            CreateTable(
                "security.Admins",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Username = c.String(nullable: false, maxLength: 255),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Language = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        Description = c.String(),
                        Role_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("security.Roles", t => t.Role_Id)
                .Index(t => t.Username, unique: true)
                .Index(t => t.Role_Id);
            
            CreateTable(
                "security.AdminBrands",
                c => new
                    {
                        AdminId = c.Guid(nullable: false),
                        Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.AdminId, t.Id })
                .ForeignKey("security.Admins", t => t.AdminId, cascadeDelete: true)
                .Index(t => t.AdminId);
            
            CreateTable(
                "security.BrandFilterSelections",
                c => new
                    {
                        AdminId = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.AdminId, t.BrandId })
                .ForeignKey("security.Admins", t => t.AdminId, cascadeDelete: true)
                .Index(t => t.AdminId);
            
            CreateTable(
                "security.AdminCurrencies",
                c => new
                    {
                        AdminId = c.Guid(nullable: false),
                        Currency = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.AdminId, t.Currency })
                .ForeignKey("security.Admins", t => t.AdminId, cascadeDelete: true)
                .Index(t => t.AdminId);
            
            CreateTable(
                "security.LicenseeFilterSelections",
                c => new
                    {
                        AdminId = c.Guid(nullable: false),
                        LicenseeId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.AdminId, t.LicenseeId })
                .ForeignKey("security.Admins", t => t.AdminId, cascadeDelete: true)
                .Index(t => t.AdminId);
            
            CreateTable(
                "security.AdminLicensees",
                c => new
                    {
                        AdminId = c.Guid(nullable: false),
                        Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.AdminId, t.Id })
                .ForeignKey("security.Admins", t => t.AdminId, cascadeDelete: true)
                .Index(t => t.AdminId);
            
            CreateTable(
                "security.Roles",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Code = c.String(maxLength: 50),
                        Name = c.String(maxLength: 255),
                        Description = c.String(maxLength: 255),
                        CreatedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedDate = c.DateTimeOffset(precision: 7),
                        CreatedBy_Id = c.Guid(),
                        UpdatedBy_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("security.Admins", t => t.CreatedBy_Id)
                .ForeignKey("security.Admins", t => t.UpdatedBy_Id)
                .Index(t => t.CreatedBy_Id)
                .Index(t => t.UpdatedBy_Id);
            
            CreateTable(
                "security.RoleLicensees",
                c => new
                    {
                        RoleId = c.Guid(nullable: false),
                        Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.RoleId, t.Id })
                .ForeignKey("security.Roles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.RoleId);
            
            CreateTable(
                "security.AdminIpRegulationSettings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "security.BrandIpRegulation",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        LicenseeId = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        RedirectionUrl = c.String(),
                        BlockingType = c.String(),
                        IpAddress = c.String(),
                        Description = c.String(),
                        CreatedDate = c.DateTimeOffset(precision: 7),
                        UpdatedDate = c.DateTimeOffset(precision: 7),
                        CreatedBy_Id = c.Guid(),
                        UpdatedBy_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("security.Admins", t => t.CreatedBy_Id)
                .ForeignKey("security.Admins", t => t.UpdatedBy_Id)
                .Index(t => t.CreatedBy_Id)
                .Index(t => t.UpdatedBy_Id);
            
            CreateTable(
                "security.Brands",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        LicenseeId = c.Guid(nullable: false),
                        TimeZoneId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "security.Errors",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Message = c.String(),
                        Source = c.String(),
                        Detail = c.String(),
                        User = c.String(),
                        HostName = c.String(),
                        Type = c.String(),
                        Time = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("security.BrandIpRegulation", "UpdatedBy_Id", "security.Admins");
            DropForeignKey("security.BrandIpRegulation", "CreatedBy_Id", "security.Admins");
            DropForeignKey("security.AdminIpRegulation", "UpdatedBy_Id", "security.Admins");
            DropForeignKey("security.AdminIpRegulation", "CreatedBy_Id", "security.Admins");
            DropForeignKey("security.Admins", "Role_Id", "security.Roles");
            DropForeignKey("security.Roles", "UpdatedBy_Id", "security.Admins");
            DropForeignKey("security.RoleLicensees", "RoleId", "security.Roles");
            DropForeignKey("security.Roles", "CreatedBy_Id", "security.Admins");
            DropForeignKey("security.AdminLicensees", "AdminId", "security.Admins");
            DropForeignKey("security.LicenseeFilterSelections", "AdminId", "security.Admins");
            DropForeignKey("security.AdminCurrencies", "AdminId", "security.Admins");
            DropForeignKey("security.BrandFilterSelections", "AdminId", "security.Admins");
            DropForeignKey("security.AdminBrands", "AdminId", "security.Admins");
            DropIndex("security.BrandIpRegulation", new[] { "UpdatedBy_Id" });
            DropIndex("security.BrandIpRegulation", new[] { "CreatedBy_Id" });
            DropIndex("security.RoleLicensees", new[] { "RoleId" });
            DropIndex("security.Roles", new[] { "UpdatedBy_Id" });
            DropIndex("security.Roles", new[] { "CreatedBy_Id" });
            DropIndex("security.AdminLicensees", new[] { "AdminId" });
            DropIndex("security.LicenseeFilterSelections", new[] { "AdminId" });
            DropIndex("security.AdminCurrencies", new[] { "AdminId" });
            DropIndex("security.BrandFilterSelections", new[] { "AdminId" });
            DropIndex("security.AdminBrands", new[] { "AdminId" });
            DropIndex("security.Admins", new[] { "Role_Id" });
            DropIndex("security.Admins", new[] { "Username" });
            DropIndex("security.AdminIpRegulation", new[] { "UpdatedBy_Id" });
            DropIndex("security.AdminIpRegulation", new[] { "CreatedBy_Id" });
            DropTable("security.Errors");
            DropTable("security.Brands");
            DropTable("security.BrandIpRegulation");
            DropTable("security.AdminIpRegulationSettings");
            DropTable("security.RoleLicensees");
            DropTable("security.Roles");
            DropTable("security.AdminLicensees");
            DropTable("security.LicenseeFilterSelections");
            DropTable("security.AdminCurrencies");
            DropTable("security.BrandFilterSelections");
            DropTable("security.AdminBrands");
            DropTable("security.Admins");
            DropTable("security.AdminIpRegulation");
        }
    }
}
