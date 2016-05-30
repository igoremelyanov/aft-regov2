using System;

namespace AFT.RegoV2.Core.Common.Data.Brand
{
    public class EditBrandRequest
    {
        public Guid Licensee { get; set; }
        public Guid Brand { get; set; }
        public BrandType Type { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
        public string SmsNumber { get; set; }
        public string WebsiteUrl { get; set; }
        public bool EnablePlayerPrefix { get; set; }
        public string PlayerPrefix { get; set; }
        public PlayerActivationMethod PlayerActivationMethod { get; set; }
        public int InternalAccounts { get; set; }
        public string TimeZoneId { get; set; }
        public string Remarks { get; set; }
    }
}
