using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Bonus.Core.Models.Events.Management
{
    public class BonusActivated : DomainEventBase
    {
        public string Remarks { get; set; }
    }
}
