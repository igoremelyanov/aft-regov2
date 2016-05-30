using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Bonus.Core.Models.Events.Management;
using AFT.RegoV2.Bonus.Core.Models.Events.Redemption;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Brand.Events.ContentTranslation;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Fraud.Events;
using AFT.RegoV2.Core.Fraud.Interface.Events;
using AFT.RegoV2.Core.Game.Events;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.Core.Messaging.Interface.Events;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Player.Events;
using AFT.RegoV2.Core.Report.Events;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.RegoBus.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class AdminActivityLogWorker : WorkerBase<AdminActivityLogSubscriber>
    {
        public AdminActivityLogWorker(IUnityContainer container, IServiceBus serviceBus) : base(container, serviceBus) { }
    }

    public class AdminActivityLogSubscriber : IBusSubscriber,
        // Player
        IConsumes<PlayerUpdated>,
        IConsumes<PlayerActivated>,
        IConsumes<PlayerDeactivated>,
        IConsumes<PlayerRegistered>,
        IConsumes<NewPasswordSent>,
        IConsumes<PlayerAccountRestrictionsChanged>,
        IConsumes<PlayerSelfExcluded>,
        IConsumes<PlayerTimedOut>,
        IConsumes<PlayerCancelExclusion>,
        IConsumes<PlayerFrozen>,
        IConsumes<PlayerUnfrozen>,
        IConsumes<PlayerLocked>,
        IConsumes<PlayerUnlocked>,
        IConsumes<PlayerVipLevelChanged>,

        // Brand category
        IConsumes<BrandRegistered>,
        IConsumes<BrandUpdated>,
        IConsumes<BrandActivated>,
        IConsumes<BrandDeactivated>,
        IConsumes<BrandCountriesAssigned>,
        IConsumes<BrandCurrenciesAssigned>,
        IConsumes<BrandLanguagesAssigned>,
        IConsumes<BrandProductsAssigned>,
        IConsumes<WalletTemplateCreated>,
        IConsumes<WalletTemplateUpdated>,

        // Licensee category
        IConsumes<LicenseeCreated>,
        IConsumes<LicenseeUpdated>,
        IConsumes<LicenseeActivated>,
        IConsumes<LicenseeDeactivated>,

        // Currency category
        IConsumes<CurrencyCreated>,
        IConsumes<CurrencyUpdated>,
        IConsumes<CurrencyStatusChanged>,

        // Language category
        IConsumes<LanguageCreated>,
        IConsumes<LanguageUpdated>,
        IConsumes<LanguageStatusChanged>,

        // Country category
        IConsumes<CountryCreated>,
        IConsumes<CountryUpdated>,
        IConsumes<CountryRemoved>,

        // VIP Level category
        IConsumes<VipLevelRegistered>,
        IConsumes<VipLevelUpdated>,
        IConsumes<VipLevelActivated>,
        IConsumes<VipLevelDeactivated>,

        // Backend IP Regulation category
        IConsumes<AdminIpRegulationCreated>,
        IConsumes<AdminIpRegulationUpdated>,
        IConsumes<AdminIpRegulationDeleted>,

        // Player IP Regulation category
        IConsumes<BrandIpRegulationCreated>,
        IConsumes<BrandIpRegulationUpdated>,
        IConsumes<BrandIpRegulationDeleted>,

        // Report category
        IConsumes<ReportExported>,

        //Withdrawal
        IConsumes<WithdrawalEvent>,
        IConsumes<WithdrawalCancelled>,
        IConsumes<WithdrawalCreated>,
        IConsumes<WithdrawalApproved>,

        // Player Bank Account
        IConsumes<PlayerBankAccountAdded>,
        IConsumes<PlayerBankAccountEdited>,
        IConsumes<PlayerBankAccountVerified>,
        IConsumes<PlayerBankAccountRejected>,
        IConsumes<PlayerBankAccountCurrentSet>,

        // Bank Account
        IConsumes<BankAccountAdded>,
        IConsumes<BankAccountEdited>,
        IConsumes<BankAccountActivated>,
        IConsumes<BankAccountDeactivated>,

        // Bank
        IConsumes<BankAdded>,
        IConsumes<BankEdited>,

        // Content Translation
        IConsumes<ContentTranslationCreated>,
        IConsumes<ContentTranslationUpdated>,

        // fraud risk level
        IConsumes<RiskLevelCreated>,
        IConsumes<RiskLevelUpdated>,
        IConsumes<RiskLevelStatusUpdated>,
        IConsumes<RiskLevelTagPlayer>,
        IConsumes<RiskLevelUntagPlayer>,

        // Transfer Fund Settings
        IConsumes<TransferFundSettingsActivated>,
        IConsumes<TransferFundSettingsDeactivated>,

        // Payment Level
        IConsumes<PaymentLevelAdded>,
        IConsumes<PaymentLevelEdited>,
        IConsumes<PaymentLevelActivated>,
        IConsumes<PaymentLevelDeactivated>,
        IConsumes<PlayerPaymentLevelChanged>,

        // Offline Deposit
        IConsumes<DepositConfirmed>,
        IConsumes<DepositVerified>,
        IConsumes<DepositUnverified>,

        // Payment Settings
        IConsumes<PaymentSettingActivated>,
        IConsumes<PaymentSettingDeactivated>,
        IConsumes<PaymentSettingCreated>,
        IConsumes<PaymentSettingUpdated>,

        // Bonus
        IConsumes<BonusCreated>,
        IConsumes<BonusUpdated>,
        IConsumes<BonusActivated>,
        IConsumes<BonusDeactivated>,
        IConsumes<BonusTemplateCreated>,
        IConsumes<BonusTemplateUpdated>,
        IConsumes<BonusRedeemed>,

        // Role
        IConsumes<RoleCreated>,
        IConsumes<RoleUpdated>,

        // User
        IConsumes<AdminCreated>,
        IConsumes<AdminUpdated>,
        IConsumes<AdminActivated>,
        IConsumes<AdminDeactivated>,
        IConsumes<AdminPasswordChanged>,

        // Deposit
        IConsumes<DepositSubmitted>,
        IConsumes<DepositApproved>,
        IConsumes<DepositRejected>,
        // Game
        IConsumes<GameCreated>,
        IConsumes<GameUpdated>,
        IConsumes<GameDeleted>,

        // Product
        IConsumes<ProductCreated>,
        IConsumes<ProductUpdated>,
        IConsumes<BetLimitCreated>,
        IConsumes<BetLimitDeleted>,
        IConsumes<BetLimitUpdated>,

        // Identification Documents
        IConsumes<IdentificationDocumentSettingsCreated>,
        IConsumes<IdentificationDocumentSettingsUpdated>,
        IConsumes<IdentityDocumentUploaded>,
        IConsumes<IdentityDocumentVerified>,
        IConsumes<IdentityDocumentUnverified>,

        //Auto Verification Check Configuration
        IConsumes<AutoVerificationCheckActivated>,
        IConsumes<AutoVerificationCheckDeactivated>,
        IConsumes<AutoVerificationCheckCreated>,
        IConsumes<AutoVerificationCheckUpdated>,

        // Payment Gateway Settings
        IConsumes<PaymentGatewaySettingActivated>,
        IConsumes<PaymentGatewaySettingDeactivated>,
        IConsumes<PaymentGatewaySettingCreated>,
        IConsumes<PaymentGatewaySettingUpdated>,

        // Sign Up Fraud Type
        IConsumes<SignUpFraudTypeCreated>,
        IConsumes<SignUpFraudTypeUpdated>,

        // Duplicate Mechanism Configuration
        IConsumes<DuplicateMechanismConfigurationCreated>,
        IConsumes<DuplicateMechanismConfigurationUpdated>,

        // Risks
        IConsumes<RiskProfileCheckConfigCreated>,
        IConsumes<RiskProfileCheckConfigUpdated>,

        // Messaging
        IConsumes<MassMessageSendRequestedEvent>,
        IConsumes<MessageTemplateAddedEvent>,
        IConsumes<MessageTemplateEditedEvent>,
        IConsumes<MessageTemplateActivatedEvent>
    {
        private readonly AdminActivityLogEventHandlers _eventHandlers;

        public AdminActivityLogSubscriber(AdminActivityLogEventHandlers eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }
        
        public void Consume(PlayerUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerDeactivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerRegistered message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(NewPasswordSent message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerAccountRestrictionsChanged message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerSelfExcluded message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerTimedOut message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerCancelExclusion message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerFrozen message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerUnfrozen message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerLocked message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerUnlocked message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerVipLevelChanged message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandRegistered message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandDeactivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandCountriesAssigned message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandCurrenciesAssigned message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandLanguagesAssigned message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandProductsAssigned message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(WalletTemplateCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(WalletTemplateUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LicenseeCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LicenseeUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LicenseeActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LicenseeDeactivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(CurrencyCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(CurrencyUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(CurrencyStatusChanged message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LanguageCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LanguageUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(LanguageStatusChanged message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(CountryCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(CountryUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(CountryRemoved message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(VipLevelRegistered message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(VipLevelUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(VipLevelActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(VipLevelDeactivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(AdminIpRegulationCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(AdminIpRegulationUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(AdminIpRegulationDeleted message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandIpRegulationCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandIpRegulationUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BrandIpRegulationDeleted message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(ReportExported message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(WithdrawalEvent message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(WithdrawalCancelled message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(WithdrawalCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(WithdrawalApproved message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerBankAccountAdded message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerBankAccountEdited message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerBankAccountVerified message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerBankAccountRejected message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerBankAccountCurrentSet message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BankAccountAdded message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BankAccountEdited message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BankAccountActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BankAccountDeactivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BankAdded message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BankEdited message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(ContentTranslationCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(ContentTranslationUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(RiskLevelCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(RiskLevelUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(RiskLevelStatusUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(RiskLevelTagPlayer message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(RiskLevelUntagPlayer message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(TransferFundSettingsActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(TransferFundSettingsDeactivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PaymentLevelAdded message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PaymentLevelEdited message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PaymentLevelActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PaymentLevelDeactivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PlayerPaymentLevelChanged message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(DepositConfirmed message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(DepositVerified message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(DepositUnverified message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PaymentSettingActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PaymentSettingDeactivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PaymentSettingCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PaymentSettingUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BonusCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BonusUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BonusActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BonusDeactivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BonusTemplateCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BonusTemplateUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BonusRedeemed message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(RoleCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(RoleUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(AdminCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(AdminUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(AdminActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(AdminDeactivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(AdminPasswordChanged message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(DepositSubmitted message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(DepositApproved message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(DepositRejected message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(GameCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(GameUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(GameDeleted message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(ProductCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(ProductUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BetLimitCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BetLimitDeleted message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(BetLimitUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(IdentificationDocumentSettingsCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(IdentificationDocumentSettingsUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(IdentityDocumentUploaded message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(IdentityDocumentVerified message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(IdentityDocumentUnverified message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(AutoVerificationCheckActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(AutoVerificationCheckDeactivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(AutoVerificationCheckCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(AutoVerificationCheckUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PaymentGatewaySettingActivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PaymentGatewaySettingDeactivated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PaymentGatewaySettingCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(PaymentGatewaySettingUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(SignUpFraudTypeCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(SignUpFraudTypeUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(DuplicateMechanismConfigurationCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(DuplicateMechanismConfigurationUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(RiskProfileCheckConfigCreated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(RiskProfileCheckConfigUpdated message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(MassMessageSendRequestedEvent message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(MessageTemplateAddedEvent message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(MessageTemplateEditedEvent message)
        {
            _eventHandlers.Handle(message);
        }

        public void Consume(MessageTemplateActivatedEvent message)
        {
            _eventHandlers.Handle(message);
        }
    }
}
