using AFT.RegoV2.Core.Common.Events.Notifications;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IEmailNotifier
    {
        NotificationSentEvent SendEmail(
            string senderEmail,
            string senderName,
            string recipientEmail,
            string recipientName,
            string subject,
            string body);
    }
}