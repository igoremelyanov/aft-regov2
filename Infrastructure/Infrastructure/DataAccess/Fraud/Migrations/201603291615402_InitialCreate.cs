namespace AFT.RegoV2.Infrastructure.DataAccess.Fraud.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "fraud.AutoVerificationCheckConfigurations",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        Currency = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreatedBy = c.Guid(nullable: false),
                        HasFraudRiskLevel = c.Boolean(nullable: false),
                        HasPaymentLevel = c.Boolean(nullable: false),
                        HasWithdrawalExemption = c.Boolean(nullable: false),
                        HasNoRecentBonus = c.Boolean(nullable: false),
                        HasWinLoss = c.Boolean(nullable: false),
                        WinLossAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        WinLossOperator = c.Int(nullable: false),
                        HasDepositCount = c.Boolean(nullable: false),
                        TotalDepositCountAmount = c.Int(nullable: false),
                        TotalDepositCountOperator = c.Int(nullable: false),
                        HasWithdrawalCount = c.Boolean(nullable: false),
                        TotalWithdrawalCountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalWithdrawalCountOperator = c.Int(nullable: false),
                        HasAccountAge = c.Boolean(nullable: false),
                        AccountAge = c.Int(nullable: false),
                        AccountAgeOperator = c.Int(nullable: false),
                        HasTotalDepositAmount = c.Boolean(nullable: false),
                        TotalDepositAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalDepositAmountOperator = c.Int(nullable: false),
                        HasWinnings = c.Boolean(nullable: false),
                        HasCompleteDocuments = c.Boolean(nullable: false),
                        Status = c.Int(nullable: false),
                        ActivatedBy = c.String(),
                        DateActivated = c.DateTimeOffset(precision: 7),
                        DeactivatedBy = c.String(),
                        DateDeactivated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("fraud.Brands", t => t.BrandId)
                .Index(t => t.BrandId);
            
            CreateTable(
                "fraud.RiskLevels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        Level = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Status = c.Int(nullable: false),
                        Description = c.String(maxLength: 200),
                        CreatedBy = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        DateUpdated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("fraud.Brands", t => t.BrandId)
                .Index(t => t.BrandId);
            
            CreateTable(
                "fraud.Brands",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Code = c.String(nullable: false, maxLength: 20),
                        Name = c.String(nullable: false, maxLength: 20),
                        LicenseeId = c.Guid(nullable: false),
                        LicenseeName = c.String(nullable: false, maxLength: 50),
                        TimeZoneId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "fraud.SignUpFraudTypes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        SystemAction = c.Int(nullable: false),
                        Remarks = c.String(nullable: false, maxLength: 200),
                        CreatedBy = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        DateUpdated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "fraud.PaymentLevels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        CurrencyCode = c.String(),
                        Code = c.String(maxLength: 20),
                        Name = c.String(nullable: false, maxLength: 50),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("fraud.Brands", t => t.BrandId)
                .Index(t => t.BrandId);
            
            CreateTable(
                "fraud.VipLevels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        Code = c.String(),
                        Name = c.String(),
                        Description = c.String(),
                        Status = c.Int(nullable: false),
                        AutoVerificationCheckConfiguration_Id = c.Guid(),
                        RiskProfileConfiguration_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("fraud.Brands", t => t.BrandId)
                .ForeignKey("fraud.AutoVerificationCheckConfigurations", t => t.AutoVerificationCheckConfiguration_Id)
                .ForeignKey("fraud.RiskProfileConfigurations", t => t.RiskProfileConfiguration_Id)
                .Index(t => t.BrandId)
                .Index(t => t.AutoVerificationCheckConfiguration_Id)
                .Index(t => t.RiskProfileConfiguration_Id);
            
            CreateTable(
                "fraud.WinningRules",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ProductId = c.Guid(nullable: false),
                        Comparison = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Period = c.Int(nullable: false),
                        StartDate = c.DateTimeOffset(precision: 7),
                        EndDate = c.DateTimeOffset(precision: 7),
                        AutoVerificationCheckConfigurationId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("fraud.AutoVerificationCheckConfigurations", t => t.AutoVerificationCheckConfigurationId)
                .Index(t => t.AutoVerificationCheckConfigurationId);
            
            CreateTable(
                "fraud.Bonuses",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        Code = c.String(maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        BonusType = c.Int(nullable: false),
                        BrandId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "fraud.DuplicateMechanismConfigurations",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTimeOffset(precision: 7),
                        DeviceIdExactScore = c.Int(nullable: false),
                        FirstNameExactScore = c.Int(nullable: false),
                        LastNameExactScore = c.Int(nullable: false),
                        FullNameExactScore = c.Int(nullable: false),
                        UsernameExactScore = c.Int(nullable: false),
                        AddressExactScore = c.Int(nullable: false),
                        SignUpIpExactScore = c.Int(nullable: false),
                        MobilePhoneExactScore = c.Int(nullable: false),
                        DateOfBirthExactScore = c.Int(nullable: false),
                        EmailAddressExactScore = c.Int(nullable: false),
                        ZipCodeExactScore = c.Int(nullable: false),
                        DeviceIdFuzzyScore = c.Int(nullable: false),
                        FirstNameFuzzyScore = c.Int(nullable: false),
                        LastNameFuzzyScore = c.Int(nullable: false),
                        FullNameFuzzyScore = c.Int(nullable: false),
                        UsernameFuzzyScore = c.Int(nullable: false),
                        AddressFuzzyScore = c.Int(nullable: false),
                        SignUpIpFuzzyScore = c.Int(nullable: false),
                        MobilePhoneFuzzyScore = c.Int(nullable: false),
                        DateOfBirthFuzzyScore = c.Int(nullable: false),
                        EmailAddressFuzzyScore = c.Int(nullable: false),
                        ZipCodeFuzzyScore = c.Int(nullable: false),
                        NoHandlingScoreMin = c.Int(nullable: false),
                        NoHandlingScoreMax = c.Int(nullable: false),
                        NoHandlingSystemAction = c.Int(nullable: false),
                        NoHandlingDescr = c.String(),
                        RecheckScoreMin = c.Int(nullable: false),
                        RecheckScoreMax = c.Int(nullable: false),
                        RecheckSystemAction = c.Int(nullable: false),
                        RecheckDescr = c.String(),
                        FraudulentScoreMin = c.Int(nullable: false),
                        FraudulentScoreMax = c.Int(nullable: false),
                        FraudulentSystemAction = c.Int(nullable: false),
                        FraudulentDescr = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("fraud.Brands", t => t.BrandId)
                .Index(t => t.BrandId);
            
            CreateTable(
                "fraud.MatchingCriterias",
                c => new
                    {
                        Key = c.Guid(nullable: false),
                        Id = c.Int(nullable: false),
                        Code = c.String(),
                        MatchingResult_FirstPlayerId = c.Guid(),
                        MatchingResult_SecondPlayerId = c.Guid(),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("fraud.MatchingResults", t => new { t.MatchingResult_FirstPlayerId, t.MatchingResult_SecondPlayerId })
                .Index(t => new { t.MatchingResult_FirstPlayerId, t.MatchingResult_SecondPlayerId });
            
            CreateTable(
                "fraud.MatchingResults",
                c => new
                    {
                        FirstPlayerId = c.Guid(nullable: false),
                        SecondPlayerId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.FirstPlayerId, t.SecondPlayerId })
                .ForeignKey("fraud.Players", t => t.FirstPlayerId)
                .ForeignKey("fraud.Players", t => t.SecondPlayerId)
                .Index(t => t.FirstPlayerId, name: "PlayerScore_1")
                .Index(t => t.SecondPlayerId, name: "PlayerScore_2");
            
            CreateTable(
                "fraud.Players",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Address = c.String(),
                        ZipCode = c.String(),
                        IPAddress = c.String(),
                        Username = c.String(),
                        Email = c.String(),
                        Phone = c.String(),
                        FolderAction = c.Int(nullable: false),
                        Tag = c.Int(nullable: false),
                        FraudType = c.String(),
                        AccountStatus = c.Int(nullable: false),
                        SignUpRemark = c.String(),
                        DateRegistered = c.DateTimeOffset(nullable: false, precision: 7),
                        DuplicateCheckDate = c.DateTimeOffset(precision: 7),
                        HandledDate = c.DateTimeOffset(precision: 7),
                        CompletedDate = c.DateTimeOffset(precision: 7),
                        DateOfBirth = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "fraud.PaymentMethods",
                c => new
                    {
                        Key = c.Guid(nullable: false),
                        Id = c.Int(nullable: false),
                        Code = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Key);
            
            CreateTable(
                "player.RiskLevels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        PlayerId = c.Guid(nullable: false),
                        RiskLevelId = c.Guid(nullable: false),
                        Status = c.Int(nullable: false),
                        Description = c.String(maxLength: 200),
                        CreatedBy = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        DateUpdated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("fraud.RiskLevels", t => t.RiskLevelId)
                .Index(t => t.RiskLevelId);
            
            CreateTable(
                "fraud.RiskProfileConfigurations",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        Currency = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreatedBy = c.Guid(nullable: false),
                        HasAccountAge = c.Boolean(nullable: false),
                        AccountAgeOperator = c.Int(nullable: false),
                        AccountAge = c.Int(nullable: false),
                        HasDepositCount = c.Boolean(nullable: false),
                        TotalDepositCountAmount = c.Int(nullable: false),
                        TotalDepositCountOperator = c.Int(nullable: false),
                        HasWithdrawalCount = c.Boolean(nullable: false),
                        TotalWithdrawalCountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalWithdrawalCountOperator = c.Int(nullable: false),
                        HasWinLoss = c.Boolean(nullable: false),
                        WinLossOperator = c.Int(nullable: false),
                        WinLossAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        HasFraudRiskLevel = c.Boolean(nullable: false),
                        HasPaymentMethodCheck = c.Boolean(nullable: false),
                        HasBonusCheck = c.Boolean(nullable: false),
                        HasWithdrawalAveragePercentageCheck = c.Boolean(nullable: false),
                        WithdrawalAveragePercentageOperator = c.Int(nullable: false),
                        WithdrawalAveragePercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        HasWinningsToDepositPercentageIncreaseCheck = c.Boolean(nullable: false),
                        WinningsToDepositPercentageIncreaseOperator = c.Int(nullable: false),
                        WinningsToDepositPercentageIncrease = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("fraud.Brands", t => t.BrandId)
                .Index(t => t.BrandId);
            
            CreateTable(
                "fraud.WagerConfiguration",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        CurrencyCode = c.String(),
                        IsServeAllCurrencies = c.Boolean(nullable: false),
                        IsDepositWageringCheck = c.Boolean(nullable: false),
                        IsManualAdjustmentWageringCheck = c.Boolean(nullable: false),
                        IsRebateWageringCheck = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.Guid(nullable: false),
                        UpdatedBy = c.Guid(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateUpdated = c.DateTimeOffset(precision: 7),
                        ActivatedBy = c.Guid(),
                        DeactivatedBy = c.Guid(),
                        DateActivated = c.DateTimeOffset(precision: 7),
                        DateDeactivated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "fraud.WithdrawalVerificationLog",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        WithdrawalId = c.Guid(nullable: false),
                        VerificationStep = c.Int(nullable: false),
                        IsSuccess = c.Boolean(nullable: false),
                        VerificationType = c.Int(nullable: false),
                        Status = c.String(),
                        VerificationRule = c.String(),
                        RuleRequiredValue = c.String(),
                        CurrentValue = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "fraud.SignUpFraudTypesRiskLevels",
                c => new
                    {
                        SignUpFraudTypeId = c.Guid(nullable: false),
                        RiskLevelId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.SignUpFraudTypeId, t.RiskLevelId })
                .ForeignKey("fraud.SignUpFraudTypes", t => t.SignUpFraudTypeId, cascadeDelete: true)
                .ForeignKey("fraud.RiskLevels", t => t.RiskLevelId, cascadeDelete: true)
                .Index(t => t.SignUpFraudTypeId)
                .Index(t => t.RiskLevelId);
            
            CreateTable(
                "fraud.AutoVerificationCheckConfigurationsRiskLevels",
                c => new
                    {
                        AutoVerificationCheckConfigurationId = c.Guid(nullable: false),
                        RiskLevelId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.AutoVerificationCheckConfigurationId, t.RiskLevelId })
                .ForeignKey("fraud.AutoVerificationCheckConfigurations", t => t.AutoVerificationCheckConfigurationId, cascadeDelete: true)
                .ForeignKey("fraud.RiskLevels", t => t.RiskLevelId, cascadeDelete: true)
                .Index(t => t.AutoVerificationCheckConfigurationId)
                .Index(t => t.RiskLevelId);
            
            CreateTable(
                "fraud.AutoVerificationCheckConfigurationsPaymentLevels",
                c => new
                    {
                        AutoVerificationCheckConfigurationId = c.Guid(nullable: false),
                        PaymentLevelId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.AutoVerificationCheckConfigurationId, t.PaymentLevelId })
                .ForeignKey("fraud.AutoVerificationCheckConfigurations", t => t.AutoVerificationCheckConfigurationId, cascadeDelete: true)
                .ForeignKey("fraud.PaymentLevels", t => t.PaymentLevelId, cascadeDelete: true)
                .Index(t => t.AutoVerificationCheckConfigurationId)
                .Index(t => t.PaymentLevelId);
            
            CreateTable(
                "fraud.RiskProfileConfigurationsBonuses",
                c => new
                    {
                        RiskProfileConfigurationId = c.Guid(nullable: false),
                        BonusId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.RiskProfileConfigurationId, t.BonusId })
                .ForeignKey("fraud.RiskProfileConfigurations", t => t.RiskProfileConfigurationId, cascadeDelete: true)
                .ForeignKey("fraud.Bonuses", t => t.BonusId, cascadeDelete: true)
                .Index(t => t.RiskProfileConfigurationId)
                .Index(t => t.BonusId);
            
            CreateTable(
                "fraud.RiskProfileConfigurationsPaymentMethods",
                c => new
                    {
                        RiskProfileConfigurationId = c.Guid(nullable: false),
                        PaymentMethodId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.RiskProfileConfigurationId, t.PaymentMethodId })
                .ForeignKey("fraud.RiskProfileConfigurations", t => t.RiskProfileConfigurationId, cascadeDelete: true)
                .ForeignKey("fraud.PaymentMethods", t => t.PaymentMethodId, cascadeDelete: true)
                .Index(t => t.RiskProfileConfigurationId)
                .Index(t => t.PaymentMethodId);
            
            CreateTable(
                "fraud.RiskProfileConfigurationsRiskLevels",
                c => new
                    {
                        RiskProfileConfigurationId = c.Guid(nullable: false),
                        RiskLevelId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.RiskProfileConfigurationId, t.RiskLevelId })
                .ForeignKey("fraud.RiskProfileConfigurations", t => t.RiskProfileConfigurationId, cascadeDelete: true)
                .ForeignKey("fraud.RiskLevels", t => t.RiskLevelId, cascadeDelete: true)
                .Index(t => t.RiskProfileConfigurationId)
                .Index(t => t.RiskLevelId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("fraud.VipLevels", "RiskProfileConfiguration_Id", "fraud.RiskProfileConfigurations");
            DropForeignKey("fraud.RiskProfileConfigurations", "BrandId", "fraud.Brands");
            DropForeignKey("fraud.RiskProfileConfigurationsRiskLevels", "RiskLevelId", "fraud.RiskLevels");
            DropForeignKey("fraud.RiskProfileConfigurationsRiskLevels", "RiskProfileConfigurationId", "fraud.RiskProfileConfigurations");
            DropForeignKey("fraud.RiskProfileConfigurationsPaymentMethods", "PaymentMethodId", "fraud.PaymentMethods");
            DropForeignKey("fraud.RiskProfileConfigurationsPaymentMethods", "RiskProfileConfigurationId", "fraud.RiskProfileConfigurations");
            DropForeignKey("fraud.RiskProfileConfigurationsBonuses", "BonusId", "fraud.Bonuses");
            DropForeignKey("fraud.RiskProfileConfigurationsBonuses", "RiskProfileConfigurationId", "fraud.RiskProfileConfigurations");
            DropForeignKey("player.RiskLevels", "RiskLevelId", "fraud.RiskLevels");
            DropForeignKey("fraud.MatchingResults", "SecondPlayerId", "fraud.Players");
            DropForeignKey("fraud.MatchingCriterias", new[] { "MatchingResult_FirstPlayerId", "MatchingResult_SecondPlayerId" }, "fraud.MatchingResults");
            DropForeignKey("fraud.MatchingResults", "FirstPlayerId", "fraud.Players");
            DropForeignKey("fraud.DuplicateMechanismConfigurations", "BrandId", "fraud.Brands");
            DropForeignKey("fraud.WinningRules", "AutoVerificationCheckConfigurationId", "fraud.AutoVerificationCheckConfigurations");
            DropForeignKey("fraud.VipLevels", "AutoVerificationCheckConfiguration_Id", "fraud.AutoVerificationCheckConfigurations");
            DropForeignKey("fraud.VipLevels", "BrandId", "fraud.Brands");
            DropForeignKey("fraud.AutoVerificationCheckConfigurationsPaymentLevels", "PaymentLevelId", "fraud.PaymentLevels");
            DropForeignKey("fraud.AutoVerificationCheckConfigurationsPaymentLevels", "AutoVerificationCheckConfigurationId", "fraud.AutoVerificationCheckConfigurations");
            DropForeignKey("fraud.PaymentLevels", "BrandId", "fraud.Brands");
            DropForeignKey("fraud.AutoVerificationCheckConfigurations", "BrandId", "fraud.Brands");
            DropForeignKey("fraud.AutoVerificationCheckConfigurationsRiskLevels", "RiskLevelId", "fraud.RiskLevels");
            DropForeignKey("fraud.AutoVerificationCheckConfigurationsRiskLevels", "AutoVerificationCheckConfigurationId", "fraud.AutoVerificationCheckConfigurations");
            DropForeignKey("fraud.SignUpFraudTypesRiskLevels", "RiskLevelId", "fraud.RiskLevels");
            DropForeignKey("fraud.SignUpFraudTypesRiskLevels", "SignUpFraudTypeId", "fraud.SignUpFraudTypes");
            DropForeignKey("fraud.RiskLevels", "BrandId", "fraud.Brands");
            DropIndex("fraud.RiskProfileConfigurationsRiskLevels", new[] { "RiskLevelId" });
            DropIndex("fraud.RiskProfileConfigurationsRiskLevels", new[] { "RiskProfileConfigurationId" });
            DropIndex("fraud.RiskProfileConfigurationsPaymentMethods", new[] { "PaymentMethodId" });
            DropIndex("fraud.RiskProfileConfigurationsPaymentMethods", new[] { "RiskProfileConfigurationId" });
            DropIndex("fraud.RiskProfileConfigurationsBonuses", new[] { "BonusId" });
            DropIndex("fraud.RiskProfileConfigurationsBonuses", new[] { "RiskProfileConfigurationId" });
            DropIndex("fraud.AutoVerificationCheckConfigurationsPaymentLevels", new[] { "PaymentLevelId" });
            DropIndex("fraud.AutoVerificationCheckConfigurationsPaymentLevels", new[] { "AutoVerificationCheckConfigurationId" });
            DropIndex("fraud.AutoVerificationCheckConfigurationsRiskLevels", new[] { "RiskLevelId" });
            DropIndex("fraud.AutoVerificationCheckConfigurationsRiskLevels", new[] { "AutoVerificationCheckConfigurationId" });
            DropIndex("fraud.SignUpFraudTypesRiskLevels", new[] { "RiskLevelId" });
            DropIndex("fraud.SignUpFraudTypesRiskLevels", new[] { "SignUpFraudTypeId" });
            DropIndex("fraud.RiskProfileConfigurations", new[] { "BrandId" });
            DropIndex("player.RiskLevels", new[] { "RiskLevelId" });
            DropIndex("fraud.MatchingResults", "PlayerScore_2");
            DropIndex("fraud.MatchingResults", "PlayerScore_1");
            DropIndex("fraud.MatchingCriterias", new[] { "MatchingResult_FirstPlayerId", "MatchingResult_SecondPlayerId" });
            DropIndex("fraud.DuplicateMechanismConfigurations", new[] { "BrandId" });
            DropIndex("fraud.WinningRules", new[] { "AutoVerificationCheckConfigurationId" });
            DropIndex("fraud.VipLevels", new[] { "RiskProfileConfiguration_Id" });
            DropIndex("fraud.VipLevels", new[] { "AutoVerificationCheckConfiguration_Id" });
            DropIndex("fraud.VipLevels", new[] { "BrandId" });
            DropIndex("fraud.PaymentLevels", new[] { "BrandId" });
            DropIndex("fraud.RiskLevels", new[] { "BrandId" });
            DropIndex("fraud.AutoVerificationCheckConfigurations", new[] { "BrandId" });
            DropTable("fraud.RiskProfileConfigurationsRiskLevels");
            DropTable("fraud.RiskProfileConfigurationsPaymentMethods");
            DropTable("fraud.RiskProfileConfigurationsBonuses");
            DropTable("fraud.AutoVerificationCheckConfigurationsPaymentLevels");
            DropTable("fraud.AutoVerificationCheckConfigurationsRiskLevels");
            DropTable("fraud.SignUpFraudTypesRiskLevels");
            DropTable("fraud.WithdrawalVerificationLog");
            DropTable("fraud.WagerConfiguration");
            DropTable("fraud.RiskProfileConfigurations");
            DropTable("player.RiskLevels");
            DropTable("fraud.PaymentMethods");
            DropTable("fraud.Players");
            DropTable("fraud.MatchingResults");
            DropTable("fraud.MatchingCriterias");
            DropTable("fraud.DuplicateMechanismConfigurations");
            DropTable("fraud.Bonuses");
            DropTable("fraud.WinningRules");
            DropTable("fraud.VipLevels");
            DropTable("fraud.PaymentLevels");
            DropTable("fraud.SignUpFraudTypes");
            DropTable("fraud.Brands");
            DropTable("fraud.RiskLevels");
            DropTable("fraud.AutoVerificationCheckConfigurations");
        }
    }
}
