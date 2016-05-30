using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AFT.RegoV2.Bonus.Core.Models.Events.Management;
using AFT.RegoV2.Bonus.Core.Models.Events.Redemption;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Brand.Events.ContentTranslation;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Fraud.Events;
using AFT.RegoV2.Core.Fraud.Interface.Events;
using AFT.RegoV2.Core.Game.Events;
using AFT.RegoV2.Core.Game.Interface.Events;
using AFT.RegoV2.Core.Messaging.Interface.Events;
using AFT.RegoV2.Core.Player.Events;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Core.Report.Events;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using BrandLanguagesAssigned = AFT.RegoV2.Core.Common.Events.Brand.BrandLanguagesAssigned;

namespace AFT.RegoV2.ApplicationServices.Report.EventHandlers
{
    public class AdminActivityLogEventHandlers
    {
        private readonly IUnityContainer _container;

        public AdminActivityLogEventHandlers(IUnityContainer container)
        {
            _container = container;
        }

        #region Player

        public void Handle(PlayerUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(PlayerActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(PlayerDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(PlayerRegistered @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(PlayerSelfExcluded @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(PlayerTimedOut @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(PlayerCancelExclusion @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(NewPasswordSent @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(PlayerAccountRestrictionsChanged @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(PlayerVipLevelChanged @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }
        
        #endregion

        #region Brand category

        public void Handle(BrandRegistered @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event);
        }

        public void Handle(BrandUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event);
        }

        public void Handle(BrandActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event);
        }

        public void Handle(BrandDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event);
        }

        public void Handle(BrandCountriesAssigned @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event);
        }

        public void Handle(BrandCurrenciesAssigned @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event);
        }

        public void Handle(BrandLanguagesAssigned @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event);
        }

        public void Handle(BrandProductsAssigned @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event);
        }

        public void Handle(WalletTemplateCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, "Wallet Created", @event, @event.EventCreatedBy, null);
        }

        public void Handle(WalletTemplateUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, "Wallet Updated", @event, @event.EventCreatedBy, null);
        }
        
        #endregion

        #region Licensee category

        public void Handle(LicenseeCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Licensee, @event);
        }

        public void Handle(LicenseeUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Licensee, @event);
        }

        public void Handle(LicenseeActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Licensee, @event);
        }

        public void Handle(LicenseeDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Licensee, @event);
        }
        #endregion

        #region Currency

        public void Handle(CurrencyCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Currency, @event);
        }

        public void Handle(CurrencyUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Currency, @event);
        }

        public void Handle(CurrencyStatusChanged @event)
        {
            AddActivityLog(AdminActivityLogCategory.Currency, @event);
        }

        #endregion

        #region Language
        public void Handle(LanguageCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Language, @event);
        }

        public void Handle(LanguageUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Language, @event);
        }

        public void Handle(LanguageStatusChanged @event)
        {
            AddActivityLog(AdminActivityLogCategory.Language, @event);
        }
        #endregion

        #region Country
        public void Handle(CountryCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Country, @event);
        }

        public void Handle(CountryUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Country, @event);
        }

        public void Handle(CountryRemoved @event)
        {
            AddActivityLog(AdminActivityLogCategory.Country, @event);
        }
        #endregion

        #region VIP Level
        public void Handle(VipLevelRegistered @event)
        {
            AddActivityLog(AdminActivityLogCategory.VipLevel, @event);
        }

        public void Handle(VipLevelUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.VipLevel, @event);
        }

        public void Handle(VipLevelActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.VipLevel, @event);
        }

        public void Handle(VipLevelDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.VipLevel, @event);
        }
        #endregion

        #region Banks

        public void Handle(BankAdded @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bank, @event);
        }

        public void Handle(BankEdited @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bank, @event);
        }

        #endregion

        #region Player Bank Account
        public void Handle(PlayerBankAccountAdded @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerBankAccount, @event);
        }

        public void Handle(PlayerBankAccountEdited @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerBankAccount, @event);
        }

        public void Handle(PlayerBankAccountVerified @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerBankAccount, "Player bank account verified", @event, @event.EventCreatedBy, @event.Remarks);
        }

        public void Handle(PlayerBankAccountRejected @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerBankAccount, "Player bank account rejected", @event, @event.EventCreatedBy, @event.Remarks);
        }

        public void Handle(PlayerBankAccountCurrentSet @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerBankAccount, @event);
        }
        #endregion

        #region Bank Account

        public void Handle(BankAccountAdded @event)
        {
            AddActivityLog(AdminActivityLogCategory.BankAccount, @event);
        }

        public void Handle(BankAccountEdited @event)
        {
            AddActivityLog(AdminActivityLogCategory.BankAccount, @event);
        }

        public void Handle(BankAccountActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.BankAccount, @event);
        }

        public void Handle(BankAccountDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.BankAccount, @event);
        }

        #endregion

        #region Backend IP Regulation
        public void Handle(AdminIpRegulationCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.BackendIPRegulation, @event);
        }

        public void Handle(AdminIpRegulationUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.BackendIPRegulation, @event);
        }

        public void Handle(AdminIpRegulationDeleted @event)
        {
            AddActivityLog(AdminActivityLogCategory.BackendIPRegulation, @event);
        }
        #endregion

        #region Player IP Regulation
        public void Handle(BrandIpRegulationCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerIPRegulation, @event);
        }

        public void Handle(BrandIpRegulationUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerIPRegulation, @event);
        }

        public void Handle(BrandIpRegulationDeleted @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerIPRegulation, @event);
        }
        #endregion

        #region Report
        public void Handle(ReportExported @event)
        {
            AddActivityLog(AdminActivityLogCategory.Report, @event);
        }
        #endregion

        #region Fraud Risk Level
        public void Handle(RiskLevelCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.FraudRiskLevel, @event);
        }
        public void Handle(RiskLevelStatusUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.FraudRiskLevel, @event);
        }
        public void Handle(RiskLevelUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.FraudRiskLevel, @event);
        }
        public void Handle(RiskLevelTagPlayer @event)
        {
            AddActivityLog(AdminActivityLogCategory.FraudRiskLevel, @event);
        }
        public void Handle(RiskLevelUntagPlayer @event)
        {
            AddActivityLog(AdminActivityLogCategory.FraudRiskLevel, @event);
        }
        #endregion

        #region Auto Verification Check Configuration
        public void Handle(AutoVerificationCheckActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.AutoVerificationConfiguration, @event);
        }

        public void Handle(AutoVerificationCheckDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.AutoVerificationConfiguration, @event);
        }

        public void Handle(AutoVerificationCheckCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.AutoVerificationConfiguration, @event);
        }

        public void Handle(AutoVerificationCheckUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.AutoVerificationConfiguration, @event);
        }
        #endregion

        #region Common
        protected void AddActivityLog(AdminActivityLogCategory category, IDomainEvent @event, string performedBy = null)
        {
            var defaultPropertyNames = typeof(IDomainEvent).GetProperties().Select(pi => pi.Name);
            var customProperties = @event.GetType().GetProperties().Where(pi => !defaultPropertyNames.Contains(pi.Name));
            var activityName = @event.GetType().Name.SeparateWords();
            var remark = GetRemark(@event, customProperties);

            var actionMadeBy = performedBy ?? @event.EventCreatedBy;

            AddActivityLog(category, activityName, @event, actionMadeBy, remark);
        }

        private static string GetRemark(IDomainEvent @event, IEnumerable<PropertyInfo> customProperties)
        {
            return string.Join("\n", customProperties
                .Where(pi => pi.GetValue(@event) != null)
                .Select(pi =>
                {
                    var body = GetStringValue(@event, pi);

                    return pi.Name + ": " + body;
                }));
        }

        private static string GetStringValue(IDomainEvent @event, PropertyInfo pi)
        {
            var value = pi.GetValue(@event);

            if (pi.PropertyType.IsValueType || pi.PropertyType == typeof(string))
            {
                if (pi.PropertyType == typeof (DateTime))
                    return ((DateTime) value).GetNormalizedDate();

                if (pi.PropertyType == typeof (DateTimeOffset))
                    return ((DateTimeOffset) value).ToString("yyyy/MM/dd HH:mm:ss zzz");

                return value.ToString();
            }

            return JsonConvert.SerializeObject(value);
        }

        protected void AddActivityLog(
            AdminActivityLogCategory category, 
            string activityName, 
            IDomainEvent @event, 
            string performedBy, 
            string remarks)
        {
            var repository = _container.Resolve<IReportRepository>();
            repository.AdminActivityLog.Add(new AdminActivityLog
            {
                Id = Guid.NewGuid(),
                Category = category,
                ActivityDone = activityName,
                DatePerformed = @event.EventCreated,
                PerformedBy = performedBy,
                Remarks = remarks ?? string.Empty
            });
            repository.SaveChanges();
        }
        #endregion

        #region Content Translations
        public void Handle(ContentTranslationCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.ContentTranslation, @event);
        }

        public void Handle(ContentTranslationUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.ContentTranslation, @event);
        }
        #endregion

        #region Transfer Fund Settings

        public void Handle(TransferFundSettingsActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.TransferFundSettings, @event);
        }

        public void Handle(TransferFundSettingsDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.TransferFundSettings, @event);
        }

        #endregion

        #region Payment Level

        public void Handle(PaymentLevelAdded @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentLevel, @event);
        }

        public void Handle(PaymentLevelEdited @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentLevel, @event);
        }

        public void Handle(PaymentLevelActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentLevel, @event);
        }

        public void Handle(PaymentLevelDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentLevel, @event);
        }

        public void Handle(PlayerPaymentLevelChanged @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }
        #endregion

        #region Payment Settings

        public void Handle(PaymentSettingCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentSetings, @event);
        }

        public void Handle(PaymentSettingUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentSetings, @event);
        }

        public void Handle(PaymentSettingActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentSetings, @event);
        }

        public void Handle(PaymentSettingDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentSetings, @event);
        }

        #endregion

        #region Bonus

        public void Handle(BonusCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bonus, @event);
        }

        public void Handle(BonusUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bonus, @event);
        }

        public void Handle(BonusActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bonus, @event);
        }

        public void Handle(BonusDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bonus, @event);
        }

        public void Handle(BonusTemplateCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bonus, @event);
        }

        public void Handle(BonusTemplateUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bonus, @event);
        }

        public void Handle(BonusRedeemed @event)
        {
            if (@event.IssuedByCs)
            {
                var remark = string.Format(
                    "Bonus ({0}) of {1} was issued to Player ({2})",
                    @event.BonusName,
                    @event.Amount.Format(),
                    @event.PlayerName);

                AddActivityLog(AdminActivityLogCategory.Bonus, "Bonus issued by CS", @event, @event.EventCreatedBy, remark);
            }
        }

        #endregion

        #region Withdrawal
        public void Handle(WithdrawalCancelled @event)
        {
            AddActivityLog(AdminActivityLogCategory.Withdrawal, @event, @event.Username);
        }

        public void Handle(WithdrawalCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Withdrawal, @event, @event.Username);
        }

        public void Handle(WithdrawalApproved @event)
        {
            AddActivityLog(AdminActivityLogCategory.Withdrawal, @event, @event.Username);
        }

        public void Handle(WithdrawalEvent @event)
        {
            AddActivityLog(AdminActivityLogCategory.Withdrawal, GetConcreteWithdrawalEvent(@event), @event.Username);
        }


        /// <summary>
        /// For the handle method, when we add ctivity to the admin log we should pas a concrete event but not a common parent as WithdrawalEvent is
        /// in the case of all the withdrawal events. This is why, based on the WithdrawalEvent.Status we return an instance of the corresponding object.
        /// </summary>
        /// <param name="wdEvent"></param>
        /// <returns></returns>
        private IDomainEvent GetConcreteWithdrawalEvent(WithdrawalEvent wdEvent)
        {
            var wdId = wdEvent.WithdrawalId;
            var amnt = wdEvent.Amount;
            var dtCreated = wdEvent.DateCreated;
            var uId = wdEvent.UserId;
            var status = wdEvent.Status;
            var remark = wdEvent.Remark;
            var transNum = wdEvent.TransactionNumber;
            var wdMadeBy = wdEvent.WithdrawalMadeBy;
            var uname = wdEvent.Username;

            switch (wdEvent.Status)
            {
                case WithdrawalStatus.Accepted:
                    return new WithdrawalAccepted(wdId, amnt, dtCreated, uId, wdMadeBy, status, remark, transNum, uname)
                    {
                        EventCreated = dtCreated
                    };
                case WithdrawalStatus.Approved:
                    return new WithdrawalApproved(wdId, amnt, dtCreated, uId, wdMadeBy, status, remark, transNum, uname)
                    {
                        EventCreated = dtCreated
                    };
                case WithdrawalStatus.Canceled:               
                    return new WithdrawalCancelled(wdId, amnt, dtCreated, uId, wdMadeBy, status, remark, transNum, uname)
                    {
                        EventCreated = dtCreated
                    };
                case WithdrawalStatus.Reverted:
                    return new WithdrawalReverted(wdId, amnt, dtCreated, uId, wdMadeBy, status, remark, transNum, uname)
                    {
                        EventCreated = dtCreated
                    };
                case WithdrawalStatus.Investigation:
                    return new WithdrawalInvestigated(wdId, amnt, dtCreated, uId, wdMadeBy, status, remark, transNum, uname)
                    {
                        EventCreated = dtCreated
                    };
                case WithdrawalStatus.Documents:
                    return new WithdrawalDocumentsChecked(wdId, amnt, dtCreated, uId, wdMadeBy, status, remark, transNum, uname)
                    {
                        EventCreated = dtCreated
                    };
                case WithdrawalStatus.Verified:
                    return new WithdrawalVerified(wdId, amnt, dtCreated, uId, wdMadeBy, status, remark, transNum, uname)
                    {
                        EventCreated = dtCreated
                    };
                case WithdrawalStatus.Unverified:
                    return new WithdrawalUnverified(wdId, amnt, dtCreated, uId, wdMadeBy, status, remark, transNum, uname)
                    {
                        EventCreated = dtCreated
                    };
                case WithdrawalStatus.New:
                    return new WithdrawalCreated(wdId, amnt, dtCreated, uId, wdMadeBy, status, remark, transNum, uname)
                    {
                        EventCreated = dtCreated
                    };
                case WithdrawalStatus.Rejected:
                    return new WithdrawalRejected(wdId, amnt, dtCreated, uId, wdMadeBy, status, remark, transNum, uname)
                    {
                        EventCreated = dtCreated
                    };                    

                default:
                    return null;
            }
        }
        #endregion

        #region Deposit

        public void Handle(DepositConfirmed @event)
        {
            AddActivityLog(AdminActivityLogCategory.Deposit, @event);
        }

        public void Handle(DepositVerified @event)
        {
            AddActivityLog(AdminActivityLogCategory.Deposit, @event);
        }

        public void Handle(DepositUnverified @event)
        {
            AddActivityLog(AdminActivityLogCategory.Deposit, @event);
        }

        public void Handle(DepositApproved @event)
        {
            AddActivityLog(AdminActivityLogCategory.Deposit, @event);
        }

        public void Handle(DepositRejected @event)
        {
            AddActivityLog(AdminActivityLogCategory.Deposit, @event);
        }
        #endregion

        #region Role

        public void Handle(RoleUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Role, @event);
        }

        public void Handle(RoleCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Role, @event);
        }

        #endregion

        #region Admin

        public void Handle(AdminCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.User, @event);
        }

        public void Handle(AdminUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.User, @event);
        }

        public void Handle(AdminActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.User, @event);
        }

        public void Handle(AdminDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.User, @event);
        }

        public void Handle(AdminPasswordChanged @event)
        {
            AddActivityLog(AdminActivityLogCategory.User, @event);
        }

        #endregion

        #region Game

        public void Handle(GameCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Game, @event);
        }

        public void Handle(GameUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Game, @event);
        }

        public void Handle(GameDeleted @event)
        {
            AddActivityLog(AdminActivityLogCategory.Game, @event);
        }

        #endregion

        #region Product

        public void Handle(ProductCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Product, @event);
        }

        public void Handle(ProductUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Product, @event);
        }

        public void Handle(BetLimitCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Product, "Bet Limit Created", @event, @event.EventCreatedBy, null);
        }

        public void Handle(BetLimitDeleted @event)
        {
            AddActivityLog(AdminActivityLogCategory.Product, "Bet Limit Deleted", @event, @event.EventCreatedBy, null);
        }

        public void Handle(BetLimitUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Product, "Bet Limit Updated", @event, @event.EventCreatedBy, null);
        }
        
        #endregion

        #region IdentificationDocumentSettings
        public void Handle(IdentificationDocumentSettingsCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.IdentificationDocumentSettings, @event);
        }

        public void Handle(IdentificationDocumentSettingsUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.IdentificationDocumentSettings, @event);
        }

        public void Handle(IdentityDocumentUploaded @event)
        {
            AddActivityLog(AdminActivityLogCategory.IdentityVerification, @event);
        }

        public void Handle(IdentityDocumentVerified @event)
        {
            AddActivityLog(AdminActivityLogCategory.IdentityVerification, @event);
        }

        public void Handle(IdentityDocumentUnverified @event)
        {
            AddActivityLog(AdminActivityLogCategory.IdentityVerification, @event);
        }

        #endregion

        #region Messaging
        public void Handle(MassMessageSendRequestedEvent @event)
        {
            AddActivityLog(AdminActivityLogCategory.Messaging, @event);
        }

        public void Handle(MessageTemplateAddedEvent @event)
        {
            AddActivityLog(AdminActivityLogCategory.Messaging, @event);
        }

        public void Handle(MessageTemplateEditedEvent @event)
        {
            AddActivityLog(AdminActivityLogCategory.Messaging, @event);
        }

        public void Handle(MessageTemplateActivatedEvent @event)
        {
            AddActivityLog(AdminActivityLogCategory.Messaging, @event);
        }
        #endregion

        public void Handle(DepositSubmitted @event)
        {
            AddActivityLog(AdminActivityLogCategory.Deposit, @event);
        }

        public void Handle(SignUpFraudTypeCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.SignUpFraudType, @event);
        }

        public void Handle(SignUpFraudTypeUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.SignUpFraudType, @event);
        }

        public void Handle(PlayerFrozen @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(PlayerUnfrozen @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(PlayerLocked @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(PlayerUnlocked @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(DuplicateMechanismConfigurationCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.DuplicateMechanismConfiguration, @event);
        }

        public void Handle(DuplicateMechanismConfigurationUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.DuplicateMechanismConfiguration, @event);
        }

        #region Payment Gateway Settings
        public void Handle(PaymentGatewaySettingCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentGatewaySetings, @event);
        }

        public void Handle(PaymentGatewaySettingUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentGatewaySetings, @event);
        }

        public void Handle(PaymentGatewaySettingActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentGatewaySetings, @event);
        }

        public void Handle(PaymentGatewaySettingDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentGatewaySetings, @event);
        }
        #endregion

        public void Handle(RiskProfileCheckConfigCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.RiskProfileCheck, @event);
        }

        public void Handle(RiskProfileCheckConfigUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.RiskProfileCheck, @event);
        }
    }
}

