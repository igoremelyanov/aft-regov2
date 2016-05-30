using System;

namespace AFT.RegoV2.Core.Auth.Interface.Data
{
    public class CreateActor
    {
        public Guid ActorId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
