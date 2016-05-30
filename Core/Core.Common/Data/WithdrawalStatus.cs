using System.ComponentModel;

namespace AFT.RegoV2.Domain.Payment.Data
{
    public enum WithdrawalStatus
    {
        [Description("Offline withdrawal created")]
        New,
        [Description("Auto verification failed")]
        AutoVerificationFailed,
        [Description("Offline withdrawal in investigation")]
        Investigation,
        [Description("Offline withdrawal with documents verified")]
        Documents,
        [Description("Offline withdrawal verified")]
        Verified,
        [Description("Offline withdrawal unverified")]
        Unverified,
        [Description("Offline withdrawal on hold")]
        OnHold,
        [Description("Offline withdrawal accepted")]
        Accepted,
        [Description("Offline withdrawal reverted")]
        Reverted,
        [Description("Offline withdrawal approved")]
        Approved,
        [Description("Offline withdrawal rejected")]
        Rejected,
        [Description("Offline withdrawal canceled")]
        Canceled
    }
}