namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public enum OnlineDepositStatus
    {
        New,
        /// <summary>
        /// Online deposit has been submitted by Player but not confirmed by Payment Processor
        /// </summary>
        Processing,
        /// <summary>
        /// Online deposit has been confirmed by Payment Processor
        /// </summary>
        Approved,

        Verified,
        Rejected,
        Unverified
    }
}