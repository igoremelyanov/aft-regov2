using System;
using AFT.RegoV2.Core.Common.Events.Notifications;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class WithdrawalFormDataRequest
    {
        public Guid BrandId { get; set; }
    }

    public class WithdrawalFormDataResponse
    {
        public BankData BankAccount { get; set; }
        public PaymentSettingsData PaymentSettings { get; set; }
        public bool SuccesfullyRecieved { get; set; }
    }

    public class WithdrawalRequest
    {
        public decimal Amount { get; set; }
        public NotificationType NotificationType { get; set; }
    }


    public class OfflineWithdrawalResponse
    {
        public string Result { get; set; }
        public string[] Errors { get; set; }
    }

}
