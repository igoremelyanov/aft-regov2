namespace AFT.RegoV2.Infrastructure.DataAccess.Player.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "player.BankAccounts",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AccountId = c.String(nullable: false, maxLength: 20),
                        BankId = c.Guid(nullable: false),
                        BankAccountStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("player.Banks", t => t.BankId, cascadeDelete: true)
                .Index(t => t.BankId);
            
            CreateTable(
                "player.Banks",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BankId = c.String(),
                        BankName = c.String(),
                        BrandId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "player.Brands",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        DefaultVipLevelId = c.Guid(),
                        LicenseeId = c.Guid(nullable: false),
                        TimezoneId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("player.VipLevels", t => t.DefaultVipLevelId)
                .Index(t => t.DefaultVipLevelId);
            
            CreateTable(
                "player.VipLevels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        Code = c.String(),
                        Name = c.String(),
                        Rank = c.Int(nullable: false),
                        Description = c.String(),
                        ColorCode = c.String(),
                        Status = c.Int(nullable: false),
                        Brand_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("player.Brands", t => t.BrandId, cascadeDelete: true)
                .ForeignKey("player.Brands", t => t.Brand_Id)
                .Index(t => t.BrandId)
                .Index(t => t.Brand_Id);
            
            CreateTable(
                "player.IdentificationDocumentSettings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        LicenseeId = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        TransactionType = c.Int(),
                        PaymentGatewayMethod = c.Int(nullable: false),
                        PaymentGatewayBankAccountId = c.Guid(nullable: false),
                        IdFront = c.Boolean(nullable: false),
                        IdBack = c.Boolean(nullable: false),
                        CreditCardFront = c.Boolean(nullable: false),
                        CreditCardBack = c.Boolean(nullable: false),
                        POA = c.Boolean(nullable: false),
                        DCF = c.Boolean(nullable: false),
                        Remark = c.String(maxLength: 200),
                        CreatedBy = c.String(),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        UpdatedOn = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("player.Brands", t => t.BrandId, cascadeDelete: true)
                .ForeignKey("player.BankAccounts", t => t.PaymentGatewayBankAccountId, cascadeDelete: true)
                .Index(t => t.BrandId)
                .Index(t => t.PaymentGatewayBankAccountId);
            
            CreateTable(
                "player.OnSiteMessages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Subject = c.String(),
                        Content = c.String(),
                        Received = c.DateTimeOffset(nullable: false, precision: 7),
                        IsNew = c.Boolean(nullable: false),
                        Player_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("player.Players", t => t.Player_Id, cascadeDelete: true)
                .Index(t => t.Player_Id);
            
            CreateTable(
                "player.Players",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        VipLevelId = c.Guid(),
                        PaymentLevelId = c.Guid(nullable: false),
                        FirstName = c.String(maxLength: 255),
                        LastName = c.String(maxLength: 255),
                        Email = c.String(maxLength: 255),
                        PhoneNumber = c.String(maxLength: 20),
                        MailingAddressLine1 = c.String(maxLength: 50),
                        MailingAddressLine2 = c.String(maxLength: 50),
                        MailingAddressLine3 = c.String(maxLength: 50),
                        MailingAddressLine4 = c.String(maxLength: 50),
                        MailingAddressCity = c.String(maxLength: 50),
                        MailingAddressPostalCode = c.String(maxLength: 10),
                        MailingAddressStateProvince = c.String(),
                        PhysicalAddressLine1 = c.String(),
                        PhysicalAddressLine2 = c.String(),
                        PhysicalAddressLine3 = c.String(),
                        PhysicalAddressLine4 = c.String(),
                        PhysicalAddressCity = c.String(),
                        PhysicalAddressPostalCode = c.String(),
                        PhysicalAddressStateProvince = c.String(),
                        CountryCode = c.String(),
                        CurrencyCode = c.String(),
                        CultureCode = c.String(),
                        Comments = c.String(maxLength: 1500),
                        Username = c.String(maxLength: 255),
                        DateOfBirth = c.DateTime(nullable: false),
                        DateRegistered = c.DateTimeOffset(nullable: false, precision: 7),
                        Gender = c.Int(nullable: false),
                        Title = c.Int(nullable: false),
                        ContactPreference = c.Int(nullable: false),
                        AccountAlertEmail = c.Boolean(nullable: false),
                        AccountAlertSms = c.Boolean(nullable: false),
                        MarketingAlertEmail = c.Boolean(nullable: false),
                        MarketingAlertSms = c.Boolean(nullable: false),
                        MarketingAlertPhone = c.Boolean(nullable: false),
                        IdStatus = c.Int(nullable: false),
                        IsFrozen = c.Boolean(nullable: false),
                        IsInactive = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(nullable: false),
                        TimeOut = c.Int(),
                        TimeOutDate = c.DateTimeOffset(precision: 7),
                        SelfExclusion = c.Int(),
                        SelfExclusionDate = c.DateTimeOffset(precision: 7),
                        IpAddress = c.String(maxLength: 15),
                        DomainName = c.String(maxLength: 255),
                        AccountActivationEmailToken = c.String(),
                        AccountActivationEmailUrl = c.String(),
                        AccountActivationSmsToken = c.String(),
                        IsPhoneNumberVerified = c.Boolean(nullable: false),
                        MobileVerificationCode = c.Int(nullable: false),
                        SecurityQuestionId = c.Guid(),
                        SecurityAnswer = c.String(),
                        InternalAccount = c.Boolean(nullable: false),
                        RefIdentifier = c.Guid(nullable: false),
                        ReferralId = c.Guid(),
                        FailedLoginAttempts = c.Int(nullable: false),
                        LastActivityDate = c.DateTimeOffset(precision: 7),
                        ResetPasswordToken = c.String(),
                        ResetPasswordDate = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("player.Brands", t => t.BrandId, cascadeDelete: true)
                .ForeignKey("player.VipLevels", t => t.VipLevelId)
                .Index(t => t.BrandId)
                .Index(t => t.VipLevelId);
            
            CreateTable(
                "player.IdentityVerifications",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DocumentType = c.Int(nullable: false),
                        CardNumber = c.String(maxLength: 20),
                        ExpirationDate = c.DateTimeOffset(precision: 7),
                        VerificationStatus = c.Int(nullable: false),
                        VerifiedBy = c.String(),
                        DateVerified = c.DateTimeOffset(precision: 7),
                        UnverifiedBy = c.String(),
                        DateUnverified = c.DateTimeOffset(precision: 7),
                        UploadedBy = c.String(),
                        DateUploaded = c.DateTimeOffset(nullable: false, precision: 7),
                        FrontFile = c.Guid(),
                        BackFile = c.Guid(),
                        Remarks = c.String(maxLength: 200),
                        Player_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("player.Players", t => t.Player_Id)
                .Index(t => t.Player_Id);
            
            CreateTable(
                "player.PlayerActivityLog",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        PlayerId = c.Guid(nullable: false),
                        Category = c.String(),
                        ActivityDone = c.String(),
                        PerformedBy = c.String(),
                        DatePerformed = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        DateUpdated = c.DateTimeOffset(precision: 7),
                        Remarks = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "player.PlayerBetStatistics",
                c => new
                    {
                        PlayerId = c.Guid(nullable: false),
                        TotalLoss = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalWon = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotlAdjusted = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.PlayerId);
            
            CreateTable(
                "player.PlayerInfoLog",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Email = c.String(),
                        PhoneNumber = c.String(),
                        MailingAddressLine1 = c.String(),
                        MailingAddressLine2 = c.String(),
                        MailingAddressLine3 = c.String(),
                        MailingAddressLine4 = c.String(),
                        MailingAddressCity = c.String(),
                        MailingAddressPostalCode = c.String(),
                        PhysicalAddressLine1 = c.String(),
                        PhysicalAddressLine2 = c.String(),
                        PhysicalAddressLine3 = c.String(),
                        PhysicalAddressLine4 = c.String(),
                        PhysicalAddressCity = c.String(),
                        PhysicalAddressPostalCode = c.String(),
                        CountryCode = c.String(),
                        CurrencyCode = c.String(),
                        CultureCode = c.String(),
                        Comments = c.String(),
                        Username = c.String(),
                        PasswordEncrypted = c.String(),
                        DateOfBirth = c.DateTime(nullable: false),
                        DateRegistered = c.DateTimeOffset(nullable: false, precision: 7),
                        BrandId = c.Guid(nullable: false),
                        PaymentLevelId = c.Guid(nullable: false),
                        Gender = c.Int(nullable: false),
                        Title = c.Int(nullable: false),
                        ContactPreference = c.Int(nullable: false),
                        IdStatus = c.Int(nullable: false),
                        IpAddress = c.String(),
                        DomainName = c.String(),
                        AccountActivationToken = c.String(),
                        IsPhoneNumberVerified = c.Boolean(nullable: false),
                        MobileVerificationCode = c.Int(nullable: false),
                        SecurityQuestionId = c.Guid(),
                        SecurityAnswer = c.String(),
                        InternalAccount = c.Boolean(nullable: false),
                        ReferralId = c.Guid(),
                        IsFrozen = c.Boolean(nullable: false),
                        IsInactive = c.Boolean(nullable: false),
                        IsSelfExcluded = c.Boolean(nullable: false),
                        IsLocked = c.Boolean(nullable: false),
                        Player_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("player.Players", t => t.Player_Id)
                .Index(t => t.Player_Id);
            
            CreateTable(
                "player.SecurityQuestions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Question = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("player.PlayerInfoLog", "Player_Id", "player.Players");
            DropForeignKey("player.Players", "VipLevelId", "player.VipLevels");
            DropForeignKey("player.OnSiteMessages", "Player_Id", "player.Players");
            DropForeignKey("player.IdentityVerifications", "Player_Id", "player.Players");
            DropForeignKey("player.Players", "BrandId", "player.Brands");
            DropForeignKey("player.IdentificationDocumentSettings", "PaymentGatewayBankAccountId", "player.BankAccounts");
            DropForeignKey("player.IdentificationDocumentSettings", "BrandId", "player.Brands");
            DropForeignKey("player.VipLevels", "Brand_Id", "player.Brands");
            DropForeignKey("player.Brands", "DefaultVipLevelId", "player.VipLevels");
            DropForeignKey("player.VipLevels", "BrandId", "player.Brands");
            DropForeignKey("player.BankAccounts", "BankId", "player.Banks");
            DropIndex("player.PlayerInfoLog", new[] { "Player_Id" });
            DropIndex("player.IdentityVerifications", new[] { "Player_Id" });
            DropIndex("player.Players", new[] { "VipLevelId" });
            DropIndex("player.Players", new[] { "BrandId" });
            DropIndex("player.OnSiteMessages", new[] { "Player_Id" });
            DropIndex("player.IdentificationDocumentSettings", new[] { "PaymentGatewayBankAccountId" });
            DropIndex("player.IdentificationDocumentSettings", new[] { "BrandId" });
            DropIndex("player.VipLevels", new[] { "Brand_Id" });
            DropIndex("player.VipLevels", new[] { "BrandId" });
            DropIndex("player.Brands", new[] { "DefaultVipLevelId" });
            DropIndex("player.BankAccounts", new[] { "BankId" });
            DropTable("player.SecurityQuestions");
            DropTable("player.PlayerInfoLog");
            DropTable("player.PlayerBetStatistics");
            DropTable("player.PlayerActivityLog");
            DropTable("player.IdentityVerifications");
            DropTable("player.Players");
            DropTable("player.OnSiteMessages");
            DropTable("player.IdentificationDocumentSettings");
            DropTable("player.VipLevels");
            DropTable("player.Brands");
            DropTable("player.Banks");
            DropTable("player.BankAccounts");
        }
    }
}
