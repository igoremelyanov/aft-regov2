using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Payment.Interface.Data;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class PlayerBankAccountRequest
    {
        public Guid? PlayerId { get; set; }
        public Guid Bank { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Branch { get; set; }
    }

    public class PlayerBankAccountResponse
    {
        public Guid Id { get; set; }
        public Core.Payment.Interface.Data.Player Player { get; set;}
        public BankAccountStatus Status { get; set; }
        public Bank Bank { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Branch { get; set; }
        public string SwiftCode { get; set; }
        public string Address { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public DateTime Created { get; set; }
         public DateTime CreatedBy { get; set; }
    }
}
