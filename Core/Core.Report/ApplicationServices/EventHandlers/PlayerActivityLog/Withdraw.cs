using System;
using System.ComponentModel;
using System.Linq;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Interface.Events;
using AFT.RegoV2.Core.Payment;
using AFT.RegoV2.Domain.Payment.Data;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Player.ApplicationServices.EventHandlers.ActivityLog
{
    public class WithdrawActivityLogEventHandlers : PlayerActivityLogEventHandlersBase
    {
        public WithdrawActivityLogEventHandlers(IUnityContainer container) : base(container)
        {
            Category = "Withdraw";
        }

        //This is an event handler that is common for all the possible events for withdrawal
        public void Handle(WithdrawalEvent @event)
        {
            AddActivityLog(GetEnumDescription(@event.Status), @event, @event.WithdrawalMadeBy, @event.Remark);
            StoreIntoWithdrawalHistory(@event);
        }

        private string GetEnumDescription(WithdrawalStatus status)
        {
            var type = typeof(WithdrawalStatus);
            var memInfo = type.GetMember(status.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Any())
                return ((DescriptionAttribute)attributes[0]).Description;
            return "";
        }

        private void StoreIntoWithdrawalHistory(WithdrawalEvent @event)
        {
            var repository = Container.Resolve<IPaymentRepository>();

            repository.OfflineWithdrawalHistories.Add(new OfflineWithdrawalHistory()
            {
                Id = Guid.NewGuid(),
                UserId = @event.UserId,
                Action = @event.Status,
                DateCreated = @event.DateCreated,
                OfflineWithdrawalId = @event.WithdrawalId,
                Remark = @event.Remark,
                TransactionNumber = @event.TransactionNumber,
                //Kristian: TODO this is temporary. I have to remove it from here after Konstantin is ready with Core.Auth. 
                //Kristian: TODO: There's a task related to that refactoring(https://jira.afusion.com/browse/AFTREGO-3615)
                Username = @event.Username
            });

            repository.SaveChanges();
        }
    }
}
