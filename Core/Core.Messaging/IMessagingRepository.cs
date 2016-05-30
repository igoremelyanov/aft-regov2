using System;
using System.Data.Entity;
using AFT.RegoV2.Core.Messaging.Data;

namespace AFT.RegoV2.Core.Messaging
{
    public interface IMessagingRepository
    {
        IDbSet<MessageTemplate> MessageTemplates { get; }
        IDbSet<Data.Brand> Brands { get; }
        IDbSet<Language> Languages { get; }
        IDbSet<PaymentLevel> PaymentLevels { get; }
        IDbSet<VipLevel> VipLevels { get; }
        IDbSet<Player> Players { get; }
        IDbSet<MassMessage> MassMessages { get; }
        IDbSet<MassMessageContent> MassMessageContent { get; }
        MessageTemplate FindMessageTemplateById(Guid id);
        int SaveChanges();
    }
}