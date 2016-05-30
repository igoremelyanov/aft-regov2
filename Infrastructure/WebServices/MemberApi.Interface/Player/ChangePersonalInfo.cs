using System;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class ChangePersonalInfoRequest 
    {
        public Guid PlayerId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string CurrencyCode { get; set; }
        public string CountryCode { get; set; }
        public string MailingAddressCity { get; set; }
        public string MailingAddressLine1 { get; set; }
        public string MailingAddressPostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public bool AccountAlertEmail { get; set; }
        public bool AccountAlertSms {get;set;}
        public bool MarketingAlertEmail { get; set; }
        public bool MarketingAlertPhone { get; set; }
        public bool MarketingAlertSms { get; set; }
    }

    public class ChangePersonalInfoResponse 
    {
        public string UriToUserWithPersonalInfoUpdated { get; set; }
    }
}