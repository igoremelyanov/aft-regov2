using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Payment.Interface.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Player.ApplicationServices.EventHandlers.ActivityLog
{
    public class DepositActivityLogEventHandlers : PlayerActivityLogEventHandlersBase
    {
        public DepositActivityLogEventHandlers(IUnityContainer container) : base(container)
        {
            Category = "Deposit";
        }

        public void Handle(DepositSubmitted @event)
        {
            AddActivityLog("Create "+@event.DepositType+" Deposit", @event, @event.PlayerId, @event.Remarks);
        }

        public void Handle(DepositConfirmed @event)
        {
            AddActivityLog("Confirm " + @event.DepositType + " Deposit", @event, @event.PlayerId, @event.Remarks);
        }

        public void Handle(DepositVerified @event)
        {
            AddActivityLog("Verify " + @event.DepositType + " Deposit", @event, @event.PlayerId, @event.Remarks);
        }

        public void Handle(DepositApproved @event)
        {
            AddActivityLog("Approve " + @event.DepositType + " Deposit", @event, @event.PlayerId, @event.Remarks);

            if (@event.DepositWagering != 0)
            {
                AddActivityLog("Deposit Wagering Requirement", @event, @event.PlayerId);
            }
        }

        public void Handle(DepositUnverified @event)
        {
            if (@event.Status == OfflineDepositStatus.Unverified)
            {
                AddActivityLog("Unverify " + @event.DepositType + " Deposit", @event, @event.PlayerId, @event.Remarks);
            }
            else if (@event.Status == OfflineDepositStatus.Rejected)
            {
                AddActivityLog("Reject " + @event.DepositType + " Deposit", @event, @event.PlayerId, @event.Remarks);
            }
        }

        public void Handle(DepositRejected @event)
        {
            AddActivityLog("Reject " + @event.DepositType + " Deposit", @event, @event.PlayerId, @event.Remarks);          
        }
    }
}
