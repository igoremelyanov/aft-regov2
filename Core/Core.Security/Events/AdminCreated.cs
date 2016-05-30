using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data.Users;

namespace AFT.RegoV2.Core.Security.Events
{
    public class AdminCreated : DomainEventBase
    {
        public AdminCreated() { } // default constructor is required for publishing event to MQ

        public AdminCreated(Admin admin)
        {
            Id = admin.Id;
            Username = admin.Username;
            FirstName = admin.FirstName;
            LastName = admin.LastName;
            Language = admin.Language;
            IsActive = admin.IsActive;
            Description = admin.Description;
            Licensees = admin.Licensees.Select(l => l.Id);
            RoleId = admin.Role.Id;
            RoleName = admin.Role.Name;
        }

        public Guid Id { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Language { get; set; }

        public bool IsActive { get; set; }

        public string Description { get; set; }

        public IEnumerable<Guid> Licensees { get; set; }

        public Guid RoleId { get; set; }

        public string RoleName { get; set; }

    }
}
