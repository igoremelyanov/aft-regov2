using AFT.RegoV2.Core.Messaging.Interface.Events;
using AFT.RegoV2.Core.Player.ApplicationServices;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Report.ApplicationServices.EventHandlers.PlayerActivityLog
{
    public class MessagingActivityLogEventHandlers : PlayerActivityLogEventHandlersBase
    {
        public MessagingActivityLogEventHandlers(IUnityContainer container)
            : base(container)
        {
            Category = "Messaging";
        }

        public void Handle(OnSiteMessageSentEvent @event)
        {
            AddActivityLog("On Site Message Sent", @event, @event.PlayerId);
        }
    }
}
