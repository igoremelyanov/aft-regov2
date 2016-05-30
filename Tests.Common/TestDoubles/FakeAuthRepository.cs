using System.Data.Entity;
using AFT.RegoV2.Core.Auth.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Auth;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeAuthRepository : AuthRepository
    {
        private readonly FakeDbSet<Actor> _actors = new FakeDbSet<Actor>();
        private readonly FakeDbSet<Role> _roles = new FakeDbSet<Role>();
        private readonly FakeDbSet<Permission> _permissions = new FakeDbSet<Permission>();

        public override IDbSet<Actor> Actors { get { return _actors; } }
        public override IDbSet<Role> Roles { get { return _roles; } }
        public override IDbSet<Permission> Permissions { get { return _permissions; } }

        public override int SaveChanges()
        {
            return 0;
        }
    }
}