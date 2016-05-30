namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "game.BetLimitGroups",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        ExternalId = c.Int(nullable: false),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "game.BetLimits",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        GameProviderId = c.Guid(nullable: false),
                        Code = c.String(),
                        Description = c.String(),
                        Name = c.String(),
                        CreatedBy = c.String(),
                        UpdatedBy = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateUpdated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.GameProviders", t => t.GameProviderId)
                .Index(t => t.GameProviderId);
            
            CreateTable(
                "game.GameProviders",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Code = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        Category = c.Int(nullable: false),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "game.GameProviderConfigurations",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        GameProviderId = c.Guid(nullable: false),
                        Name = c.String(),
                        Code = c.String(),
                        Endpoint = c.String(),
                        Type = c.String(),
                        AuthorizationClientId = c.String(),
                        AuthorizationSecret = c.String(),
                        SecurityKey = c.String(),
                        SecurityKeyExpiryTime = c.DateTimeOffset(precision: 7),
                        Authentication = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.GameProviders", t => t.GameProviderId)
                .Index(t => t.GameProviderId);
            
            CreateTable(
                "game.GameProviderCurrencies",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        GameProviderId = c.Guid(nullable: false),
                        CurrencyCode = c.String(),
                        GameProviderCurrencyCode = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.GameProviders", t => t.GameProviderId)
                .Index(t => t.GameProviderId);
            
            CreateTable(
                "game.Games",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Code = c.String(),
                        ExternalId = c.String(),
                        EndpointPath = c.String(),
                        GameProviderId = c.Guid(nullable: false),
                        PlatformType = c.Byte(nullable: false),
                        Type = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        ZeroTurnover = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.GameProviders", t => t.GameProviderId)
                .Index(t => t.GameProviderId);
            
            CreateTable(
                "game.VipLevels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        GameProviderBetLimit_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.Brands", t => t.BrandId)
                .ForeignKey("game.BetLimits", t => t.GameProviderBetLimit_Id)
                .Index(t => t.BrandId)
                .Index(t => t.GameProviderBetLimit_Id);
            
            CreateTable(
                "game.Brands",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        LicenseeId = c.Guid(nullable: false),
                        Name = c.String(),
                        Code = c.String(),
                        TimezoneId = c.String(),
                        ClientId = c.String(),
                        ClientSecret = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.Licensees", t => t.LicenseeId)
                .Index(t => t.LicenseeId);
            
            CreateTable(
                "game.BrandGameProviderConfigurations",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        GameProviderId = c.Guid(nullable: false),
                        GameProviderConfigurationId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.Brands", t => t.BrandId)
                .ForeignKey("game.GameProviders", t => t.GameProviderId)
                .ForeignKey("game.GameProviderConfigurations", t => t.GameProviderConfigurationId)
                .Index(t => new { t.BrandId, t.GameProviderId }, unique: true, name: "gameUX_BrandGameProviderConfigurations_BrandId_GameProviderId")
                .Index(t => t.GameProviderConfigurationId);
            
            CreateTable(
                "game.BrandLobbies",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        LobbyId = c.Guid(nullable: false),
                        DateAdded = c.DateTimeOffset(nullable: false, precision: 7),
                        AddedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.Brands", t => t.BrandId)
                .ForeignKey("game.Lobbies", t => t.LobbyId)
                .Index(t => new { t.BrandId, t.LobbyId }, unique: true, name: "gameUX_BrandLobbies_BrandId_LobbyId");
            
            CreateTable(
                "game.Lobbies",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        ClientId = c.String(),
                        ClientSecret = c.String(),
                        PlatformType = c.Byte(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "game.GameGroups",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Code = c.String(),
                        LobbyId = c.Guid(nullable: false),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.Lobbies", t => t.LobbyId)
                .Index(t => t.LobbyId);
            
            CreateTable(
                "game.GameGroupGames",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        GameId = c.Guid(nullable: false),
                        GameGroupId = c.Guid(nullable: false),
                        DateAdded = c.DateTimeOffset(nullable: false, precision: 7),
                        AddedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.Games", t => t.GameId)
                .ForeignKey("game.GameGroups", t => t.GameGroupId)
                .Index(t => new { t.GameId, t.GameGroupId }, unique: true, name: "gameUX_GameGroupGames_GameId_GameGroupId");
            
            CreateTable(
                "game.Licensees",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "game.xref_VipLevelBetLimits",
                c => new
                    {
                        VipLevelId = c.Guid(nullable: false),
                        BetLimitId = c.Guid(nullable: false),
                        GameProviderId = c.Guid(nullable: false),
                        CurrencyCode = c.String(),
                    })
                .PrimaryKey(t => new { t.VipLevelId, t.BetLimitId })
                .ForeignKey("game.BetLimits", t => t.BetLimitId)
                .ForeignKey("game.GameProviders", t => t.GameProviderId)
                .ForeignKey("game.VipLevels", t => t.VipLevelId)
                .Index(t => t.VipLevelId)
                .Index(t => t.BetLimitId)
                .Index(t => t.GameProviderId);
            
            CreateTable(
                "game.Cultures",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Code);
            
            CreateTable(
                "game.Currencies",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 3),
                    })
                .PrimaryKey(t => t.Code);
            
            CreateTable(
                "game.GameActions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ExternalTransactionId = c.String(maxLength: 50),
                        ExternalTransactionReferenceId = c.String(),
                        ExternalBetId = c.String(),
                        WalletTransactionId = c.Guid(),
                        ExternalBatchId = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Description = c.String(),
                        GameActionType = c.Int(nullable: false),
                        Timestamp = c.DateTimeOffset(nullable: false, precision: 7),
                        Context_GameProviderCode = c.String(),
                        Context_GameCode = c.String(),
                        Context_PlatformType = c.Byte(nullable: false),
                        Context_TurnoverContribution = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Context_GgrContribution = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Context_UnsettledBetsContribution = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Context_PlayerIpAddress = c.String(),
                        RoundId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.Rounds", t => t.RoundId)
                .Index(t => t.ExternalTransactionId)
                .Index(t => t.RoundId);
            
            CreateTable(
                "game.Rounds",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ExternalRoundId = c.String(maxLength: 50),
                        PlayerId = c.Guid(nullable: false),
                        GameId = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        ClosedOn = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.Brands", t => t.BrandId)
                .ForeignKey("game.Games", t => t.GameId)
                .Index(t => t.ExternalRoundId, name: "IX_ExternalBetId")
                .Index(t => t.GameId)
                .Index(t => t.BrandId);
            
            CreateTable(
                "game.GameProviderLanguages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        GameProviderId = c.Guid(nullable: false),
                        CultureCode = c.String(),
                        GameProviderCultureCode = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.GameProviders", t => t.GameProviderId)
                .Index(t => t.GameProviderId);
            
            CreateTable(
                "game.Players",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        VipLevelId = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        Name = c.String(),
                        CultureCode = c.String(maxLength: 128),
                        CurrencyCode = c.String(maxLength: 3),
                        DisplayName = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.Cultures", t => t.CultureCode)
                .ForeignKey("game.Currencies", t => t.CurrencyCode)
                .Index(t => t.CultureCode)
                .Index(t => t.CurrencyCode);
            
            CreateTable(
                "game.VipLevelBetLimitGroups",
                c => new
                    {
                        VipLevelId = c.Guid(nullable: false),
                        BetLimitGroupId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.VipLevelId, t.BetLimitGroupId })
                .ForeignKey("game.BetLimitGroups", t => t.BetLimitGroupId)
                .ForeignKey("game.VipLevels", t => t.VipLevelId)
                .Index(t => new { t.VipLevelId, t.BetLimitGroupId }, unique: true, name: "gameUX_BrandLobbies_VipLevelId_BetLimitGroupId");
            
            CreateTable(
                "game.Wallets",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        PlayerId = c.Guid(nullable: false),
                        CurrencyCode = c.String(),
                        Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Brand_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.Brands", t => t.Brand_Id)
                .Index(t => t.PlayerId)
                .Index(t => t.Brand_Id);
            
            CreateTable(
                "game.WalletTransactions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        RoundId = c.Guid(),
                        GameId = c.Guid(),
                        GameName = c.String(),
                        Type = c.Int(nullable: false),
                        MainBalanceAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MainBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        WalletId = c.Guid(nullable: false),
                        RelatedTransactionId = c.Guid(nullable: false),
                        Description = c.String(nullable: false),
                        TransactionNumber = c.String(),
                        ExternalTransactionId = c.String(),
                        ExternalTransactionReferenceId = c.String(),
                        PerformedBy = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("game.Wallets", t => t.WalletId)
                .Index(t => t.RoundId, name: "IX_BetId")
                .Index(t => t.WalletId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("game.WalletTransactions", "WalletId", "game.Wallets");
            DropForeignKey("game.Wallets", "Brand_Id", "game.Brands");
            DropForeignKey("game.VipLevelBetLimitGroups", "VipLevelId", "game.VipLevels");
            DropForeignKey("game.VipLevelBetLimitGroups", "BetLimitGroupId", "game.BetLimitGroups");
            DropForeignKey("game.Players", "CurrencyCode", "game.Currencies");
            DropForeignKey("game.Players", "CultureCode", "game.Cultures");
            DropForeignKey("game.GameProviderLanguages", "GameProviderId", "game.GameProviders");
            DropForeignKey("game.GameActions", "RoundId", "game.Rounds");
            DropForeignKey("game.Rounds", "GameId", "game.Games");
            DropForeignKey("game.Rounds", "BrandId", "game.Brands");
            DropForeignKey("game.VipLevels", "GameProviderBetLimit_Id", "game.BetLimits");
            DropForeignKey("game.xref_VipLevelBetLimits", "VipLevelId", "game.VipLevels");
            DropForeignKey("game.xref_VipLevelBetLimits", "GameProviderId", "game.GameProviders");
            DropForeignKey("game.xref_VipLevelBetLimits", "BetLimitId", "game.BetLimits");
            DropForeignKey("game.VipLevels", "BrandId", "game.Brands");
            DropForeignKey("game.Brands", "LicenseeId", "game.Licensees");
            DropForeignKey("game.GameGroups", "LobbyId", "game.Lobbies");
            DropForeignKey("game.GameGroupGames", "GameGroupId", "game.GameGroups");
            DropForeignKey("game.GameGroupGames", "GameId", "game.Games");
            DropForeignKey("game.BrandLobbies", "LobbyId", "game.Lobbies");
            DropForeignKey("game.BrandLobbies", "BrandId", "game.Brands");
            DropForeignKey("game.BrandGameProviderConfigurations", "GameProviderConfigurationId", "game.GameProviderConfigurations");
            DropForeignKey("game.BrandGameProviderConfigurations", "GameProviderId", "game.GameProviders");
            DropForeignKey("game.BrandGameProviderConfigurations", "BrandId", "game.Brands");
            DropForeignKey("game.BetLimits", "GameProviderId", "game.GameProviders");
            DropForeignKey("game.Games", "GameProviderId", "game.GameProviders");
            DropForeignKey("game.GameProviderCurrencies", "GameProviderId", "game.GameProviders");
            DropForeignKey("game.GameProviderConfigurations", "GameProviderId", "game.GameProviders");
            DropIndex("game.WalletTransactions", new[] { "WalletId" });
            DropIndex("game.WalletTransactions", "IX_BetId");
            DropIndex("game.Wallets", new[] { "Brand_Id" });
            DropIndex("game.Wallets", new[] { "PlayerId" });
            DropIndex("game.VipLevelBetLimitGroups", "gameUX_BrandLobbies_VipLevelId_BetLimitGroupId");
            DropIndex("game.Players", new[] { "CurrencyCode" });
            DropIndex("game.Players", new[] { "CultureCode" });
            DropIndex("game.GameProviderLanguages", new[] { "GameProviderId" });
            DropIndex("game.Rounds", new[] { "BrandId" });
            DropIndex("game.Rounds", new[] { "GameId" });
            DropIndex("game.Rounds", "IX_ExternalBetId");
            DropIndex("game.GameActions", new[] { "RoundId" });
            DropIndex("game.GameActions", new[] { "ExternalTransactionId" });
            DropIndex("game.xref_VipLevelBetLimits", new[] { "GameProviderId" });
            DropIndex("game.xref_VipLevelBetLimits", new[] { "BetLimitId" });
            DropIndex("game.xref_VipLevelBetLimits", new[] { "VipLevelId" });
            DropIndex("game.GameGroupGames", "gameUX_GameGroupGames_GameId_GameGroupId");
            DropIndex("game.GameGroups", new[] { "LobbyId" });
            DropIndex("game.BrandLobbies", "gameUX_BrandLobbies_BrandId_LobbyId");
            DropIndex("game.BrandGameProviderConfigurations", new[] { "GameProviderConfigurationId" });
            DropIndex("game.BrandGameProviderConfigurations", "gameUX_BrandGameProviderConfigurations_BrandId_GameProviderId");
            DropIndex("game.Brands", new[] { "LicenseeId" });
            DropIndex("game.VipLevels", new[] { "GameProviderBetLimit_Id" });
            DropIndex("game.VipLevels", new[] { "BrandId" });
            DropIndex("game.Games", new[] { "GameProviderId" });
            DropIndex("game.GameProviderCurrencies", new[] { "GameProviderId" });
            DropIndex("game.GameProviderConfigurations", new[] { "GameProviderId" });
            DropIndex("game.BetLimits", new[] { "GameProviderId" });
            DropTable("game.WalletTransactions");
            DropTable("game.Wallets");
            DropTable("game.VipLevelBetLimitGroups");
            DropTable("game.Players");
            DropTable("game.GameProviderLanguages");
            DropTable("game.Rounds");
            DropTable("game.GameActions");
            DropTable("game.Currencies");
            DropTable("game.Cultures");
            DropTable("game.xref_VipLevelBetLimits");
            DropTable("game.Licensees");
            DropTable("game.GameGroupGames");
            DropTable("game.GameGroups");
            DropTable("game.Lobbies");
            DropTable("game.BrandLobbies");
            DropTable("game.BrandGameProviderConfigurations");
            DropTable("game.Brands");
            DropTable("game.VipLevels");
            DropTable("game.Games");
            DropTable("game.GameProviderCurrencies");
            DropTable("game.GameProviderConfigurations");
            DropTable("game.GameProviders");
            DropTable("game.BetLimits");
            DropTable("game.BetLimitGroups");
        }
    }
}
