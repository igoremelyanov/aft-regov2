using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Messaging;
using AFT.RegoV2.Core.Messaging.Data;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{

    public class FakeMessagingRepository : IMessagingRepository
    {
        private readonly FakeDbSet<MessageTemplate> _messageTemplatesDbSetFake = new FakeDbSet<MessageTemplate>();
        private readonly FakeDbSet<Brand> _brandDbSetFake = new FakeDbSet<Brand>();
        private readonly FakeDbSet<Language> _languageDbSetFake = new FakeDbSet<Language>();
        private readonly FakeDbSet<PaymentLevel> _paymentLevelDbSetFake = new FakeDbSet<PaymentLevel>();
        private readonly FakeDbSet<VipLevel> _vipLevelDbSetFake = new FakeDbSet<VipLevel>();
        private readonly FakeDbSet<Player> _playerDbSetFake = new FakeDbSet<Player>();
        private readonly FakeDbSet<MassMessage> _massMessageDbSetFake = new FakeDbSet<MassMessage>();
        private readonly FakeDbSet<MassMessageContent> _massMessageContentDbSetFake = new FakeDbSet<MassMessageContent>();

        public IDbSet<MessageTemplate> MessageTemplates { get { return _messageTemplatesDbSetFake; } }
        public IDbSet<Brand> Brands { get { return _brandDbSetFake; } }
        public IDbSet<Language> Languages { get { return _languageDbSetFake; } }
        public IDbSet<PaymentLevel> PaymentLevels { get { return _paymentLevelDbSetFake; } }
        public IDbSet<VipLevel> VipLevels { get { return _vipLevelDbSetFake; } }
        public IDbSet<Player> Players { get { return _playerDbSetFake; } }
        public IDbSet<MassMessage> MassMessages { get { return _massMessageDbSetFake; } }
        public IDbSet<MassMessageContent> MassMessageContent { get { return _massMessageContentDbSetFake; } } 

        public FakeMessagingRepository()
        {
        }

        public MessageTemplate FindMessageTemplateById(Guid id)
        {
            return MessageTemplates.FirstOrDefault(x => x.Id == id);
        }

        public int SaveChanges()
        {
            return 0;
        }
    }
}