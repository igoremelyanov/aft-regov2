using System;

namespace AFT.RegoV2.Core.Player.Interface.Data
{
    public class EditPlayerData
    {
        public Guid PlayerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public PlayerEnums.Title? Title { get; set; }
        public PlayerEnums.Gender? Gender { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string MailingAddressLine1 { get; set; }
        public string MailingAddressLine2 { get; set; }
        public string MailingAddressLine3 { get; set; }
        public string MailingAddressLine4 { get; set; }
        public string MailingAddressCity { get; set; }
        public string MailingAddressPostalCode { get; set; }
        public string MailingAddressStateProvince { get; set; }
        public string PhysicalAddressLine1 { get; set; }
        public string PhysicalAddressLine2 { get; set; }
        public string PhysicalAddressLine3 { get; set; }
        public string PhysicalAddressLine4 { get; set; }
        public string PhysicalAddressCity { get; set; }
        public string PhysicalAddressPostalCode { get; set; }
        public string PhysicalAddressStateProvince { get; set; }
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
        public PlayerEnums.ContactMethod ContactPreference { get; set; }
        public bool AccountAlertEmail { get; set; }
        public bool AccountAlertSms { get; set; }
        public bool MarketingAlertEmail { get; set; }
        public bool MarketingAlertSms { get; set; }
        public bool MarketingAlertPhone { get; set; }
        public Guid PaymentLevelId { get; set; }
    }
}