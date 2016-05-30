using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Bonus.Core.Models.Events.Redemption
{
    public class RedemptionNegated : DomainEventBase
    {
        public string[] Reasons { get; set; }
    }
}