namespace AFT.RegoV2.MemberApi.Interface.Common
{
    public enum ErrorMessagesEnum
    {
        PlayerWithRequestedIdDoesntExist,
        SecurityQuestionNotAvailableForThisPlayer,
        TokenForPlayerWasNotValid,
        TokenExpired,
        WalletTemplatesMissingForThisBrand,
        PlayerWithRequestedUsernameDoesntExist,
        OnSiteMessageWithRequestedIdDoesntExist,

        RedemptionWithSuchIdDoesntExist,
        LobbyIsMissingForThisBrand,
        NoBrandRelatedToThisPlayer,
        BonusForSuchCodeDoesntExist,
        InvalidBrandCode,
        InvalidIpAddress,
        ServiceUnavailable,
        ErrorInTheRequestedAmount,
        ThereIsNoDefaultVipLevelForRequestedBrand,
        RequestedOfflineDepositDoesntExist,
        BankAccountDoesnNotExistForOfflineDeposit,
        NoDepositRelatedToThisTransactionId
    }
}