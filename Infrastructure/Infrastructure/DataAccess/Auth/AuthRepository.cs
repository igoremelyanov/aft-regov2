using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth;
using AFT.RegoV2.Core.Auth.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Auth.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Auth
{
    public class AuthRepository : DbContext, IAuthRepository
    {
        private const string Schema = "auth";

        static AuthRepository()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AuthRepository, Configuration>());
        }

        public AuthRepository() : base("name=Default") { }

        public virtual IDbSet<Actor> Actors { get; set; }
        public virtual IDbSet<Role> Roles { get; set; }
        public virtual IDbSet<Permission> Permissions { get; set; }

        public Core.Auth.Entities.Actor GetActor(Guid actorId)
        {
            var actorData = Actors.Single(a => a.Id == actorId);
            return new Core.Auth.Entities.Actor(actorData);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Actor>().ToTable("Actors", Schema);
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .Map(t => t.ToTable("RolePermissions", Schema));
            modelBuilder.Entity<Role>().ToTable("Roles", Schema);
            modelBuilder.Entity<Permission>().ToTable("Permissions", Schema);
        }
    }
}
