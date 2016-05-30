using System;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class BankAccountDto
    {
        public Guid Id { get; set; }
        public string BankName { get; set; }
        public string AccountName { get; set; }
    }
}
