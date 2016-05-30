using System;
using AFT.RegoV2.Core.Security.Data.Users;

namespace AFT.RegoV2.Core.Security.Data.Base
{
    public class IpRegulationBase
    {
        public Guid Id { get; set; }

        public string IpAddress { get; set; }

        public string Description { get; set; }

        public Admin CreatedBy { get; set; }

        public DateTimeOffset? CreatedDate { get; set; }

        public Admin UpdatedBy { get; set; }

        public DateTimeOffset? UpdatedDate { get; set; }
    }
}

