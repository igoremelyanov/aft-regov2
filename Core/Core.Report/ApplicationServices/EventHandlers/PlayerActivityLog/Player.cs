using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Core.Payment.Interface.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Player.ApplicationServices.EventHandlers.ActivityLog
{
    public class PlayerActivityLogEventHandlers : PlayerActivityLogEventHandlersBase
    {
        public PlayerActivityLogEventHandlers(IUnityContainer container)
            : base(container)
        {
            Category = "Player";
        }

        public void Handle(PlayerActivated @event)
        {
            AddActivityLog("Activation performed", @event, @event.PlayerId);
        }

        public void Handle(PlayerDeactivated @event)
        {
            AddActivityLog("Account deactivated", @event, @event.PlayerId);
        }

        public void Handle(PlayerRegistered @event)
        {
            if (@event.IsActive)
            {
                AddActivityLog("Automatic activation performed", @event, @event.PlayerId);
            }
        }

        public void Handle(WithdrawalEvent @event)
        {
            AddActivityLog(string.Format("Withdrawal created. Amount: {0}", @event.Amount), @event, @event.UserId, GetPlayerName(@event.UserId));
        }

        public void Handle(PlayerContactVerified @event)
        {
            if (@event.ContactType == ContactType.Email)
            {
                AddActivityLog("Activation performed by email", @event, @event.PlayerId);
            }
            else if (@event.ContactType == ContactType.Mobile)
            {
                AddActivityLog("Activation performed by sms", @event, @event.PlayerId);
            }
        }

        public void Handle(TransferFundCreated @event)
        {
            AddActivityLog(string.Format("Transfer Fund created. Amount: {0}. Transaction Number: {1}", @event.Amount, @event.TransactionNumber), @event, @event.PlayerId,
                @event.Remarks);
        }

        public void Handle(PlayerPaymentLevelChanged @event)
        {
            AddActivityLog("Payment Level changed", @event, @event.PlayerId,@event.Remarks);
        }
    }
}
