using System;

namespace AFT.RegoV2.MemberApi.Interface.Payment
{
    public class BankAccountIdSettings
    {
        public Guid Id { get; set; }

        public bool InternetSameBank { get; set; }

        public bool AtmSameBank { get; set; }

        public bool CounterDepositSameBank { get; set; }

        public bool InternetDifferentBank { get; set; }

        public bool AtmDifferentBank { get; set; }

        public bool CounterDepositDifferentBank { get; set; }
    }
}
