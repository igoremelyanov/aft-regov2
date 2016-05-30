using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Security.Events
{
    public abstract class AdminStatusChanged : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }

        protected AdminStatusChanged() { }

        protected AdminStatusChanged(Guid id, string remarks)
        {
            Id = id;
            Remarks = remarks;
        }
    }

    public class AdminActivated : AdminStatusChanged
    {
        public AdminActivated() { } // default constructor is required for publishing event to MQ

        public AdminActivated(Guid id, string remarks) : base(id, remarks) { }
    }

    public class AdminDeactivated : AdminStatusChanged
    {
        public AdminDeactivated() { } // default constructor is required for publishing event to MQ

        public AdminDeactivated(Guid id, string remarks) : base(id, remarks) { }
    }
}
