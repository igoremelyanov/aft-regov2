using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Brand
{
    public class VipLevelStatusChanged : DomainEventBase
    {
        public Guid VipLevelId { get; set; }
        public string Remarks { get; set; }

        protected VipLevelStatusChanged() { }

        protected VipLevelStatusChanged(Guid vipLevelId, string remarks)
        {
            VipLevelId = vipLevelId;
            Remarks = remarks;
        }
    }

    public class VipLevelActivated : VipLevelStatusChanged
    {
        public VipLevelActivated() { }

        public VipLevelActivated(Guid vipLevelId, string remarks) : base(vipLevelId, remarks) { }
    }

    public class VipLevelDeactivated : VipLevelStatusChanged
    {
        public VipLevelDeactivated() { }

        public VipLevelDeactivated(Guid vipLevelId, string remarks) : base(vipLevelId, remarks) { }
    }
}