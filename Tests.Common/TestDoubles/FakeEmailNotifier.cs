using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeEmailNotifier : IEmailNotifier
    {
        public NotificationSentEvent SendEmail(
            string senderEmail, 
            string senderName, 
            string recipientEmail, 
            string recipientName, 
            string subject, 
            string body)
        {
            return new NotificationSentEvent
            {
                Status = NotificationStatus.Send,
                Type = NotificationType.Email,
                Reciever = recipientEmail,
                Subject = subject,
                Message = body
            };
        }
    }
}
