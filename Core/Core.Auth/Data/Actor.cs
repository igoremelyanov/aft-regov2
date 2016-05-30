using System;

namespace AFT.RegoV2.Core.Auth.Data
{
    public class Actor
    {
        public Actor()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
        public virtual Role Role { get; set; }
    }
}