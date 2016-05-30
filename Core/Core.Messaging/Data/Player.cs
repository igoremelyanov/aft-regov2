using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFT.RegoV2.Core.Messaging.Data
{
    public class Player
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        [ForeignKey("Language")]
        public string LanguageCode { get; set; }
        public Language Language { get; set; }
        [ForeignKey("Brand")]
        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }
        [ForeignKey("VipLevel")]
        public Guid VipLevelId { get; set; }
        public VipLevel VipLevel { get; set; }
        [ForeignKey("PaymentLevel")]
        public Guid? PaymentLevelId { get; set; }
        public PaymentLevel PaymentLevel { get; set; }
        public bool AccountAlertEmail { get; set; }
        public bool AccountAlertSms { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset DateRegistered { get; set; }
    }
}