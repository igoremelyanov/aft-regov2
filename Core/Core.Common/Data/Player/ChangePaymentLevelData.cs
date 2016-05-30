using System;

namespace AFT.RegoV2.Core.Common.Data.Player
{
    public class ChangePaymentLevelData
    {
        public Guid PlayerId { get; set; }
        public Guid PaymentLevelId { get; set; }
        public string Remarks { get; set; }
    }
}