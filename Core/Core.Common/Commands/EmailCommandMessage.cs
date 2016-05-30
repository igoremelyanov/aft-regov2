using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Commands
{
    public class EmailCommandMessage : ICommand
    {
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public EmailCommandMessage(
            string senderEmail,
            string senderName,
            string recipientEmail,
            string recipientName,
            string subject,
            string body)
        {
            SenderEmail = senderEmail;
            SenderName = senderName;
            RecipientEmail = recipientEmail;
            RecipientName = recipientName;
            Subject = subject;
            Body = body;
        }

        public EmailCommandMessage()
        {
        }
    }
}