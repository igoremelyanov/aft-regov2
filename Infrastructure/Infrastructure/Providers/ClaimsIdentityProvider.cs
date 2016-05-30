using System;
using System.Security.Claims;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;

namespace AFT.RegoV2.Infrastructure.Providers
{
    public class ClaimsIdentityProvider
    {
        private readonly IAuthQueries _authQueries;

        public ClaimsIdentityProvider(IAuthQueries authQueries)
        {
            _authQueries = authQueries;
        }

        public ClaimsIdentity GetActorIdentity(Guid actorId, string authentificationType)
        {
            var actorData = _authQueries.GetActor(actorId);

            var claimsIdentity = new ClaimsIdentity(authentificationType);
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, actorId.ToString()));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, actorData.UserName));

            return claimsIdentity;
        }
    }
}