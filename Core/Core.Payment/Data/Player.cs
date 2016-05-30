using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFT.RegoV2.Core.Payment.Data
{
    public class Player
    {
        private Guid _id;

        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string DomainName { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CurrencyCode { get; set; }
        public Guid BrandId { get; set; }

        public Brand Brand { get; set; }

        public Guid VipLevelId { get; set; }
        
        public bool HousePlayer { get; set; }
        public bool? ExemptWithdrawalVerification { get; set; }
        public DateTimeOffset? ExemptWithdrawalFrom { get; set; }
        public DateTimeOffset? ExemptWithdrawalTo { get; set; }
        public int? ExemptLimit { get; set; }
        public DateTimeOffset DateRegistered { get; set; }
        public PlayerBankAccount CurrentBankAccount { get; set; }
        public bool IsActive { get; set; }
        public bool IsTimeOut { get; set; }
        public DateTimeOffset? TimeOutEndDate { get; set; }
        public bool IsSelfExclude { get; set; }
        public DateTimeOffset? SelfExcludeEndDate { get; set; }

        [ForeignKey("PlayerId")]
        public PlayerPaymentLevel PlayerPaymentLevel { get; set; }

        public string GetFullName()
        {
            return FirstName + " " + LastName;
        }
    }
}
