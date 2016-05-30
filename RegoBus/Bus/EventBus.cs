using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;

namespace AFT.RegoV2.RegoBus.Bus
{
    public class EventBus : Bus, IEventBus
    {
        public void Publish(IDomainEvent @event)
        {
            base.Publish(@event);
        }
    }
}