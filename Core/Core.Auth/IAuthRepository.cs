using System;
using System.Data.Entity;
using AFT.RegoV2.Core.Auth.Data;

namespace AFT.RegoV2.Core.Auth
{
    public interface IAuthRepository
    {
        IDbSet<Actor> Actors { get; }
        IDbSet<Role> Roles { get; }
        IDbSet<Permission> Permissions { get; }

        Entities.Actor GetActor(Guid actorId);
        int SaveChanges();
    }
}