using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Infrastructure.Mail
{
    public class EmailNotifier : IEmailNotifier
    {

        private readonly string _host;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _fromEmail;

        public EmailNotifier()
        {
            _host = ConfigurationManager.AppSettings["Smtp.Host"];
            _port = int.Parse(ConfigurationManager.AppSettings["Smtp.Port"]);
            _userName = ConfigurationManager.AppSettings["Smtp.UserName"];
            _password = ConfigurationManager.AppSettings["Smtp.Password"];
            _fromEmail = ConfigurationManager.AppSettings["Smtp.FromEmail"];
        }

        public NotificationSentEvent SendEmail(
            string senderEmail, 
            string senderName, 
            string recipientEmail, 
            string recipientName, 
            string subject, 
            string body)
        {
            var notificationSentEvent = new NotificationSentEvent
            {
                Status = NotificationStatus.Send,
                Type = NotificationType.Email,
                Reciever = recipientEmail,
                Subject = subject,
                Message = body
            };

            if (!bool.Parse(ConfigurationManager.AppSettings["EnableEmails"]))
                return notificationSentEvent;


            try
            {


                using (var mailMessage = new MailMessage
                                         {
                                             From = new MailAddress(_fromEmail, senderName),
                                             Subject = subject,
                                             Body = body,
                                             BodyEncoding = Encoding.UTF8,
                                             IsBodyHtml = true
                                         })
                {

                    mailMessage.To.Add(new MailAddress(recipientEmail, recipientName));


                    using (var smtpClient = new SmtpClient(_host, _port)
                                            {
                                                Credentials = new NetworkCredential(_userName, _password),
                                                EnableSsl = true
                                            })
                    {

                        // After calling SendAsync, you must wait for the e-mail transmission to complete before attempting to send another e-mail message using Send or SendAsync.
                        // https://msdn.microsoft.com/en-us/library/x5x13z6h.aspx

                        smtpClient.Send(mailMessage);
                    }
                }
            }
            catch (SmtpException e)
            {
                notificationSentEvent.Status = NotificationStatus.Error;
                notificationSentEvent.ErrorCode = Enum.GetName(typeof(SmtpStatusCode), e.StatusCode);
                notificationSentEvent.ErrorMessage = e.Message;
            }

            return notificationSentEvent;
        }
    }
}