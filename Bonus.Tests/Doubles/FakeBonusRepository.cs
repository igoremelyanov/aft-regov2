using System;
using System.Data.Entity;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Infrastructure.DataAccess;
using AFT.RegoV2.Tests.Common.TestDoubles;

namespace AFT.RegoV2.Bonus.Tests.Doubles
{
    public class FakeBonusRepository : BonusRepository
    {
        private readonly FakeDbSet<Core.Data.Bonus> _bonuses = new FakeDbSet<Core.Data.Bonus>();
        private readonly FakeDbSet<Template> _templates = new FakeDbSet<Template>();
        private readonly FakeDbSet<Player> _players = new FakeDbSet<Player>();
        private readonly FakeDbSet<Brand> _brands = new FakeDbSet<Brand>();
        private readonly FakeDbSet<Game> _games = new FakeDbSet<Game>();

        public override IDbSet<Player> Players => _players;
        public override IDbSet<Core.Data.Bonus> Bonuses => _bonuses;
        public override IDbSet<Template> Templates => _templates;
        public override IDbSet<Brand> Brands => _brands;
        public override IDbSet<Game> Games => _games;

        protected override void LockBonus(Guid bonusId) { }
        protected override void LockPlayer(Guid playerId) { }

        public override int SaveChanges()
        {
            return 0;
        }
    }
}