namespace AFT.RegoV2.Core.Messaging.Interface.Data
{
    public enum MessagingValidationError
    {
        AlreadyActive,
        InvalidBrand,
        InvalidId,
        InvalidLanguage,
        InvalidMessageContent,
        InvalidMessageDeliveryMethod,
        InvalidMessageType,
        InvalidPlayerId,
        InvalidSenderEmail,
        InvalidSenderNumber,
        InvalidUpdateRecipientsType,
        InvalidSubject,
        Required,
        SenderEmailNotApplicable,
        SenderNumberNotApplicable,
        SubjectNotApplicable,
        TemplateNameInUse
    }
}