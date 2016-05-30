namespace AFT.RegoV2.Core.Fraud.Data
{
    public enum AVCConfigurationValidationMessagesEnum
    {
        RecordWithTheSameCompositeKeyAlreadyExists,
        AtLeastOnePaymentLevelIsNeeded,
        AvcMissingInTheDbOrItsStatusIsAlreadyActive,
        AvcMissingInTheDbOrItsStatusIsAlreadyInactive
    }
}
