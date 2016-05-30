using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakePlayerRepository : IPlayerRepository
    {
        private readonly FakeDbSet<Player> _players = new FakeDbSet<Player>();
        private readonly FakeDbSet<VipLevel> _vipLevels = new FakeDbSet<VipLevel>();
        private readonly FakeDbSet<SecurityQuestion> _securityQuestions = new FakeDbSet<SecurityQuestion>();
        private readonly FakeDbSet<PlayerBetStatistics> _playerBetStatistics = new FakeDbSet<PlayerBetStatistics>();
        private readonly FakeDbSet<PlayerActivityLog> _playerActivityLog = new FakeDbSet<PlayerActivityLog>();
        private readonly FakeDbSet<PlayerInfoLog> _playerInfoLog = new FakeDbSet<PlayerInfoLog>();
        private readonly FakeDbSet<Brand> _brands = new FakeDbSet<Brand>();
        private readonly FakeDbSet<IdentityVerification> _identityVerifications = new FakeDbSet<IdentityVerification>();
        private readonly FakeDbSet<BankAccount> _bankAccounts = new FakeDbSet<BankAccount>();
        private readonly FakeDbSet<Bank> _banks = new FakeDbSet<Bank>();
        private readonly FakeDbSet<OnSiteMessage> _onSiteMessages = new FakeDbSet<OnSiteMessage>();

        private readonly IDbSet<IdentificationDocumentSettings> _identificationDocumentSettingses = new FakeDbSet<IdentificationDocumentSettings>();

        public EventHandler SavedChanges;

        public IDbSet<Player> Players
        {
            get { return _players; }
        }

        public IDbSet<IdentityVerification> IdentityVerifications
        {
            get { return _identityVerifications; }
        }

        public IDbSet<PlayerBetStatistics> PlayerBetStatistics
        {
            get { return _playerBetStatistics; }
        }

        public IDbSet<VipLevel> VipLevels
        {
            get { return _vipLevels; }
        }        
        
        public IDbSet<BankAccount> BankAccounts
        {
            get { return _bankAccounts; }
        }        
        
        public IDbSet<Bank> Banks
        {
            get { return _banks; }
        }

        public IDbSet<SecurityQuestion> SecurityQuestions
        {
            get { return _securityQuestions; }
        }

        public IDbSet<PlayerActivityLog> PlayerActivityLog
        {
            get { return _playerActivityLog; }
        }

        public IDbSet<PlayerInfoLog> PlayerInfoLog
        {
            get { return _playerInfoLog; }
        }

        public IDbSet<Brand> Brands
        {
            get { return _brands; }
        }

        public IDbSet<IdentificationDocumentSettings> IdentificationDocumentSettings
        {
            get { return _identificationDocumentSettingses; }
        }

        public IDbSet<OnSiteMessage> OnSiteMessages
        {
            get { return _onSiteMessages; }
        }

        public int SaveChanges()
        {
            //calling ToArray to prevent concurrency issues
            foreach (var brand in _brands.ToArray())
            {
                if (brand.DefaultVipLevelId.HasValue)
                    brand.DefaultVipLevel = _vipLevels.Where(x => x.Id == brand.DefaultVipLevelId).Single();
            }
            foreach (var player in _players.ToArray())
            {
                if (player.VipLevelId.HasValue)
                    player.VipLevel = _vipLevels.Where(x => x.Id == player.VipLevelId).Single();
            }

            var handler = SavedChanges;
            if (handler != null)
                handler(this, EventArgs.Empty);

            return 0;
        }
    }
}
