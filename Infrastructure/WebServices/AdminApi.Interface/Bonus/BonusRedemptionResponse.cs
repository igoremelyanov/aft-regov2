using System;

namespace AFT.RegoV2.AdminApi.Interface.Bonus
{
    public class BonusRedemptionResponse
    {
        public string LicenseeName { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public string Username { get; set; }
        public string BonusName { get; set; }
        public string ActivationState { get; set; }
        public string RolloverState { get; set; }
        public string Amount { get; set; }
        public string LockedAmount { get; set; }
        public string Rollover { get; set; }
    }
}
