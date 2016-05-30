namespace AFT.RegoV2.Bonus.Api.Interface
{
    public static class Routes
    {
        public const string Token = "/Token";

        //BonusController
        public const string ListBonuses = "Bonus/Data";
        public const string GetBonuses = "Bonus/GetBonuses";
        public const string GetBonusOrNull = "Bonus/GetBonusOrNull";
        public const string ChangeBonusStatus = "Bonus/ChangeStatus";
        public const string CreateUpdateBonus = "Bonus/CreateUpdate";

        //BonusTemplateController
        public const string ListBonusTemplates = "BonusTemplate/Data";
        public const string GetCompletedTemplates = "BonusTemplate/GetCompletedTemplates";
        public const string GetTemplateOrNull = "BonusTemplate/GetTemplateOrNull";
        public const string CreateEditBonusTemplate = "BonusTemplate/CreateEdit";
        public const string DeleteBonusTemplate = "BonusTemplate/Delete";

        //BonusHistoryController
        public const string ListBonusRedemptions = "BonusHistory/Data";
        public const string GetBonusRedemption = "BonusHistory/Get";
        public const string ListRedemptionEvents = "BonusHistory/GetRedemptionEvents";
        public const string CancelBonusRedemption = "BonusHistory/Cancel";

        //IssueBonusController
        public const string ListIssueBonuses = "IssueBonus/Data";
        public const string GetIssueBonusTransactions = "IssueBonus/Transactions";
        public const string IssueBonus = "IssueBonus/IssueBonus";

        //PlayerController
        public const string GetWageringBalance = "Player/GetWageringBalance";
        public const string GetPlayerBalance = "Player/GetPlayerBalance";
        public const string GetClaimableRedemptions = "Player/GetClaimableRedemptions";
        public const string GetDepositQualifiedBonuses = "Player/GetDepositQualifiedBonuses";
        public const string GetVisibleDepositQualifiedBonuses = "Player/GetVisibleDepositQualifiedBonuses";
        public const string GetDepositQualifiedBonusByCode = "Player/GetDepositQualifiedBonusByCode";
        public const string GetDepositBonusApplicationValidation = "Player/GetDepositBonusApplicationValidation";
        public const string ClaimBonusRedemption = "Player/ClaimBonusRedemption";
        public const string ApplyForFundInBonus = "Player/ApplyForFundInBonus";
        public const string ApplyForDepositBonus = "Player/ApplyForDepositBonus";
        public const string GetCompletedBonuses = "Player/GetCompletedBonuses";
        public const string GetBonusesWithIncompleteWagering = "Player/GetBonusesWithIncompleteWagering";

        //AdminController
        public const string GetDepositQualifiedBonusesByAdmin = "Admin/GetDepositQualifiedBonuses";
        public const string EnsureOrWaitUserRegistered = "Admin/EnsureOrWaitUserRegistered";
    }
}