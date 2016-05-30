using AFT.RegoV2.Bonus.Core.Models.Events.Redemption;
using AFT.RegoV2.Core.Player.ApplicationServices;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Report.ApplicationServices.EventHandlers.PlayerActivityLog
{
    public class BonusActivityLogEventHandlers : PlayerActivityLogEventHandlersBase
    {
        public BonusActivityLogEventHandlers(IUnityContainer container)
            : base(container)
        {
            Category = "Bonus";
        }

        public void Handle(RedemptionClaimed @event)
        {
            AddActivityLog("Bonus Issued", @event, @event.PlayerId);
        }
    }
}
