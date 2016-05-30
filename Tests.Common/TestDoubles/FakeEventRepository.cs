using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Common.EventStore.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Event;
using Newtonsoft.Json;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeEventRepository : IEventRepository
    {
        private readonly FakeDbSet<Event> _events = new FakeDbSet<Event>();
  
        public IDbSet<Event> Events { get { return _events; } }

        public IEnumerable<T> GetEvents<T>()
        {
            return Events
                .Where(x => x.DataType == typeof(T).Name)
                .Select(x => JsonConvert.DeserializeObject<T>(x.Data));
        }

        public void SaveEvent(IDomainEvent @event)
        {
            Events.Add(EventRepository.CreateEventData(@event));
        }

        public int SaveChanges()
        {
            return 0;
        }
    }
}
