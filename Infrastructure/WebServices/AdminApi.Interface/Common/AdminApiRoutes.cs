namespace AFT.RegoV2.AdminApi.Interface.Common
{
    public static class AdminApiRoutes
    {
        #region AdminIpRegulationsController

        public const string IsIpAddressUniqueInAdminIpRegulations = "AdminIpRegulations/IsIpAddressUnique";
        public const string IsIpAddressBatchUniqueInAdminIpRegulations = "AdminIpRegulations/IsIpAddressBatchUnique";
        public const string ListAdminIpRegulations = "AdminIpRegulations/List";
        public const string GetEditDataInAdminIpRegulations = "AdminIpRegulations/GetEditData";
        public const string CreateIpRegulationInAdminIpRegulations = "AdminIpRegulations/CreateIpRegulation";
        public const string UpdateIpRegulationInAdminIpRegulations = "AdminIpRegulations/UpdateIpRegulation";
        public const string DeleteIpRegulationInAdminIpRegulations = "AdminIpRegulations/DeleteIpRegulation";

        #endregion

        #region AdminActivityLogController

        public const string ListAdminActivityLog = "AdminActivityLog/Data";

        #endregion

        #region AdminManagerController

        public const string ListUsers = "AdminManager/Data";
        public const string CreateUserInAdminManager = "AdminManager/CreateUser";
        public const string UpdateUserInAdminManager = "AdminManager/UpdateUser";
        public const string ResetPasswordInAdminManager = "AdminManager/ResetPassword";
        public const string ActivateUserInAdminManager = "AdminManager/Activate";
        public const string DeactivateUserInAdminManager = "AdminManager/Deactivate";
        public const string GetUserEditDataInAdminManager = "AdminManager/GetEditData";
        public const string GetLicenseeDataInAdminManager = "AdminManager/GetLicenseeData";
        public const string SaveBrandFilterSelectionInAdminManager = "AdminManager/SaveBrandFilterSelection";
        public const string SaveLicenseeFilterSelectionInAdminManager = "AdminManager/SaveLicenseeFilterSelection";

        #endregion

        #region AuthenticationLogController

        public const string ListAdminAuthenticationLogs = "AuthenticationLog/AdminAuthData";
        public const string ListMemberAuthenticationLogs = "AuthenticationLog/MemberAuthData";

        #endregion

        #region BonusController

        public const string ListBonuses = "Bonus/Data";
        public const string ChangeBonusStatus = "Bonus/ChangeStatus";
        public const string GetBonusRelatedData = "Bonus/GetRelatedData";
        public const string CreateUpdateBonus = "Bonus/CreateUpdate";

        #endregion

        #region BonusTemplateController

        public const string ListBonusTemplates = "BonusTemplate/Data";
        public const string GetBonusTemplateRelatedData = "BonusTemplate/GetRelatedData";
        public const string CreateEditBonusTemplate = "BonusTemplate/CreateEdit";
        public const string DeleteBonusTemplate = "BonusTemplate/Delete";

        #endregion

        #region BonusHistoryController
        public const string ListBonusRedemptions = "BonusHistory/Data";
        public const string GetBonusRedemption = "BonusHistory/Get";
        public const string ListRedemptionEvents = "BonusHistory/GetRedemptionEvents";
        public const string CancelBonusRedemption = "BonusHistory/Cancel";
        #endregion

        #region IssueBonusController
        public const string ListIssueBonuses = "IssueBonus/Data";
        public const string GetIssueBonusTransactions = "IssueBonus/Transactions";
        public const string IssueBonus = "IssueBonus/IssueBonus";
        #endregion

        #region BrandController

        public const string ListBrands = "Brand/GetBrands";
        public const string GetUserBrands = "Brand/GetUserBrands";
        public const string GetBrandAddData = "Brand/GetAddData";
        public const string GetBrandEditData = "Brand/GetEditData";
        public const string GetBrandViewData = "Brand/GetViewData";
        public const string AddBrand = "Brand/Add";
        public const string EditBrand = "Brand/Edit";
        public const string GetBrandCountries = "Brand/GetCountries";
        public const string ActivateBrand = "Brand/Activate";
        public const string DeactivateBrand = "Brand/Deactivate";
        public const string GetBrands = "Brand/Brands";

        #endregion

        #region BrandIpRegulationsController

        public const string IsIpAddressUniqueInBrandIpRegulations = "BrandIpRegulations/IsIpAddressUnique";
        public const string IsIpAddressBatchUniqueInBrandIpRegulations = "BrandIpRegulations/IsIpAddressBatchUnique";
        public const string GetLicenseeBrandsInBrandIpRegulations = "BrandIpRegulations/GetLicenseeBrands";
        public const string ListBrandIpRegulations = "BrandIpRegulations/List";
        public const string GetEditDataInBrandIpRegulations = "BrandIpRegulations/GetEditData";
        public const string CreateIpRegulationInBrandIpRegulations = "BrandIpRegulations/CreateIpRegulation";
        public const string UpdateIpRegulationInBrandIpRegulations = "BrandIpRegulations/UpdateIpRegulation";
        public const string DeleteIpRegulationInBrandIpRegulations = "BrandIpRegulations/DeleteIpRegulation";

        #endregion

        #region BrandCountryController

        public const string ListBrandCountries = "BrandCountry/List";
        public const string GetBrandCountryAssignData = "BrandCountry/GetAssignData";
        public const string AssignBrandCountry = "BrandCountry/Assign";

        #endregion

        #region BrandCultureController

        public const string ListBrandCultures = "BrandCulture/List";
        public const string GetBrandCultureAssignData = "BrandCulture/GetAssignData";
        public const string AssignBrandCulture = "BrandCulture/Assign";

        #endregion

        #region BrandCurrencyController

        public const string ListBrandCurrencies = "BrandCurrency/List";
        public const string GetBrandCurrencies = "BrandCurrency/GetBrandCurrencies";
        public const string GetBrandCurrenciesWithNames = "BrandCurrency/GetBrandCurrenciesWithNames";
        public const string GetBrandCurrencyAssignData = "BrandCurrency/GetAssignData";
        public const string AssignBrandCurrency = "BrandCurrency/Assign";

        #endregion

        #region BrandProductController

        public const string ListBrandProducts = "BrandProduct/List";
        public const string GetBrandProductAssignData = "BrandProduct/GetAssignData";
        public const string AssignBrandProduct = "BrandProduct/Assign";
        public const string GetBrandProductBetLevels = "BrandProduct/BetLevels";
        public const string UpdateBrandProductSettings = "BrandProduct/ProductSettings";

        #endregion

        #region ContentTranslationController

        public const string GetContentTranslations = "ContentTranslation/GetContentTranslations";
        public const string CreateContentTranslation = "ContentTranslation/CreateContentTranslation";
        public const string UpdateContentTranslation = "ContentTranslation/UpdateContentTranslation";
        public const string ActivateContentTranslation = "ContentTranslation/Activate";
        public const string DeactivateContentTranslation = "ContentTranslation/Deactivate";
        public const string DeleteContentTranslation = "ContentTranslation/DeleteContentTranslation";
        public const string GetContentTranslationAddData = "ContentTranslation/GetContentTranslationAddData";
        public const string GetContentTranslationEditData = "ContentTranslation/GetContentTranslationEditData";

        #endregion

        #region CountryController

        public const string ListCountries = "Country/List";
        public const string GetCountryByCode = "Country/GetByCode";
        public const string SaveCountry = "Country/Save";
        public const string DeleteCountry = "Country/Delete";

        #endregion

        #region CultureController

        public const string ListCulture = "Culture/List";
        public const string GetCultureByCode = "Culture/GetByCode";
        public const string ActivateCulture = "Culture/Activate";
        public const string DeactivateCulture = "Culture/Deactivate";
        public const string SaveCulture = "Culture/Save";

        #endregion

        #region CurrencyController

        public const string ListCurrencies = "Currency/List";
        public const string GetCurrencyByCode = "Currency/GetByCode";
        public const string ActivateCurrency = "Currency/Activate";
        public const string DeactivateCurrency = "Currency/Deactivate";
        public const string SaveCurrency = "Currency/Save";

        #endregion

        #region IdentificationDocumentSettingsController

        public const string ListIdentificationDocumentSettings = "IdentificationDocumentSettings/Data";
        public const string CreateSettingInIdentificationDocumentSettings = "IdentificationDocumentSettings/CreateSetting";
        public const string GetEditDataInIdentificationDocumentSettings = "IdentificationDocumentSettings/GetEditData";
        public const string UpdateSettingInIdentificationDocumentSettings = "IdentificationDocumentSettings/UpdateSetting";
        public const string GetLicenseeBrandsInIdentificationDocumentSettings = "IdentificationDocumentSettings/GetLicenseeBrands";
        public const string GetPaymentMethodsInIdentificationDocumentSettings = "IdentificationDocumentSettings/GetPaymentMethods";

        #endregion

        #region MassMessageController

        public const string GetNewDataInMassMessage = "MassMessage/GetNewData";
        public const string SearchPlayersListInMassMessage = "MassMessage/SearchPlayersList";
        public const string RecipientsListInMassMessage = "MassMessage/RecipientsList";
        public const string UpdateRecipientsInMassMessage = "MassMessage/UpdateRecipients";
        public const string SendMassMessage = "MassMessage/Send";
        
        #endregion

        #region PlayerManagerController

        public const string ListPlayers = "PlayerManager/Data";
        public const string GetAddPlayerDataInPlayerManager = "PlayerManager/GetAddPlayerData";
        public const string GetAddPlayerBrandsInPlayerManager = "PlayerManager/GetAddPlayerBrands";
        public const string GetAddPlayerBrandDataInPlayerManager = "PlayerManager/GetAddPlayerBrandData";
        public const string GetPaymentLevelsInPlayerManager = "PlayerManager/GetPaymentLevels";
        public const string GetVipLevelsInPlayerManager = "PlayerManager/GetVipLevels";
        public const string ChangeVipLevelInPlayerManager = "PlayerManager/ChangeVipLevel";
        public const string ChangePaymentLevelInPlayerManager = "PlayerManager/ChangePaymentLevel";
        public const string ChangePlayersPaymentLevelInPlayerManager = "PlayerManager/ChangePlayersPaymentLevel";
        public const string SendNewPasswordInPlayerManager = "PlayerManager/SendNewPassword";
        public const string AddPlayerInPlayerManager = "PlayerManager/Add";
        public const string GetPlayerForBankAccountInPlayerManager = "PlayerManager/GetPlayerForBankAccount";
        public const string GetBankAccountInPlayerManager = "PlayerManager/GetBankAccount";
        public const string GetCurrentBankAccountInPlayerManager = "PlayerManager/GetCurrentBankAccount";
        public const string SaveBankAccountInPlayerManager = "PlayerManager/SaveBankAccount";
        public const string SetCurrentBankAccountInPlayerManager = "PlayerManager/SetCurrentBankAccount";

        #endregion

        #region PlayerInfoController

        public const string ListDepositTransactionsInPlayerInfo = "PlayerInfo/DepositTransactions";
        public const string ListWithdrawTransactionsInPlayerInfo = "PlayerInfo/WithdrawTransactions";
        public const string ListTransactionsInPlayerInfo = "PlayerInfo/Transactions";
        public const string ListTransactionsAdvInPlayerInfo = "PlayerInfo/TransactionsAdv";
        public const string ListActivityLogsInPlayerInfo = "PlayerInfo/ActivityLog";
        public const string ListIdentityVerificationsInPlayerInfo = "PlayerInfo/IdentityVerification";
        public const string EditLogRemarkInPlayerInfo = "PlayerInfo/EditLogRemark";
        public const string GetBalancesInPlayerInfo = "PlayerInfo/GetBalances";
        public const string GetTransactionTypesInPlayerInfo = "PlayerInfo/GetTransactionTypes";
        public const string GetWalletTemplatesInPlayerInfo = "PlayerInfo/GetWalletTemplates";
        public const string GetStatusInPlayerInfo = "PlayerInfo/GetStatus";
        public const string GetPlayerTitleInPlayerInfo = "PlayerInfo/GetPlayerTitle";
        public const string GetIdentificationDocumentEditDataInPlayerInfo = "PlayerInfo/GetIdentificationDocumentEditData";
        public const string UploadIdInPlayerInfo = "PlayerInfo/UploadId";
        public const string GetPlayerInPlayerInfo = "PlayerInfo/Get";
        public const string GetExemptionDataInPlayerInfo = "PlayerInfo/GetExemptionData";
        public const string EditPlayerInfo = "PlayerInfo/Edit";
        public const string SubmitExemptionInPlayerInfo = "PlayerInfo/SubmitExemption";
        public const string SetStatusInPlayerInfo = "PlayerInfo/SetStatus";
        public const string SetFreezeStatusInPlayerInfo = "PlayerInfo/SetFreezeStatus";
        public const string UnlockPlayerInPlayerInfo = "PlayerInfo/Unlock";
        public const string CancelExclusionInPlayerInfo = "PlayerInfo/CancelExclusion";
        public const string VerifyIdDocumentInPlayerInfo = "PlayerInfo/VerifyIdDocument";
        public const string UnverifyIdDocumentInPlayerInfo = "PlayerInfo/UnverifyIdDocument";
        public const string ResendActivationEmailInPlayerInfo = "PlayerInfo/ResendActivationEmail";

        #endregion

        #region PaymentLevelSettingsController
        public const string ListPlayersInPaymentLevelSettings = "PaymentLevelSettings/List";
        #endregion

        #region RoleManagerController

        public const string ListRoles = "RoleManager/Data";
        public const string CreateRoleInRoleManager = "RoleManager/CreateRole";
        public const string GetRoleInRoleManager = "RoleManager/GetRole";
        public const string GetEditDataInRoleManager = "RoleManager/GetEditData";
        public const string UpdateRoleInRoleManager = "RoleManager/UpdateRole";
        public const string GetLicenseeDataInRoleManager = "RoleManager/GetLicenseeData";

        #endregion

        #region PaymentGatewaySettingController
        public const string ListPaymentGatewaySettings = "PaymentGatewaySettings/List";
        public const string AddPaymentGatewaySettings = "PaymentGatewaySettings/Add";
        public const string EditPaymentGatewaySettings = "PaymentGatewaySettings/Edit";
        public const string ActivatePaymentGatewaySettings = "PaymentGatewaySettings/Activate";
        public const string DeactivatePaymentGatewaySettings = "PaymentGatewaySettings/Deactivate";
        public const string GetPaymentGatewaysInPaymentGatewaySettings = "PaymentGatewaySettings/GetPaymentGateways";
        public const string GetPaymentGatewaySettingsById = "PaymentGatewaySettings/GetById";
        #endregion 

        #region OnlineDepositController
        public const string GetOnlineDepositById = "OnlineDeposit/GetById";
        public const string VerifyOnlineDeposit = "OnlineDeposit/Verify";
        public const string ApproveOnlineDeposit = "OnlineDeposit/Approve";
        public const string RejectOnlineDeposit = "OnlineDeposit/Reject";
        public const string UnverifyOnlineDeposit = "OnlineDeposit/Unverify";
        #endregion

        #region OnfflineDepositController
        public const string GetOfflineDepositById = "OfflineDeposit/GetById";
        public const string CreateOfflineDeposit = "OfflineDeposit/Create";
        public const string ConfirmOfflineDeposit = "OfflineDeposit/Confirm";
        public const string VerifyOfflineDeposit = "OfflineDeposit/Verify";
        public const string UnverifyOfflineDeposit = "OfflineDeposit/Unverify";
        public const string ApproveOfflineDeposit = "OfflineDeposit/Approve";
        public const string RejectOfflineDeposit = "OfflineDeposit/Reject";
        #endregion

        #region BanksController
        public const string ListBank = "Banks/List";
        public const string AddBank = "Banks/Add";
        public const string EditBank = "Banks/Edit";
        public const string GetBankById = "Banks/GetById";
        #endregion

        #region BankAccountsController
        public const string ListBankAccount = "BankAccounts/List";
        public const string AddBankAccount = "BankAccounts/Add";
        public const string EditBankAccount = "BankAccounts/Edit";
        public const string ActivateBankAccount = "BankAccounts/Activate";
        public const string DeactivateBankAccount = "BankAccounts/Deactivate";
        public const string GetBankAccountById = "BankAccounts/GetById";
        public const string GetCurrencyByUserId = "BankAccounts/GetCurrencyByUserId";
        public const string GetBankAccountTypes = "BankAccounts/GetBankAccountTypes";
        public const string GetBankListByBrandId = "BankAccounts/GetBankListByBrandId";
        public const string GetBankAccountsByPlayerId = "BankAccounts/GetBankAccountsByPlayerId";
        #endregion

        #region PlayerBankAccountController
        public const string VerifyPlayerBankAccount = "PlayerBankAccounts/Verify";
        public const string RejectPlayerBankAccount = "PlayerBankAccounts/Reject";
        #endregion

        #region PaymentLevelController
        public const string GetPaymentLevels = "PaymentLevel/GetPaymentLevels";
        #endregion
    }
}