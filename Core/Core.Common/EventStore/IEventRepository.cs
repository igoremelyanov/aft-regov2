using System.Collections.Generic;
using System.Data.Entity;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.EventStore
{
    public interface IEventRepository
    {
        IDbSet<Data.Event> Events { get; }

        IEnumerable<T> GetEvents<T>();
        void SaveEvent(IDomainEvent @event);

        int SaveChanges();
    }
}