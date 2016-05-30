namespace AFT.RegoV2.Core.Payment.Validators
{
    public enum TransferFundSettingsErrors
    {
        AlreadyExistsError = 1,
        NotImplementedError,
        MinAmountPerTransactionError,
        MaxAmountPerTransactionError,
        MaxAmountPerDayError,
        MaxTransactionPerDayError,
        MaxTransactionPerWeekError,
        MaxTransactionPerMonthError,
        MaxminAmountPerTransactionError,
        MinAmountPerTransactionErrorAmountPerDay,
        MaxAmountPerTransactionErrorAmountPerDay,
        MaxTransactionPerWeekErrorPerDay,
        MaxTransactionPerMonthErrorPerWeek,
        MaxTransactionPerMonthErrorPerDay,
    }

    public enum PaymentSettingsErrors
    {
        AlreadyExistsError = 1,
        MinAmountPerTransactionError,
        MaxAmountPerTransactionError,
        MaxAmountPerDayError,
        MaxTransactionPerDayError,
        MaxTransactionPerWeekError,
        MaxTransactionPerMonthError,
        MaxminAmountPerTransactionError,
        MinAmountPerTransactionErrorAmountPerDay,
        MaxAmountPerTransactionErrorAmountPerDay,
        MaxTransactionPerWeekErrorPerDay,
        MaxTransactionPerMonthErrorPerWeek,
        MaxTransactionPerMonthErrorPerDay,
        BankAccountNotFound,
        PaymentMethodIsRequired,
        TheSameSettingsActivated
    }

    public enum PaymentGatewaySettingsErrors
    {
        AlreadyExistsError = 1,
        AlphanumericSpaces,
        RequiredField,
        ExceedMaxLength,
        UrlFormatError,
        PaymentGatewaySettingAlreadyExists,
        OnlinePaymentMethodNameAlreadyExists,
        NotFound,
        AlreadyActive,
        NotActive
    }

    public enum DepositErrors
    {
        NotFound
    }
}
