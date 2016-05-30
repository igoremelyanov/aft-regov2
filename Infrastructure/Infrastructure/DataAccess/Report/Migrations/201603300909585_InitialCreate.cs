namespace AFT.RegoV2.Infrastructure.DataAccess.Report.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "report.AdminActivityLogs",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Category = c.Int(nullable: false),
                        ActivityDone = c.String(),
                        PerformedBy = c.String(),
                        DatePerformed = c.DateTimeOffset(nullable: false, precision: 7),
                        Remarks = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "report.AdminAuthenticationLog",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        PerformedBy = c.String(maxLength: 100),
                        DatePerformed = c.DateTimeOffset(nullable: false, precision: 7),
                        IPAddress = c.String(maxLength: 100),
                        Headers = c.String(),
                        FailReason = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.PerformedBy)
                .Index(t => t.DatePerformed)
                .Index(t => t.IPAddress)
                .Index(t => t.FailReason);
            
            CreateTable(
                "report.Brands",
                c => new
                    {
                        BrandId = c.Guid(nullable: false),
                        Licensee = c.String(maxLength: 200),
                        BrandCode = c.String(maxLength: 100),
                        Brand = c.String(maxLength: 200),
                        BrandType = c.String(maxLength: 100),
                        PlayerPrefix = c.String(maxLength: 100),
                        AllowedInternalAccountsNumber = c.Int(nullable: false),
                        BrandStatus = c.String(maxLength: 100),
                        BrandTimeZone = c.String(maxLength: 100),
                        CreatedBy = c.String(maxLength: 200),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(maxLength: 200),
                        Updated = c.DateTimeOffset(precision: 7),
                        ActivatedBy = c.String(maxLength: 200),
                        Activated = c.DateTimeOffset(precision: 7),
                        DeactivatedBy = c.String(maxLength: 200),
                        Deactivated = c.DateTimeOffset(precision: 7),
                        Remarks = c.String(maxLength: 1000),
                        DefaultVipLevelId = c.Guid(),
                    })
                .PrimaryKey(t => t.BrandId)
                .Index(t => t.Licensee)
                .Index(t => t.BrandCode)
                .Index(t => t.Brand)
                .Index(t => t.BrandType)
                .Index(t => t.PlayerPrefix)
                .Index(t => t.AllowedInternalAccountsNumber)
                .Index(t => t.BrandStatus)
                .Index(t => t.BrandTimeZone)
                .Index(t => t.CreatedBy)
                .Index(t => t.Created)
                .Index(t => t.UpdatedBy)
                .Index(t => t.Updated)
                .Index(t => t.ActivatedBy)
                .Index(t => t.Activated)
                .Index(t => t.DeactivatedBy)
                .Index(t => t.Deactivated)
                .Index(t => t.Remarks)
                .Index(t => t.DefaultVipLevelId);
            
            CreateTable(
                "report.Deposits",
                c => new
                    {
                        DepositId = c.Guid(nullable: false),
                        Licensee = c.String(maxLength: 100),
                        Brand = c.String(maxLength: 100),
                        Username = c.String(maxLength: 100),
                        IsInternalAccount = c.Boolean(nullable: false),
                        VipLevel = c.String(maxLength: 100),
                        TransactionId = c.String(maxLength: 100),
                        PaymentMethod = c.String(maxLength: 100),
                        Currency = c.String(maxLength: 100),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ActualAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Fee = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.String(maxLength: 100),
                        Submitted = c.DateTimeOffset(nullable: false, precision: 7),
                        SubmittedBy = c.String(maxLength: 100),
                        Approved = c.DateTimeOffset(precision: 7),
                        ApprovedBy = c.String(maxLength: 100),
                        Rejected = c.DateTimeOffset(precision: 7),
                        RejectedBy = c.String(maxLength: 100),
                        Verified = c.DateTimeOffset(precision: 7),
                        VerifiedBy = c.String(maxLength: 100),
                        DepositType = c.String(maxLength: 100),
                        BankAccountName = c.String(maxLength: 100),
                        BankAccountId = c.String(maxLength: 100),
                        BankName = c.String(maxLength: 100),
                        BankProvince = c.String(maxLength: 100),
                        BankBranch = c.String(maxLength: 100),
                        BankAccountNumber = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.DepositId)
                .Index(t => t.Licensee)
                .Index(t => t.Brand)
                .Index(t => t.Username)
                .Index(t => t.IsInternalAccount)
                .Index(t => t.VipLevel)
                .Index(t => t.TransactionId)
                .Index(t => t.PaymentMethod)
                .Index(t => t.Currency)
                .Index(t => t.Amount)
                .Index(t => t.ActualAmount)
                .Index(t => t.Fee)
                .Index(t => t.Status)
                .Index(t => t.Submitted)
                .Index(t => t.SubmittedBy)
                .Index(t => t.Approved)
                .Index(t => t.ApprovedBy)
                .Index(t => t.Rejected)
                .Index(t => t.RejectedBy)
                .Index(t => t.Verified)
                .Index(t => t.VerifiedBy)
                .Index(t => t.DepositType)
                .Index(t => t.BankAccountName)
                .Index(t => t.BankAccountId)
                .Index(t => t.BankName)
                .Index(t => t.BankProvince)
                .Index(t => t.BankBranch)
                .Index(t => t.BankAccountNumber);
            
            CreateTable(
                "report.Languages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Code = c.String(maxLength: 100),
                        Name = c.String(maxLength: 200),
                        NativeName = c.String(maxLength: 200),
                        Status = c.String(maxLength: 100),
                        Licensee = c.String(maxLength: 200),
                        Brand = c.String(maxLength: 200),
                        CreatedBy = c.String(maxLength: 200),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(maxLength: 200),
                        Updated = c.DateTimeOffset(precision: 7),
                        ActivatedBy = c.String(maxLength: 200),
                        Activated = c.DateTimeOffset(precision: 7),
                        DeactivatedBy = c.String(maxLength: 200),
                        Deactivated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code)
                .Index(t => t.Name)
                .Index(t => t.NativeName)
                .Index(t => t.Status)
                .Index(t => t.Licensee)
                .Index(t => t.Brand)
                .Index(t => t.CreatedBy)
                .Index(t => t.Created)
                .Index(t => t.UpdatedBy)
                .Index(t => t.Updated)
                .Index(t => t.ActivatedBy)
                .Index(t => t.Activated)
                .Index(t => t.DeactivatedBy)
                .Index(t => t.Deactivated);
            
            CreateTable(
                "report.Licensees",
                c => new
                    {
                        LicenseeId = c.Guid(nullable: false),
                        Name = c.String(maxLength: 200),
                        CompanyName = c.String(maxLength: 200),
                        EmailAddress = c.String(maxLength: 200),
                        AffiliateSystem = c.Boolean(nullable: false),
                        Status = c.String(maxLength: 100),
                        ContractStart = c.DateTimeOffset(nullable: false, precision: 7),
                        ContractEnd = c.DateTimeOffset(precision: 7),
                        CreatedBy = c.String(maxLength: 200),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(maxLength: 200),
                        Updated = c.DateTimeOffset(precision: 7),
                        ActivatedBy = c.String(maxLength: 200),
                        Activated = c.DateTimeOffset(precision: 7),
                        DeactivatedBy = c.String(maxLength: 200),
                        Deactivated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.LicenseeId)
                .Index(t => t.Name)
                .Index(t => t.CompanyName)
                .Index(t => t.EmailAddress)
                .Index(t => t.AffiliateSystem)
                .Index(t => t.Status)
                .Index(t => t.ContractStart)
                .Index(t => t.ContractEnd)
                .Index(t => t.CreatedBy)
                .Index(t => t.Created)
                .Index(t => t.UpdatedBy)
                .Index(t => t.Updated)
                .Index(t => t.ActivatedBy)
                .Index(t => t.Activated)
                .Index(t => t.DeactivatedBy)
                .Index(t => t.Deactivated);
            
            CreateTable(
                "report.MemberAuthenticationLog",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Brand = c.String(maxLength: 200),
                        BrandId = c.Guid(nullable: false),
                        PerformedBy = c.String(maxLength: 100),
                        DatePerformed = c.DateTimeOffset(nullable: false, precision: 7),
                        IPAddress = c.String(maxLength: 100),
                        Headers = c.String(),
                        FailReason = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Brand)
                .Index(t => t.PerformedBy)
                .Index(t => t.DatePerformed)
                .Index(t => t.IPAddress)
                .Index(t => t.FailReason);
            
            CreateTable(
                "report.Bets",
                c => new
                    {
                        GameActionId = c.Guid(nullable: false),
                        RoundId = c.Guid(nullable: false),
                        Licensee = c.String(maxLength: 100),
                        Brand = c.String(maxLength: 100),
                        LoginName = c.String(maxLength: 100),
                        UserIP = c.String(maxLength: 100),
                        GameName = c.String(maxLength: 100),
                        DateBet = c.DateTimeOffset(nullable: false, precision: 7),
                        BetAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalWinLoss = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Currency = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.GameActionId)
                .Index(t => t.RoundId)
                .Index(t => t.Licensee)
                .Index(t => t.Brand)
                .Index(t => t.LoginName)
                .Index(t => t.UserIP)
                .Index(t => t.GameName)
                .Index(t => t.DateBet)
                .Index(t => t.BetAmount)
                .Index(t => t.TotalWinLoss)
                .Index(t => t.Currency);
            
            CreateTable(
                "report.Players",
                c => new
                    {
                        PlayerId = c.Guid(nullable: false),
                        Licensee = c.String(maxLength: 100),
                        Brand = c.String(maxLength: 100),
                        Username = c.String(maxLength: 100),
                        Mobile = c.String(maxLength: 100),
                        Email = c.String(maxLength: 100),
                        Birthday = c.DateTime(nullable: false),
                        IsInternalAccount = c.Boolean(nullable: false),
                        RegistrationDate = c.DateTimeOffset(nullable: false, precision: 7),
                        IsInactive = c.Boolean(nullable: false),
                        Language = c.String(maxLength: 100),
                        Currency = c.String(maxLength: 100),
                        SignUpIP = c.String(maxLength: 100),
                        VipLevel = c.String(maxLength: 100),
                        Country = c.String(maxLength: 100),
                        PlayerName = c.String(maxLength: 100),
                        Title = c.String(maxLength: 100),
                        Gender = c.String(maxLength: 100),
                        StreetAddress = c.String(maxLength: 100),
                        PostCode = c.String(maxLength: 100),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        CreatedBy = c.String(maxLength: 100),
                        Updated = c.DateTimeOffset(precision: 7),
                        UpdatedBy = c.String(maxLength: 100),
                        Activated = c.DateTimeOffset(precision: 7),
                        ActivatedBy = c.String(maxLength: 100),
                        Deactivated = c.DateTimeOffset(precision: 7),
                        DeactivatedBy = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.PlayerId)
                .Index(t => t.Licensee)
                .Index(t => t.Brand)
                .Index(t => t.Username)
                .Index(t => t.Mobile)
                .Index(t => t.Email)
                .Index(t => t.Birthday)
                .Index(t => t.IsInternalAccount)
                .Index(t => t.RegistrationDate)
                .Index(t => t.Language)
                .Index(t => t.Currency)
                .Index(t => t.SignUpIP)
                .Index(t => t.VipLevel)
                .Index(t => t.Country)
                .Index(t => t.PlayerName)
                .Index(t => t.Title)
                .Index(t => t.Gender)
                .Index(t => t.StreetAddress)
                .Index(t => t.PostCode)
                .Index(t => t.Created)
                .Index(t => t.CreatedBy)
                .Index(t => t.Updated)
                .Index(t => t.UpdatedBy)
                .Index(t => t.Activated)
                .Index(t => t.ActivatedBy)
                .Index(t => t.Deactivated)
                .Index(t => t.DeactivatedBy);
            
            CreateTable(
                "report.PlayerTransactions",
                c => new
                    {
                        TransactionId = c.String(nullable: false, maxLength: 128),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        PlayerId = c.Guid(nullable: false),
                        PerformedBy = c.String(maxLength: 100),
                        Wallet = c.String(maxLength: 100),
                        RoundId = c.Guid(),
                        GameId = c.Guid(),
                        Type = c.String(maxLength: 100),
                        Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MainBalanceAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BonusBalanceAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TemporaryBalanceAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LockBonusAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LockFraudAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LockWithdrawalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MainBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BonusBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TemporaryBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LockBonus = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LockFraud = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LockWithdrawal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsInternal = c.Boolean(nullable: false),
                        CurrencyCode = c.String(maxLength: 100),
                        Description = c.String(maxLength: 200),
                        TransactionNumber = c.String(maxLength: 100),
                        RelatedTransactionId = c.Guid(),
                    })
                .PrimaryKey(t => t.TransactionId)
                .Index(t => t.CreatedOn)
                .Index(t => t.PlayerId)
                .Index(t => t.PerformedBy)
                .Index(t => t.Wallet)
                .Index(t => t.RoundId)
                .Index(t => t.GameId)
                .Index(t => t.Type)
                .Index(t => t.Balance)
                .Index(t => t.MainBalanceAmount)
                .Index(t => t.BonusBalanceAmount)
                .Index(t => t.TemporaryBalanceAmount)
                .Index(t => t.LockBonusAmount)
                .Index(t => t.LockFraudAmount)
                .Index(t => t.LockWithdrawalAmount)
                .Index(t => t.MainBalance)
                .Index(t => t.BonusBalance)
                .Index(t => t.TemporaryBalance)
                .Index(t => t.LockBonus)
                .Index(t => t.LockFraud)
                .Index(t => t.LockWithdrawal)
                .Index(t => t.IsInternal)
                .Index(t => t.CurrencyCode)
                .Index(t => t.Description)
                .Index(t => t.TransactionNumber)
                .Index(t => t.RelatedTransactionId);
            
            CreateTable(
                "report.VipLevels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        VipLevelId = c.Guid(nullable: false),
                        VipLevelLimitId = c.Guid(),
                        Licensee = c.String(maxLength: 200),
                        Brand = c.String(maxLength: 200),
                        Code = c.String(maxLength: 100),
                        Rank = c.Int(nullable: false),
                        Status = c.String(maxLength: 100),
                        GameProvider = c.String(maxLength: 200),
                        Currency = c.String(maxLength: 100),
                        BetLevel = c.String(maxLength: 200),
                        CreatedBy = c.String(maxLength: 200),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(maxLength: 200),
                        Updated = c.DateTimeOffset(precision: 7),
                        ActivatedBy = c.String(maxLength: 200),
                        Activated = c.DateTimeOffset(precision: 7),
                        DeactivatedBy = c.String(maxLength: 200),
                        Deactivated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.VipLevelId)
                .Index(t => t.VipLevelLimitId)
                .Index(t => t.Licensee)
                .Index(t => t.Brand)
                .Index(t => t.Code)
                .Index(t => t.Rank)
                .Index(t => t.Status)
                .Index(t => t.GameProvider)
                .Index(t => t.Currency)
                .Index(t => t.BetLevel)
                .Index(t => t.CreatedBy)
                .Index(t => t.Created)
                .Index(t => t.UpdatedBy)
                .Index(t => t.Updated)
                .Index(t => t.ActivatedBy)
                .Index(t => t.Activated)
                .Index(t => t.DeactivatedBy)
                .Index(t => t.Deactivated);
            
        }
        
        public override void Down()
        {
            DropIndex("report.VipLevels", new[] { "Deactivated" });
            DropIndex("report.VipLevels", new[] { "DeactivatedBy" });
            DropIndex("report.VipLevels", new[] { "Activated" });
            DropIndex("report.VipLevels", new[] { "ActivatedBy" });
            DropIndex("report.VipLevels", new[] { "Updated" });
            DropIndex("report.VipLevels", new[] { "UpdatedBy" });
            DropIndex("report.VipLevels", new[] { "Created" });
            DropIndex("report.VipLevels", new[] { "CreatedBy" });
            DropIndex("report.VipLevels", new[] { "BetLevel" });
            DropIndex("report.VipLevels", new[] { "Currency" });
            DropIndex("report.VipLevels", new[] { "GameProvider" });
            DropIndex("report.VipLevels", new[] { "Status" });
            DropIndex("report.VipLevels", new[] { "Rank" });
            DropIndex("report.VipLevels", new[] { "Code" });
            DropIndex("report.VipLevels", new[] { "Brand" });
            DropIndex("report.VipLevels", new[] { "Licensee" });
            DropIndex("report.VipLevels", new[] { "VipLevelLimitId" });
            DropIndex("report.VipLevels", new[] { "VipLevelId" });
            DropIndex("report.PlayerTransactions", new[] { "RelatedTransactionId" });
            DropIndex("report.PlayerTransactions", new[] { "TransactionNumber" });
            DropIndex("report.PlayerTransactions", new[] { "Description" });
            DropIndex("report.PlayerTransactions", new[] { "CurrencyCode" });
            DropIndex("report.PlayerTransactions", new[] { "IsInternal" });
            DropIndex("report.PlayerTransactions", new[] { "LockWithdrawal" });
            DropIndex("report.PlayerTransactions", new[] { "LockFraud" });
            DropIndex("report.PlayerTransactions", new[] { "LockBonus" });
            DropIndex("report.PlayerTransactions", new[] { "TemporaryBalance" });
            DropIndex("report.PlayerTransactions", new[] { "BonusBalance" });
            DropIndex("report.PlayerTransactions", new[] { "MainBalance" });
            DropIndex("report.PlayerTransactions", new[] { "LockWithdrawalAmount" });
            DropIndex("report.PlayerTransactions", new[] { "LockFraudAmount" });
            DropIndex("report.PlayerTransactions", new[] { "LockBonusAmount" });
            DropIndex("report.PlayerTransactions", new[] { "TemporaryBalanceAmount" });
            DropIndex("report.PlayerTransactions", new[] { "BonusBalanceAmount" });
            DropIndex("report.PlayerTransactions", new[] { "MainBalanceAmount" });
            DropIndex("report.PlayerTransactions", new[] { "Balance" });
            DropIndex("report.PlayerTransactions", new[] { "Type" });
            DropIndex("report.PlayerTransactions", new[] { "GameId" });
            DropIndex("report.PlayerTransactions", new[] { "RoundId" });
            DropIndex("report.PlayerTransactions", new[] { "Wallet" });
            DropIndex("report.PlayerTransactions", new[] { "PerformedBy" });
            DropIndex("report.PlayerTransactions", new[] { "PlayerId" });
            DropIndex("report.PlayerTransactions", new[] { "CreatedOn" });
            DropIndex("report.Players", new[] { "DeactivatedBy" });
            DropIndex("report.Players", new[] { "Deactivated" });
            DropIndex("report.Players", new[] { "ActivatedBy" });
            DropIndex("report.Players", new[] { "Activated" });
            DropIndex("report.Players", new[] { "UpdatedBy" });
            DropIndex("report.Players", new[] { "Updated" });
            DropIndex("report.Players", new[] { "CreatedBy" });
            DropIndex("report.Players", new[] { "Created" });
            DropIndex("report.Players", new[] { "PostCode" });
            DropIndex("report.Players", new[] { "StreetAddress" });
            DropIndex("report.Players", new[] { "Gender" });
            DropIndex("report.Players", new[] { "Title" });
            DropIndex("report.Players", new[] { "PlayerName" });
            DropIndex("report.Players", new[] { "Country" });
            DropIndex("report.Players", new[] { "VipLevel" });
            DropIndex("report.Players", new[] { "SignUpIP" });
            DropIndex("report.Players", new[] { "Currency" });
            DropIndex("report.Players", new[] { "Language" });
            DropIndex("report.Players", new[] { "RegistrationDate" });
            DropIndex("report.Players", new[] { "IsInternalAccount" });
            DropIndex("report.Players", new[] { "Birthday" });
            DropIndex("report.Players", new[] { "Email" });
            DropIndex("report.Players", new[] { "Mobile" });
            DropIndex("report.Players", new[] { "Username" });
            DropIndex("report.Players", new[] { "Brand" });
            DropIndex("report.Players", new[] { "Licensee" });
            DropIndex("report.Bets", new[] { "Currency" });
            DropIndex("report.Bets", new[] { "TotalWinLoss" });
            DropIndex("report.Bets", new[] { "BetAmount" });
            DropIndex("report.Bets", new[] { "DateBet" });
            DropIndex("report.Bets", new[] { "GameName" });
            DropIndex("report.Bets", new[] { "UserIP" });
            DropIndex("report.Bets", new[] { "LoginName" });
            DropIndex("report.Bets", new[] { "Brand" });
            DropIndex("report.Bets", new[] { "Licensee" });
            DropIndex("report.Bets", new[] { "RoundId" });
            DropIndex("report.MemberAuthenticationLog", new[] { "FailReason" });
            DropIndex("report.MemberAuthenticationLog", new[] { "IPAddress" });
            DropIndex("report.MemberAuthenticationLog", new[] { "DatePerformed" });
            DropIndex("report.MemberAuthenticationLog", new[] { "PerformedBy" });
            DropIndex("report.MemberAuthenticationLog", new[] { "Brand" });
            DropIndex("report.Licensees", new[] { "Deactivated" });
            DropIndex("report.Licensees", new[] { "DeactivatedBy" });
            DropIndex("report.Licensees", new[] { "Activated" });
            DropIndex("report.Licensees", new[] { "ActivatedBy" });
            DropIndex("report.Licensees", new[] { "Updated" });
            DropIndex("report.Licensees", new[] { "UpdatedBy" });
            DropIndex("report.Licensees", new[] { "Created" });
            DropIndex("report.Licensees", new[] { "CreatedBy" });
            DropIndex("report.Licensees", new[] { "ContractEnd" });
            DropIndex("report.Licensees", new[] { "ContractStart" });
            DropIndex("report.Licensees", new[] { "Status" });
            DropIndex("report.Licensees", new[] { "AffiliateSystem" });
            DropIndex("report.Licensees", new[] { "EmailAddress" });
            DropIndex("report.Licensees", new[] { "CompanyName" });
            DropIndex("report.Licensees", new[] { "Name" });
            DropIndex("report.Languages", new[] { "Deactivated" });
            DropIndex("report.Languages", new[] { "DeactivatedBy" });
            DropIndex("report.Languages", new[] { "Activated" });
            DropIndex("report.Languages", new[] { "ActivatedBy" });
            DropIndex("report.Languages", new[] { "Updated" });
            DropIndex("report.Languages", new[] { "UpdatedBy" });
            DropIndex("report.Languages", new[] { "Created" });
            DropIndex("report.Languages", new[] { "CreatedBy" });
            DropIndex("report.Languages", new[] { "Brand" });
            DropIndex("report.Languages", new[] { "Licensee" });
            DropIndex("report.Languages", new[] { "Status" });
            DropIndex("report.Languages", new[] { "NativeName" });
            DropIndex("report.Languages", new[] { "Name" });
            DropIndex("report.Languages", new[] { "Code" });
            DropIndex("report.Deposits", new[] { "BankAccountNumber" });
            DropIndex("report.Deposits", new[] { "BankBranch" });
            DropIndex("report.Deposits", new[] { "BankProvince" });
            DropIndex("report.Deposits", new[] { "BankName" });
            DropIndex("report.Deposits", new[] { "BankAccountId" });
            DropIndex("report.Deposits", new[] { "BankAccountName" });
            DropIndex("report.Deposits", new[] { "DepositType" });
            DropIndex("report.Deposits", new[] { "VerifiedBy" });
            DropIndex("report.Deposits", new[] { "Verified" });
            DropIndex("report.Deposits", new[] { "RejectedBy" });
            DropIndex("report.Deposits", new[] { "Rejected" });
            DropIndex("report.Deposits", new[] { "ApprovedBy" });
            DropIndex("report.Deposits", new[] { "Approved" });
            DropIndex("report.Deposits", new[] { "SubmittedBy" });
            DropIndex("report.Deposits", new[] { "Submitted" });
            DropIndex("report.Deposits", new[] { "Status" });
            DropIndex("report.Deposits", new[] { "Fee" });
            DropIndex("report.Deposits", new[] { "ActualAmount" });
            DropIndex("report.Deposits", new[] { "Amount" });
            DropIndex("report.Deposits", new[] { "Currency" });
            DropIndex("report.Deposits", new[] { "PaymentMethod" });
            DropIndex("report.Deposits", new[] { "TransactionId" });
            DropIndex("report.Deposits", new[] { "VipLevel" });
            DropIndex("report.Deposits", new[] { "IsInternalAccount" });
            DropIndex("report.Deposits", new[] { "Username" });
            DropIndex("report.Deposits", new[] { "Brand" });
            DropIndex("report.Deposits", new[] { "Licensee" });
            DropIndex("report.Brands", new[] { "DefaultVipLevelId" });
            DropIndex("report.Brands", new[] { "Remarks" });
            DropIndex("report.Brands", new[] { "Deactivated" });
            DropIndex("report.Brands", new[] { "DeactivatedBy" });
            DropIndex("report.Brands", new[] { "Activated" });
            DropIndex("report.Brands", new[] { "ActivatedBy" });
            DropIndex("report.Brands", new[] { "Updated" });
            DropIndex("report.Brands", new[] { "UpdatedBy" });
            DropIndex("report.Brands", new[] { "Created" });
            DropIndex("report.Brands", new[] { "CreatedBy" });
            DropIndex("report.Brands", new[] { "BrandTimeZone" });
            DropIndex("report.Brands", new[] { "BrandStatus" });
            DropIndex("report.Brands", new[] { "AllowedInternalAccountsNumber" });
            DropIndex("report.Brands", new[] { "PlayerPrefix" });
            DropIndex("report.Brands", new[] { "BrandType" });
            DropIndex("report.Brands", new[] { "Brand" });
            DropIndex("report.Brands", new[] { "BrandCode" });
            DropIndex("report.Brands", new[] { "Licensee" });
            DropIndex("report.AdminAuthenticationLog", new[] { "FailReason" });
            DropIndex("report.AdminAuthenticationLog", new[] { "IPAddress" });
            DropIndex("report.AdminAuthenticationLog", new[] { "DatePerformed" });
            DropIndex("report.AdminAuthenticationLog", new[] { "PerformedBy" });
            DropTable("report.VipLevels");
            DropTable("report.PlayerTransactions");
            DropTable("report.Players");
            DropTable("report.Bets");
            DropTable("report.MemberAuthenticationLog");
            DropTable("report.Licensees");
            DropTable("report.Languages");
            DropTable("report.Deposits");
            DropTable("report.Brands");
            DropTable("report.AdminAuthenticationLog");
            DropTable("report.AdminActivityLogs");
        }
    }
}
