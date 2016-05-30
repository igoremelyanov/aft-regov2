using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Security.Events
{
    public class AdminPasswordChanged :DomainEventBase
    {
        public Guid Id { get; set; }

        public AdminPasswordChanged()
        {
            
        }

        public AdminPasswordChanged(Guid userId)
        {
            Id = userId;
        }
    }
}
