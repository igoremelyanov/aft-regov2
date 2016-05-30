using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Common.Data.Admin
{
    public class EditIpRegulationDataBase
    {
        public Guid Id { get; set; }

        public string IpAddress { get; set; }

        public string IpAddressBatch { get; set; }

        public string Description { get; set; }
    }

    public class EditAdminIpRegulationData : EditIpRegulationDataBase
    {
    }

    public class DeleteAdminIpRegulationData
    {
        public Guid Id { get; set; }
    }

    public class BrandIpRegulationDataBase : EditIpRegulationDataBase
    {
        public Guid BrandId { get; set; }

        public Guid LicenseeId { get; set; }

        public string RedirectionUrl { get; set; }

        public string BlockingType { get; set; }
    }

    public class AddBrandIpRegulationData : BrandIpRegulationDataBase
    {
        public IList<Guid> AssignedBrands { get; set; }
    }

    public class EditBrandIpRegulationData : BrandIpRegulationDataBase
    {
    }

    public class DeleteBrandIpRegulationData
    {
        public Guid Id { get; set; }
    }
}