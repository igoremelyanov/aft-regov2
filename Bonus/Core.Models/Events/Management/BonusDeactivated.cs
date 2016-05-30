using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Bonus.Core.Models.Events.Management
{
    public class BonusDeactivated : DomainEventBase
    {
        public string Remarks { get; set; }
    }
}
