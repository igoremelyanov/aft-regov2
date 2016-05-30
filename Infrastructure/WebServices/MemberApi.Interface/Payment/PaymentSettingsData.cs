using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class PaymentSettingsData
    {
        public decimal MinAmountPerTransaction { get; set; }
        public string MinAmountPerTransactionFormatted { get; set; }
        public decimal MaxAmountPerTransaction { get; set; }
        public string MaxAmountPerTransactionFormatted { get; set; }
        public decimal MaxAmountPerDay { get; set; }
        public string MaxAmountPerDayFormatted { get; set; }
        public int DayMaximumDeposit { get; set; }
    }
}
