using System.Collections.Generic;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class OfflineDepositBankAccountResponse
    {
        public IEnumerable<BankAccountDto> BankAccounts { get; set; }
    }
}
