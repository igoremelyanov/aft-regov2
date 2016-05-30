namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "messaging.Brands",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        Email = c.String(),
                        SmsNumber = c.String(),
                        WebsiteUrl = c.String(),
                        DefaultLanguageCode = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "messaging.Languages",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Code);
            
            CreateTable(
                "messaging.Players",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Username = c.String(nullable: false),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        PhoneNumber = c.String(),
                        LanguageCode = c.String(nullable: false, maxLength: 128),
                        BrandId = c.Guid(nullable: false),
                        VipLevelId = c.Guid(nullable: false),
                        PaymentLevelId = c.Guid(),
                        AccountAlertEmail = c.Boolean(nullable: false),
                        AccountAlertSms = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        DateRegistered = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("messaging.Brands", t => t.BrandId, cascadeDelete: true)
                .ForeignKey("messaging.Languages", t => t.LanguageCode, cascadeDelete: true)
                .ForeignKey("messaging.PaymentLevels", t => t.PaymentLevelId)
                .ForeignKey("messaging.VipLevels", t => t.VipLevelId, cascadeDelete: true)
                .Index(t => t.LanguageCode)
                .Index(t => t.BrandId)
                .Index(t => t.VipLevelId)
                .Index(t => t.PaymentLevelId);
            
            CreateTable(
                "messaging.PaymentLevels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "messaging.VipLevels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "messaging.MassMessageContent",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        MassMessageType = c.Int(nullable: false),
                        Subject = c.String(),
                        Content = c.String(nullable: false),
                        Language_Code = c.String(nullable: false, maxLength: 128),
                        MassMessage_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("messaging.Languages", t => t.Language_Code, cascadeDelete: true)
                .ForeignKey("messaging.MassMessage", t => t.MassMessage_Id, cascadeDelete: true)
                .Index(t => t.Language_Code)
                .Index(t => t.MassMessage_Id);
            
            CreateTable(
                "messaging.MassMessage",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AdminId = c.Guid(nullable: false),
                        IpAddress = c.String(),
                        DateSent = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "messaging.MessageTemplates",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        LanguageCode = c.String(maxLength: 128),
                        MessageType = c.Int(nullable: false),
                        MessageDeliveryMethod = c.Int(nullable: false),
                        TemplateName = c.String(),
                        MessageContent = c.String(),
                        Subject = c.String(),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        Updated = c.DateTimeOffset(precision: 7),
                        ActivatedBy = c.String(),
                        Activated = c.DateTimeOffset(precision: 7),
                        DeactivatedBy = c.String(),
                        Deactivated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("messaging.Brands", t => t.BrandId, cascadeDelete: true)
                .ForeignKey("messaging.Languages", t => t.LanguageCode)
                .Index(t => t.BrandId)
                .Index(t => t.LanguageCode);
            
            CreateTable(
                "messaging.xref_BrandLanguages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Code = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Id, t.Code })
                .ForeignKey("messaging.Brands", t => t.Id, cascadeDelete: true)
                .ForeignKey("messaging.Languages", t => t.Code, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.Code);
            
            CreateTable(
                "messaging.xref_MassMessageRecipients",
                c => new
                    {
                        MassMessageId = c.Guid(nullable: false),
                        PlayerId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.MassMessageId, t.PlayerId })
                .ForeignKey("messaging.MassMessage", t => t.MassMessageId, cascadeDelete: true)
                .ForeignKey("messaging.Players", t => t.PlayerId, cascadeDelete: true)
                .Index(t => t.MassMessageId)
                .Index(t => t.PlayerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("messaging.MessageTemplates", "LanguageCode", "messaging.Languages");
            DropForeignKey("messaging.MessageTemplates", "BrandId", "messaging.Brands");
            DropForeignKey("messaging.MassMessageContent", "MassMessage_Id", "messaging.MassMessage");
            DropForeignKey("messaging.xref_MassMessageRecipients", "PlayerId", "messaging.Players");
            DropForeignKey("messaging.xref_MassMessageRecipients", "MassMessageId", "messaging.MassMessage");
            DropForeignKey("messaging.MassMessageContent", "Language_Code", "messaging.Languages");
            DropForeignKey("messaging.Players", "VipLevelId", "messaging.VipLevels");
            DropForeignKey("messaging.Players", "PaymentLevelId", "messaging.PaymentLevels");
            DropForeignKey("messaging.Players", "LanguageCode", "messaging.Languages");
            DropForeignKey("messaging.Players", "BrandId", "messaging.Brands");
            DropForeignKey("messaging.xref_BrandLanguages", "Code", "messaging.Languages");
            DropForeignKey("messaging.xref_BrandLanguages", "Id", "messaging.Brands");
            DropIndex("messaging.xref_MassMessageRecipients", new[] { "PlayerId" });
            DropIndex("messaging.xref_MassMessageRecipients", new[] { "MassMessageId" });
            DropIndex("messaging.xref_BrandLanguages", new[] { "Code" });
            DropIndex("messaging.xref_BrandLanguages", new[] { "Id" });
            DropIndex("messaging.MessageTemplates", new[] { "LanguageCode" });
            DropIndex("messaging.MessageTemplates", new[] { "BrandId" });
            DropIndex("messaging.MassMessageContent", new[] { "MassMessage_Id" });
            DropIndex("messaging.MassMessageContent", new[] { "Language_Code" });
            DropIndex("messaging.Players", new[] { "PaymentLevelId" });
            DropIndex("messaging.Players", new[] { "VipLevelId" });
            DropIndex("messaging.Players", new[] { "BrandId" });
            DropIndex("messaging.Players", new[] { "LanguageCode" });
            DropTable("messaging.xref_MassMessageRecipients");
            DropTable("messaging.xref_BrandLanguages");
            DropTable("messaging.MessageTemplates");
            DropTable("messaging.MassMessage");
            DropTable("messaging.MassMessageContent");
            DropTable("messaging.VipLevels");
            DropTable("messaging.PaymentLevels");
            DropTable("messaging.Players");
            DropTable("messaging.Languages");
            DropTable("messaging.Brands");
        }
    }
}
