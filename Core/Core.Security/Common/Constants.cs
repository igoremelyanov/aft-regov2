using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AFT.RegoV2.Core.Security.Common
{
    public static class ConstantsHelper
    {
        public static Dictionary<string, string> GetConstantsDictionary<T>()
        {
            var type = typeof(T);

            var fieldInfos = type.GetFields(
                BindingFlags.Public | BindingFlags.Static |

                // This tells it to get the fields from all base types as well
                BindingFlags.FlattenHierarchy);

            var dictionary = fieldInfos
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .ToDictionary(fi => fi.Name, fi => fi.GetRawConstantValue().ToString());

            return dictionary;
        }
    }

    public class Permissions
    {
        public const string View = "View";
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Activate = "Activate";
        public const string Deactivate = "Deactivate";
        public const string Search = "Search";
        public const string Export = "Export";
        public const string Confirm = "Confirm";
        public const string Verify = "Verify";
        public const string Unverify = "Unverify";
        public const string Approve = "Approve";
        public const string Reject = "Reject";
        public const string AssignVipLevel = "AssignVipLevel";
        public const string AssignPaymentLevel = "AssignPaymentLevel";
        public const string Pass = "Pass";
        public const string Fail = "Fail";
        public const string Accept = "Accept";
        public const string Revert = "Revert";
        public const string Exempt = "Exempt";
        public const string RenewContract = "RenewContract";
        public const string Send = "Send";
        public const string ViewEmail = "ViewEmail";
        public const string ViewMobile = "ViewMobile";
    }

    public class Modules
    {
        public const string AdminManager = "AdminManager";
        public const string RoleManager = "RoleManager";
        public const string BackendIpRegulationManager = "BackendIpRegulationManager";
        public const string BrandIpRegulationManager = "BrandIpRegulationManager";
        public const string LanguageManager = "LanguageManager";
        public const string CountryManager = "CountryManager";
        public const string BrandManager = "BrandManager";
        public const string LicenseeManager = "LicenseeManager";
        public const string SupportedCurrencies = "SupportedCurrencies";
        public const string CurrencyManager = "CurrencyManager";
        public const string MessageTemplateManager = "MessageTemplateManager";
        public const string MassMessageTool = "MassMessageTool";
        //AFTREGO-4651
        //public const string ExchangeRateManager = "ExchangeRateManager";

        #region Withdrawal
        public const string OfflineWithdrawalRequest = "OfflineWithdrawalRequest";
        public const string OfflineWithdrawalWagerCheck = "OfflineWithdrawalWagerCheck";
        public const string OfflineWithdrawalExemption = "OfflineWithdrawalExemption";
        public const string OfflineWithdrawalVerification = "OfflineWithdrawalVerification";
        public const string OfflineWithdrawalAcceptance = "OfflineWithdrawalAcceptance";
        public const string OfflineWithdrawalApproval = "OfflineWithdrawalApproval";
        public const string OfflineWithdrawalOnHold = "OfflineWithdrawalOnHold";
        public const string OfflineWithdrawalInvestigation = "OfflineWithdrawalInvestigation";
        #endregion

        public const string PlayerBankAccount = "PlayerBankAccount";

        public const string OfflineDepositRequests = "OfflineDepositRequests";
        public const string OfflineDepositConfirmation = "OfflineDepositConfirmation";
        public const string DepositVerification = "DepositVerification";
        public const string DepositApproval = "DepositApproval";

        public const string BonusTemplateManager = "BonusTemplateManager";
        public const string BonusManager = "BonusManager";

        #region player manager
        public const string PlayerManager = "PlayerManager";
        public const string PaymentLevelSettings = "PaymentLevelSettings";
        #endregion

        #region Reports

        // Admin
        public const string AdminActivityLog = "AdminActivityLog";
        public const string AdminAuthenticationLog = "AdminAuthenticationLog";

        // Player
        public const string PlayerReport = "PlayerReport";
        public const string PlayerBetHistoryReport = "PlayerBetHistoryReport";

        // Payment
        public const string DepositReport = "DepositReport";

        // Brand
        public const string BrandReport = "BrandReport";
        public const string LicenseeReport = "LicenseeReport";
        //AFTREGO-4690
        //public const string LanguageReport = "LanguageReport";
        public const string VipLevelReport = "VipLevelReport";

        #endregion

        #region Vip Level Manager

        public const string VipLevelManager = "VipLevelManager";

        #endregion

        public const string GameManager = "GameManager";

        #region Fraud

        public const string FraudManager = "FraudManager";
        public const string WagerConfiguration = "WagerConfiguration";
        public const string AutoVerificationConfiguration = "AutoVerificationConfiguration";

        #endregion

        public const string SupportedLanguages = "SupportedLanguages";
        public const string SupportedCountries = "SupportedCountries";
        public const string SupportedProducts = "SupportedProducts";

        public const string Banks = "Banks";
        public const string BankAccounts = "BankAccounts";
        public const string PaymentLevelManager = "PaymentLevelManager";
        public const string PaymentSettings = "PaymentSettings";
        public const string TransferSettings = "TransferSettings";
        public const string PaymentGatewaySettings = "PaymentGatewaySettings";

        public const string TranslationManager = "TranslationManager";

        public const string MemberAuthenticationLog = "MemberAuthenticationLog";

        public const string WalletManager = "WalletManager";

        public const string BetLevels = "BetLevels";

        public const string ProductManager = "ProductManager";

        public const string EmailTemplate = "EmailTemplate";
        public const string IdentificationDocumentSettings = "IdentificationDocumentSettings";
        public const string RiskProfileCheckConfiguration = "RiskProfileCheckConfiguration";

        public const string ResponsibleGambling = "ResponsibleGambling";
        public const string SignUpFraudTypes = "SignUpFraudTypes";
        public const string DuplicateConfiguration = "DuplicateConfiguration";
    }

    public class IpRegulationConstants
    {
        public class BlockingTypes
        {
            public const string Redirection = "Redirection";
            public const string LoginRegistration = "Login/Registration";
        }
    }
}
