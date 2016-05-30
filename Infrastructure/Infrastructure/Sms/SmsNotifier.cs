using System.Configuration;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.Interfaces;
using Twilio;

namespace AFT.RegoV2.Infrastructure.Sms
{
    public class SmsNotifier : ISmsNotifier
    {
        public NotificationSentEvent SendSms(string from, string to, string body)
        {
            var notificationSentEvent = new NotificationSentEvent
            {
                Status = NotificationStatus.Send,
                Type = NotificationType.Sms,
                Message = body,
                Reciever = to
            };

            if (!bool.Parse(ConfigurationManager.AppSettings["EnableSms"]))
                return notificationSentEvent;

            var accountSid = ConfigurationManager.AppSettings["Twilio.AccountSid"];
            var authToken = ConfigurationManager.AppSettings["Twilio.AuthToken"];

            var twilioRestClient = new TwilioRestClient(accountSid, authToken);

            var message = twilioRestClient.SendMessage("+" + from, "+" + to, body);

            if (message.RestException != null)
            {
                notificationSentEvent.Status = NotificationStatus.Error;
                notificationSentEvent.ErrorCode = message.RestException.Code;
                notificationSentEvent.ErrorMessage = message.RestException.Message;
            }
            else if (message.ErrorCode.HasValue)
            {
                notificationSentEvent.Status = NotificationStatus.Error;
                notificationSentEvent.ErrorCode = message.ErrorCode.ToString();
                notificationSentEvent.ErrorMessage = message.ErrorMessage;
            }

            return notificationSentEvent;
        }
    }
}