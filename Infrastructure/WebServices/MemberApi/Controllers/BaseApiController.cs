using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using AFT.RegoV2.Infrastructure.Attributes;

namespace AFT.RegoV2.MemberApi.Controllers
{
    [ForceJsonFormatter]
    [Authorize]
    public class BaseApiController : ApiController
    {
        protected Guid PlayerId
        {
            get
            {
                var principal = (ClaimsPrincipal) User;
                var playerId = (from c in principal.Claims where c.Type == ClaimTypes.NameIdentifier select c.Value).Single();
                return new Guid(playerId);
            }
        }

        protected string Username
        {
            get
            {
                var principal = (ClaimsPrincipal) User;
                var username = (from c in principal.Claims where c.Type == ClaimTypes.Name select c.Value).Single();
                return username;
            }
        }
    }
}