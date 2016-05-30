using System;
using System.Linq;
using System.Security.Claims;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Auth.Interface.Data;

namespace AFT.RegoV2.Core.Common
{
    public class ActorInfoProvider : IActorInfoProvider
    {
        public ActorInfoProvider()
        {
            var principal = ClaimsPrincipal.Current;
            var usernameClaim = principal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Name);
            var idClaim = principal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            Actor = new ActorInfo
            {
                UserName = usernameClaim != null ? usernameClaim.Value : string.Empty,
                Id = idClaim != null ? new Guid(idClaim.Value) : Guid.Empty
            };
        }

        public ActorInfo Actor { get; }

        public bool IsActorAvailable => !string.IsNullOrWhiteSpace(Actor.UserName);
    }
}