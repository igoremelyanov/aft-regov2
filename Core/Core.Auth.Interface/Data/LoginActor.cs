using System;

namespace AFT.RegoV2.Core.Auth.Interface.Data
{
    public class LoginActor
    {
        public Guid ActorId { get; set; }
        public string Password { get; set; }
    }
}