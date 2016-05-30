using System;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.Interface.Commands
{
    public class Deposit : ICommand
    {
        public string ActorName { get; set; }
        public Guid DepositId { get; set; }
        public Guid PlayerId { get; set; }
        public string ReferenceCode { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public DateTimeOffset Approved { get; set; }
        public string ApprovedBy { get; set; }
        public string Remarks { get; set; }
        public decimal DepositWagering { get; set; }
        public DepositType DepositType { get; set; }
    }
}
