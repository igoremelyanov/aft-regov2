using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Bonus.Core.Models.Events.Management
{
    public class BonusUpdated : DomainEventBase
    {
        public string Description { get; set; }
    }
}
