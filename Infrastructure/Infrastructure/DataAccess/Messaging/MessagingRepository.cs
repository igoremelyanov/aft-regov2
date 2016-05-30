using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Messaging;
using AFT.RegoV2.Core.Messaging.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Messaging.Mappings;
using Language = AFT.RegoV2.Core.Messaging.Data.Language;
using MessageTemplate = AFT.RegoV2.Core.Messaging.Data.MessageTemplate;

namespace AFT.RegoV2.Infrastructure.DataAccess.Messaging
{
    public class MessagingRepository : DbContext, IMessagingRepository
    {
        public const string Schema = "messaging";

        public IDbSet<MessageTemplate> MessageTemplates { get; set; }
        public IDbSet<Core.Messaging.Data.Brand> Brands { get; set; }
        public IDbSet<Language> Languages { get; set; }
        public IDbSet<PaymentLevel> PaymentLevels { get; set; }
        public IDbSet<VipLevel> VipLevels { get; set; }
        public IDbSet<Core.Messaging.Data.Player> Players { get; set; }
        public IDbSet<MassMessage> MassMessages { get; set; }
        public IDbSet<MassMessageContent> MassMessageContent { get; set; }

        static MessagingRepository()
        {
            Database.SetInitializer(new MessagingRepositoryInitializer());
        }

        public MessagingRepository() : base("name=Default") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new MessageTemplateMap(Schema));
            modelBuilder.Configurations.Add(new LanguageMap(Schema));
            modelBuilder.Configurations.Add(new BrandMap(Schema));
            modelBuilder.Configurations.Add(new PaymentLevelMap(Schema));
            modelBuilder.Configurations.Add(new VipLevelMap(Schema));
            modelBuilder.Configurations.Add(new PlayerMap(Schema));
            modelBuilder.Configurations.Add(new MassMessageMap(Schema));
            modelBuilder.Configurations.Add(new MassMessageContentMap(Schema));
        }

        public MessageTemplate FindMessageTemplateById(Guid id)
        {
            return MessageTemplates.FirstOrDefault(x => x.Id == id);
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