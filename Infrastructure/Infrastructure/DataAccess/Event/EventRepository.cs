using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Common.EventStore;
using AFT.RegoV2.Core.Common.Extensions;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Event.Migrations;
using Newtonsoft.Json;
using EventData = AFT.RegoV2.Core.Common.EventStore.Data.Event;

namespace AFT.RegoV2.Infrastructure.DataAccess.Event
{
    public class EventRepository : DbContext, IEventRepository
    {
        static EventRepository()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<EventRepository, Configuration>());
        }

        public EventRepository(): base("name=Default") {}

        public void Initialize()
        {
            Database.Initialize(false);
        }

        public IDbSet<EventData> Events { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("event");

            base.OnModelCreating(modelBuilder);
        }

        public IEnumerable<T> GetEvents<T>()
        {
            var typeName = typeof (T).Name;
            var eventsOfType = Events
                .Where(x => x.DataType == typeName)
                .ToList();

            return eventsOfType.Select(x => JsonConvert.DeserializeObject<T>(x.Data));
        }

        public void SaveEvent(IDomainEvent @event)
        {
            Events.Add(CreateEventData(@event));

            try
            {
                SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                if (!ex.HasDuplicatedUniqueValues())
                {
                    throw;
                }
                ex.Entries.Single().State = EntityState.Detached;
            }
        }

        public static EventData CreateEventData (IDomainEvent @event)
        {
            return new EventData
            {
                Id = @event.EventId,
                AggregateId = @event.AggregateId,
                DataType = @event.GetType().Name,
                Data = JsonConvert.SerializeObject(@event, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                }),
                Created = @event.EventCreated
            };
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }
    }
}