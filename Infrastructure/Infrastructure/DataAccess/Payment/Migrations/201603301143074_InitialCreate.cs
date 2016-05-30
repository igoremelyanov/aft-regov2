namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "payment.BankAccounts",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AccountId = c.String(nullable: false, maxLength: 20),
                        AccountName = c.String(nullable: false, maxLength: 100),
                        AccountNumber = c.String(nullable: false, maxLength: 20),
                        Province = c.String(nullable: false, maxLength: 200),
                        Branch = c.String(nullable: false, maxLength: 200),
                        CurrencyCode = c.String(nullable: false),
                        SupplierName = c.String(nullable: false, maxLength: 50),
                        ContactNumber = c.String(nullable: false, maxLength: 20),
                        USBCode = c.String(nullable: false, maxLength: 20),
                        PurchasedDate = c.DateTime(),
                        UtilizationDate = c.DateTime(),
                        ExpirationDate = c.DateTime(),
                        IdFrontImage = c.Guid(),
                        IdBackImage = c.Guid(),
                        ATMCardImage = c.Guid(),
                        Remarks = c.String(maxLength: 200),
                        Status = c.Int(nullable: false),
                        CreatedBy = c.String(nullable: false),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        Updated = c.DateTimeOffset(precision: 7),
                        InternetSameBank = c.Boolean(nullable: false),
                        AtmSameBank = c.Boolean(nullable: false),
                        CounterDepositSameBank = c.Boolean(nullable: false),
                        InternetDifferentBank = c.Boolean(nullable: false),
                        AtmDifferentBank = c.Boolean(nullable: false),
                        CounterDepositDifferentBank = c.Boolean(nullable: false),
                        AccountType_Id = c.Guid(nullable: false),
                        Bank_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.BankAccountTypes", t => t.AccountType_Id)
                .ForeignKey("payment.Banks", t => t.Bank_Id, cascadeDelete: true)
                .Index(t => t.AccountType_Id)
                .Index(t => t.Bank_Id);
            
            CreateTable(
                "payment.BankAccountTypes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "payment.Banks",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BankId = c.String(),
                        BankName = c.String(),
                        CountryCode = c.String(nullable: false, maxLength: 128),
                        BrandId = c.Guid(nullable: false),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        CreatedBy = c.String(),
                        Updated = c.DateTimeOffset(precision: 7),
                        UpdatedBy = c.String(),
                        Remarks = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.Brands", t => t.BrandId)
                .ForeignKey("payment.Countries", t => t.CountryCode, cascadeDelete: true)
                .Index(t => t.CountryCode)
                .Index(t => t.BrandId);
            
            CreateTable(
                "payment.Brands",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Code = c.String(),
                        Name = c.String(),
                        LicenseeId = c.Guid(nullable: false),
                        LicenseeName = c.String(),
                        BaseCurrencyCode = c.String(),
                        TimezoneId = c.String(),
                        Licensee_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.Licensees", t => t.Licensee_Id)
                .ForeignKey("payment.Licensees", t => t.LicenseeId, cascadeDelete: true)
                .Index(t => t.LicenseeId)
                .Index(t => t.Licensee_Id);
            
            CreateTable(
                "payment.Licensees",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "payment.Currencies",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 3),
                        Name = c.String(nullable: false, maxLength: 100),
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
                        Licensee_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Code)
                .ForeignKey("payment.Licensees", t => t.Licensee_Id)
                .Index(t => t.Licensee_Id);
            
            CreateTable(
                "payment.Countries",
                c => new
                    {
                        Code = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Code);
            
            CreateTable(
                "payment.PaymentLevel",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Brand_Id = c.Guid(nullable: false),
                        CurrencyCode = c.String(),
                        Code = c.String(),
                        Name = c.String(),
                        EnableOfflineDeposit = c.Boolean(nullable: false),
                        EnableOnlineDeposit = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        DateUpdated = c.DateTimeOffset(precision: 7),
                        ActivatedBy = c.String(),
                        DateActivated = c.DateTimeOffset(precision: 7),
                        DeactivatedBy = c.String(),
                        DateDeactivated = c.DateTimeOffset(precision: 7),
                        Status = c.Int(nullable: false),
                        MaxBankFee = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BankFeeRatio = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.Brands", t => t.Brand_Id)
                .Index(t => t.Brand_Id);
            
            CreateTable(
                "payment.PaymentGatewaySettings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        OnlinePaymentMethodName = c.String(nullable: false, maxLength: 100),
                        PaymentGatewayName = c.String(nullable: false),
                        Channel = c.Int(nullable: false),
                        EntryPoint = c.String(nullable: false, maxLength: 100),
                        Remarks = c.String(nullable: false, maxLength: 200),
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.Brands", t => t.BrandId)
                .Index(t => t.BrandId);
            
            CreateTable(
                "payment.BrandCurrency",
                c => new
                    {
                        BrandId = c.Guid(nullable: false),
                        CurrencyCode = c.String(nullable: false, maxLength: 3),
                    })
                .PrimaryKey(t => new { t.BrandId, t.CurrencyCode })
                .ForeignKey("payment.Brands", t => t.BrandId, cascadeDelete: true)
                .ForeignKey("payment.Currencies", t => t.CurrencyCode, cascadeDelete: true)
                .Index(t => t.BrandId)
                .Index(t => t.CurrencyCode);
            
            CreateTable(
                "payment.CurrencyExchange",
                c => new
                    {
                        BrandId = c.Guid(nullable: false),
                        CurrencyToCode = c.String(nullable: false, maxLength: 3),
                        CurrentRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PreviousRate = c.Decimal(precision: 18, scale: 2),
                        IsBaseCurrency = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        DateUpdated = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => new { t.BrandId, t.CurrencyToCode })
                .ForeignKey("payment.Brands", t => t.BrandId, cascadeDelete: true)
                .ForeignKey("payment.Currencies", t => t.CurrencyToCode, cascadeDelete: true)
                .Index(t => t.BrandId)
                .Index(t => t.CurrencyToCode);
            
            CreateTable(
                "payment.Deposits",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Licensee = c.String(maxLength: 100),
                        BrandId = c.Guid(nullable: false),
                        PlayerId = c.Guid(nullable: false),
                        ReferenceCode = c.String(maxLength: 100),
                        PaymentMethod = c.String(maxLength: 100),
                        CurrencyCode = c.String(maxLength: 100),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        UniqueDepositAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.String(),
                        DateSubmitted = c.DateTimeOffset(nullable: false, precision: 7),
                        SubmittedBy = c.String(),
                        DateApproved = c.DateTimeOffset(precision: 7),
                        ApprovedBy = c.String(),
                        DateVerified = c.DateTimeOffset(precision: 7),
                        VerifiedBy = c.String(),
                        DateRejected = c.DateTimeOffset(precision: 7),
                        RejectedBy = c.String(),
                        DepositType = c.Int(nullable: false),
                        BankAccountId = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.BankAccounts", t => t.BankAccountId)
                .ForeignKey("payment.Brands", t => t.BrandId)
                .ForeignKey("payment.Players", t => t.PlayerId)
                .Index(t => t.BrandId)
                .Index(t => t.PlayerId)
                .Index(t => t.BankAccountId);
            
            CreateTable(
                "payment.Players",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DomainName = c.String(maxLength: 255),
                        Username = c.String(maxLength: 255),
                        FirstName = c.String(maxLength: 255),
                        LastName = c.String(maxLength: 255),
                        Address = c.String(maxLength: 255),
                        ZipCode = c.String(maxLength: 10),
                        Email = c.String(maxLength: 255),
                        PhoneNumber = c.String(maxLength: 20),
                        CurrencyCode = c.String(),
                        BrandId = c.Guid(nullable: false),
                        VipLevelId = c.Guid(nullable: false),
                        HousePlayer = c.Boolean(nullable: false),
                        ExemptWithdrawalVerification = c.Boolean(),
                        ExemptWithdrawalFrom = c.DateTimeOffset(precision: 7),
                        ExemptWithdrawalTo = c.DateTimeOffset(precision: 7),
                        ExemptLimit = c.Int(),
                        DateRegistered = c.DateTimeOffset(nullable: false, precision: 7),
                        IsActive = c.Boolean(nullable: false),
                        CurrentBankAccount_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.Brands", t => t.BrandId, cascadeDelete: true)
                .ForeignKey("payment.PlayerBankAccounts", t => t.CurrentBankAccount_Id)
                .Index(t => t.BrandId)
                .Index(t => t.CurrentBankAccount_Id);
            
            CreateTable(
                "payment.PlayerBankAccounts",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        AccountName = c.String(nullable: false, maxLength: 100),
                        AccountNumber = c.String(nullable: false, maxLength: 50),
                        Province = c.String(nullable: false, maxLength: 200),
                        City = c.String(nullable: false, maxLength: 200),
                        Branch = c.String(maxLength: 200),
                        SwiftCode = c.String(maxLength: 200),
                        Address = c.String(maxLength: 200),
                        Remarks = c.String(maxLength: 200),
                        Status = c.Int(nullable: false),
                        EditLock = c.Boolean(nullable: false),
                        CreatedBy = c.String(nullable: false),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        Updated = c.DateTimeOffset(precision: 7),
                        VerifiedBy = c.String(),
                        Verified = c.DateTimeOffset(precision: 7),
                        RejectedBy = c.String(),
                        Rejected = c.DateTimeOffset(precision: 7),
                        IsCurrent = c.Boolean(nullable: false),
                        Bank_Id = c.Guid(nullable: false),
                        Player_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.Banks", t => t.Bank_Id)
                .ForeignKey("payment.Players", t => t.Player_Id)
                .Index(t => t.Bank_Id)
                .Index(t => t.Player_Id);
            
            CreateTable(
                "payment.OfflineDeposits",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Brand_Id = c.Guid(nullable: false),
                        Player_Id = c.Guid(nullable: false),
                        BankAccount_Id = c.Guid(nullable: false),
                        CurrencyCode = c.String(),
                        TransactionNumber = c.String(),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        CreatedBy = c.String(),
                        Confirmed = c.DateTimeOffset(precision: 7),
                        ConfirmedBy = c.String(),
                        Verified = c.DateTimeOffset(precision: 7),
                        VerifiedBy = c.String(),
                        Approved = c.DateTimeOffset(precision: 7),
                        ApprovedBy = c.String(),
                        Status = c.Int(nullable: false),
                        PlayerAccountName = c.String(),
                        PlayerAccountNumber = c.String(),
                        BankReferenceNumber = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ActualAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Fee = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentMethod = c.Int(nullable: false),
                        TransferType = c.Int(nullable: false),
                        DepositMethod = c.Int(nullable: false),
                        DepositType = c.Int(nullable: false),
                        IdFrontImage = c.Guid(),
                        IdBackImage = c.Guid(),
                        ReceiptImage = c.Guid(),
                        Remark = c.String(),
                        PlayerRemark = c.String(),
                        DepositWagering = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BonusRedemptionId = c.Guid(),
                        UnverifyReason = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.BankAccounts", t => t.BankAccount_Id)
                .ForeignKey("payment.Brands", t => t.Brand_Id)
                .ForeignKey("payment.Players", t => t.Player_Id)
                .Index(t => t.Brand_Id)
                .Index(t => t.Player_Id)
                .Index(t => t.BankAccount_Id);
            
            CreateTable(
                "payment.OfflineWithdrawalHistory",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        OfflineWithdrawalId = c.Guid(nullable: false),
                        Action = c.Int(nullable: false),
                        UserId = c.Guid(nullable: false),
                        Username = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        Remark = c.String(maxLength: 200),
                        TransactionNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.OfflineWithdraws", t => t.OfflineWithdrawalId, cascadeDelete: true)
                .Index(t => t.OfflineWithdrawalId);
            
            CreateTable(
                "payment.OfflineWithdraws",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TransactionNumber = c.String(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        CreatedBy = c.String(nullable: false),
                        Verified = c.DateTimeOffset(precision: 7),
                        VerifiedBy = c.String(),
                        Unverified = c.DateTimeOffset(precision: 7),
                        UnverifiedBy = c.String(),
                        Approved = c.DateTimeOffset(precision: 7),
                        ApprovedBy = c.String(),
                        Rejected = c.DateTimeOffset(precision: 7),
                        RejectedBy = c.String(),
                        Remarks = c.String(maxLength: 200),
                        Status = c.Int(nullable: false),
                        PaymentMethod = c.Int(nullable: false),
                        DocumentsCheckStatus = c.Int(),
                        InvestigationStatus = c.Int(),
                        AutoVerificationCheckStatus = c.Int(),
                        AutoVerificationCheckDate = c.DateTimeOffset(precision: 7),
                        DocumentsCheckDate = c.DateTimeOffset(precision: 7),
                        InvestigationDate = c.DateTimeOffset(precision: 7),
                        RiskLevelStatus = c.Int(),
                        RiskLevelCheckDate = c.DateTimeOffset(precision: 7),
                        Exempted = c.Boolean(nullable: false),
                        ExemptionCheckTime = c.DateTimeOffset(precision: 7),
                        AutoWagerCheck = c.Boolean(nullable: false),
                        AutoWagerCheckTime = c.DateTimeOffset(precision: 7),
                        WagerCheck = c.Boolean(nullable: false),
                        AcceptedBy = c.String(),
                        AcceptedTime = c.DateTimeOffset(precision: 7),
                        RevertedBy = c.String(),
                        RevertedTime = c.DateTimeOffset(precision: 7),
                        CanceledBy = c.String(),
                        InvestigatedBy = c.String(),
                        CanceledTime = c.DateTimeOffset(precision: 7),
                        PlayerBankAccount_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.PlayerBankAccounts", t => t.PlayerBankAccount_Id)
                .Index(t => t.PlayerBankAccount_Id);
            
            CreateTable(
                "payment.OnlineDeposits",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        PlayerId = c.Guid(nullable: false),
                        TransactionNumber = c.String(maxLength: 50),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        CreatedBy = c.String(),
                        Verified = c.DateTimeOffset(precision: 7),
                        VerifiedBy = c.String(),
                        Approved = c.DateTimeOffset(precision: 7),
                        ApprovedBy = c.String(),
                        Rejected = c.DateTimeOffset(precision: 7),
                        RejectedBy = c.String(),
                        Status = c.Int(nullable: false),
                        BonusCode = c.String(),
                        BonusId = c.Guid(),
                        BonusRedemptionId = c.Guid(),
                        Remarks = c.String(maxLength: 200),
                        Method = c.String(),
                        Channel = c.Int(nullable: false),
                        MerchantId = c.String(),
                        Currency = c.String(),
                        Language = c.String(),
                        ReturnUrl = c.String(),
                        NotifyUrl = c.String(),
                        OrderIdOfRouter = c.String(),
                        OrderIdOfGateway = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.Brands", t => t.BrandId)
                .ForeignKey("payment.Players", t => t.PlayerId)
                .Index(t => t.BrandId)
                .Index(t => t.PlayerId)
                .Index(t => t.TransactionNumber, unique: true, name: "idx_TransactionNumber");
            
            CreateTable(
                "payment.PaymentSettings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Brand_Id = c.Guid(nullable: false),
                        PaymentType = c.Int(nullable: false),
                        VipLevel = c.String(nullable: false),
                        CurrencyCode = c.String(nullable: false, maxLength: 3),
                        PaymentGatewayMethod = c.Int(nullable: false),
                        PaymentMethod = c.String(),
                        MinAmountPerTransaction = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxAmountPerTransaction = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxAmountPerDay = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxTransactionPerDay = c.Int(nullable: false),
                        MaxTransactionPerWeek = c.Int(nullable: false),
                        MaxTransactionPerMonth = c.Int(nullable: false),
                        Enabled = c.Int(nullable: false),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTime(),
                        EnabledBy = c.String(),
                        EnabledDate = c.DateTime(),
                        DisabledBy = c.String(),
                        DisabledDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.Brands", t => t.Brand_Id)
                .Index(t => t.Brand_Id);
            
            CreateTable(
                "payment.PlayerPaymentLevel",
                c => new
                    {
                        PlayerId = c.Guid(nullable: false),
                        PaymentLevel_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.PlayerId)
                .ForeignKey("payment.PaymentLevel", t => t.PaymentLevel_Id)
                .Index(t => t.PaymentLevel_Id);
            
            CreateTable(
                "payment.TransferFund",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TransactionNumber = c.String(),
                        TransferType = c.Int(nullable: false),
                        WalletId = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.Int(nullable: false),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        CreatedBy = c.String(),
                        Remarks = c.String(),
                        BonusCode = c.String(),
                        DestinationWalletId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "payment.TransferSettings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Brand_Id = c.Guid(nullable: false),
                        TransferType = c.Int(nullable: false),
                        VipLevel_Id = c.Guid(nullable: false),
                        CurrencyCode = c.String(nullable: false, maxLength: 3),
                        WalletId = c.String(nullable: false),
                        MinAmountPerTransaction = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxAmountPerTransaction = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxAmountPerDay = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxTransactionPerDay = c.Int(nullable: false),
                        MaxTransactionPerWeek = c.Int(nullable: false),
                        MaxTransactionPerMonth = c.Int(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTimeOffset(precision: 7),
                        EnabledBy = c.String(),
                        EnabledDate = c.DateTimeOffset(precision: 7),
                        DisabledBy = c.String(),
                        DisabledDate = c.DateTimeOffset(precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.Brands", t => t.Brand_Id)
                .ForeignKey("payment.VipLevels", t => t.VipLevel_Id)
                .Index(t => t.Brand_Id)
                .Index(t => t.VipLevel_Id);
            
            CreateTable(
                "payment.VipLevels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BrandId = c.Guid(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("payment.Brands", t => t.BrandId, cascadeDelete: true)
                .Index(t => t.BrandId);
            
            CreateTable(
                "payment.WithdrawalLocks",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        PlayerId = c.Guid(nullable: false),
                        WithdrawalId = c.Guid(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.Int(nullable: false),
                        LockedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        LockedBy = c.String(),
                        UnLockedOn = c.DateTimeOffset(precision: 7),
                        UnLockedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.PlayerId);
            
            CreateTable(
                "payment.PaymentLevelBankAccounts",
                c => new
                    {
                        PaymentLevelId = c.Guid(nullable: false),
                        BankAccountId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.PaymentLevelId, t.BankAccountId })
                .ForeignKey("payment.PaymentLevel", t => t.PaymentLevelId, cascadeDelete: true)
                .ForeignKey("payment.BankAccounts", t => t.BankAccountId, cascadeDelete: true)
                .Index(t => t.PaymentLevelId)
                .Index(t => t.BankAccountId);
            
            CreateTable(
                "payment.PaymentLevelPaymentGatewaySettings",
                c => new
                    {
                        PaymentLevelId = c.Guid(nullable: false),
                        PaymentGatewaySettingId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.PaymentLevelId, t.PaymentGatewaySettingId })
                .ForeignKey("payment.PaymentLevel", t => t.PaymentLevelId, cascadeDelete: true)
                .ForeignKey("payment.PaymentGatewaySettings", t => t.PaymentGatewaySettingId, cascadeDelete: true)
                .Index(t => t.PaymentLevelId)
                .Index(t => t.PaymentGatewaySettingId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("payment.TransferSettings", "VipLevel_Id", "payment.VipLevels");
            DropForeignKey("payment.VipLevels", "BrandId", "payment.Brands");
            DropForeignKey("payment.TransferSettings", "Brand_Id", "payment.Brands");
            DropForeignKey("payment.PlayerPaymentLevel", "PaymentLevel_Id", "payment.PaymentLevel");
            DropForeignKey("payment.PaymentSettings", "Brand_Id", "payment.Brands");
            DropForeignKey("payment.OnlineDeposits", "PlayerId", "payment.Players");
            DropForeignKey("payment.OnlineDeposits", "BrandId", "payment.Brands");
            DropForeignKey("payment.OfflineWithdraws", "PlayerBankAccount_Id", "payment.PlayerBankAccounts");
            DropForeignKey("payment.OfflineWithdrawalHistory", "OfflineWithdrawalId", "payment.OfflineWithdraws");
            DropForeignKey("payment.OfflineDeposits", "Player_Id", "payment.Players");
            DropForeignKey("payment.OfflineDeposits", "Brand_Id", "payment.Brands");
            DropForeignKey("payment.OfflineDeposits", "BankAccount_Id", "payment.BankAccounts");
            DropForeignKey("payment.Deposits", "PlayerId", "payment.Players");
            DropForeignKey("payment.Players", "CurrentBankAccount_Id", "payment.PlayerBankAccounts");
            DropForeignKey("payment.PlayerBankAccounts", "Player_Id", "payment.Players");
            DropForeignKey("payment.PlayerBankAccounts", "Bank_Id", "payment.Banks");
            DropForeignKey("payment.Players", "BrandId", "payment.Brands");
            DropForeignKey("payment.Deposits", "BrandId", "payment.Brands");
            DropForeignKey("payment.Deposits", "BankAccountId", "payment.BankAccounts");
            DropForeignKey("payment.CurrencyExchange", "CurrencyToCode", "payment.Currencies");
            DropForeignKey("payment.CurrencyExchange", "BrandId", "payment.Brands");
            DropForeignKey("payment.BrandCurrency", "CurrencyCode", "payment.Currencies");
            DropForeignKey("payment.BrandCurrency", "BrandId", "payment.Brands");
            DropForeignKey("payment.PaymentLevelPaymentGatewaySettings", "PaymentGatewaySettingId", "payment.PaymentGatewaySettings");
            DropForeignKey("payment.PaymentLevelPaymentGatewaySettings", "PaymentLevelId", "payment.PaymentLevel");
            DropForeignKey("payment.PaymentGatewaySettings", "BrandId", "payment.Brands");
            DropForeignKey("payment.PaymentLevel", "Brand_Id", "payment.Brands");
            DropForeignKey("payment.PaymentLevelBankAccounts", "BankAccountId", "payment.BankAccounts");
            DropForeignKey("payment.PaymentLevelBankAccounts", "PaymentLevelId", "payment.PaymentLevel");
            DropForeignKey("payment.Banks", "CountryCode", "payment.Countries");
            DropForeignKey("payment.Banks", "BrandId", "payment.Brands");
            DropForeignKey("payment.Brands", "LicenseeId", "payment.Licensees");
            DropForeignKey("payment.Currencies", "Licensee_Id", "payment.Licensees");
            DropForeignKey("payment.Brands", "Licensee_Id", "payment.Licensees");
            DropForeignKey("payment.BankAccounts", "Bank_Id", "payment.Banks");
            DropForeignKey("payment.BankAccounts", "AccountType_Id", "payment.BankAccountTypes");
            DropIndex("payment.PaymentLevelPaymentGatewaySettings", new[] { "PaymentGatewaySettingId" });
            DropIndex("payment.PaymentLevelPaymentGatewaySettings", new[] { "PaymentLevelId" });
            DropIndex("payment.PaymentLevelBankAccounts", new[] { "BankAccountId" });
            DropIndex("payment.PaymentLevelBankAccounts", new[] { "PaymentLevelId" });
            DropIndex("payment.WithdrawalLocks", new[] { "PlayerId" });
            DropIndex("payment.VipLevels", new[] { "BrandId" });
            DropIndex("payment.TransferSettings", new[] { "VipLevel_Id" });
            DropIndex("payment.TransferSettings", new[] { "Brand_Id" });
            DropIndex("payment.PlayerPaymentLevel", new[] { "PaymentLevel_Id" });
            DropIndex("payment.PaymentSettings", new[] { "Brand_Id" });
            DropIndex("payment.OnlineDeposits", "idx_TransactionNumber");
            DropIndex("payment.OnlineDeposits", new[] { "PlayerId" });
            DropIndex("payment.OnlineDeposits", new[] { "BrandId" });
            DropIndex("payment.OfflineWithdraws", new[] { "PlayerBankAccount_Id" });
            DropIndex("payment.OfflineWithdrawalHistory", new[] { "OfflineWithdrawalId" });
            DropIndex("payment.OfflineDeposits", new[] { "BankAccount_Id" });
            DropIndex("payment.OfflineDeposits", new[] { "Player_Id" });
            DropIndex("payment.OfflineDeposits", new[] { "Brand_Id" });
            DropIndex("payment.PlayerBankAccounts", new[] { "Player_Id" });
            DropIndex("payment.PlayerBankAccounts", new[] { "Bank_Id" });
            DropIndex("payment.Players", new[] { "CurrentBankAccount_Id" });
            DropIndex("payment.Players", new[] { "BrandId" });
            DropIndex("payment.Deposits", new[] { "BankAccountId" });
            DropIndex("payment.Deposits", new[] { "PlayerId" });
            DropIndex("payment.Deposits", new[] { "BrandId" });
            DropIndex("payment.CurrencyExchange", new[] { "CurrencyToCode" });
            DropIndex("payment.CurrencyExchange", new[] { "BrandId" });
            DropIndex("payment.BrandCurrency", new[] { "CurrencyCode" });
            DropIndex("payment.BrandCurrency", new[] { "BrandId" });
            DropIndex("payment.PaymentGatewaySettings", new[] { "BrandId" });
            DropIndex("payment.PaymentLevel", new[] { "Brand_Id" });
            DropIndex("payment.Currencies", new[] { "Licensee_Id" });
            DropIndex("payment.Brands", new[] { "Licensee_Id" });
            DropIndex("payment.Brands", new[] { "LicenseeId" });
            DropIndex("payment.Banks", new[] { "BrandId" });
            DropIndex("payment.Banks", new[] { "CountryCode" });
            DropIndex("payment.BankAccounts", new[] { "Bank_Id" });
            DropIndex("payment.BankAccounts", new[] { "AccountType_Id" });
            DropTable("payment.PaymentLevelPaymentGatewaySettings");
            DropTable("payment.PaymentLevelBankAccounts");
            DropTable("payment.WithdrawalLocks");
            DropTable("payment.VipLevels");
            DropTable("payment.TransferSettings");
            DropTable("payment.TransferFund");
            DropTable("payment.PlayerPaymentLevel");
            DropTable("payment.PaymentSettings");
            DropTable("payment.OnlineDeposits");
            DropTable("payment.OfflineWithdraws");
            DropTable("payment.OfflineWithdrawalHistory");
            DropTable("payment.OfflineDeposits");
            DropTable("payment.PlayerBankAccounts");
            DropTable("payment.Players");
            DropTable("payment.Deposits");
            DropTable("payment.CurrencyExchange");
            DropTable("payment.BrandCurrency");
            DropTable("payment.PaymentGatewaySettings");
            DropTable("payment.PaymentLevel");
            DropTable("payment.Countries");
            DropTable("payment.Currencies");
            DropTable("payment.Licensees");
            DropTable("payment.Brands");
            DropTable("payment.Banks");
            DropTable("payment.BankAccountTypes");
            DropTable("payment.BankAccounts");
        }
    }
}
