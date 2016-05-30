using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;

namespace AFT.RegoV2.RegoBus.EventStore
{
    /// <summary>
    /// Stores all domain events in the event store
    /// </summary>
    public class EventStoreSubscriber : IBusSubscriber,
        IConsumes<IDomainEvent>
    {
        private readonly IEventRepository _eventRepository;

        public EventStoreSubscriber(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public void Consume(IDomainEvent @event)
        {
            _eventRepository.SaveEvent(@event);
        }
    }
}
