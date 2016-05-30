using AFT.RegoV2.Core.Common.Events.Notifications;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface ISmsNotifier
    {
        NotificationSentEvent SendSms(string from, string to, string body);
    }
}