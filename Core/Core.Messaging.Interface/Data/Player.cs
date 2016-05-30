using System;

namespace AFT.RegoV2.Core.Messaging.Interface.Data
{
    public class Player
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageName { get; set; }
        public Guid? PaymentLevelId { get; set; }
        public string PaymentLevelName { get; set; }
        public Guid VipLevelId { get; set; }
        public string VipLevelName { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool AccountAlertEmail { get; set; }
        public bool AccountAlertSms { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset DateRegistered { get; set; }
    }
}