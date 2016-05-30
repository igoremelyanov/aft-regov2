using System;
using System.Collections.Generic;

namespace AFT.RegoV2.MemberWebsite.Models
{
    public class OfflineDepositModel
    {
        public string CurrencyCode { get; set; }
        public decimal DepositAmountMin { get; set; }
        public string DepositAmountMinFormatted { get; set; }
        public decimal DepositAmountMax { get; set; }
        public string DepositAmountMaxFormatted { get; set; }
        public decimal DailyMaximumDepositAmount { get; set; }
        public string DailyMaximumDepositAmountFormatted { get; set; }
        public int DailyMaximumDepositCount { get; set; }
        public IEnumerable<BankAccount> BankAccounts { get; set; }
        public IEnumerable<string> AvailableBonuses { get; set; }
        public IEnumerable<QuickSelectAmount> QuickSelectAmounts { get; set; }
    }

    public class BankAccount
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
    }

    public class QuickSelectAmount
    {
        public decimal Amount { get; set; }
        public string AmountFormatted { get; set; }
    }
}