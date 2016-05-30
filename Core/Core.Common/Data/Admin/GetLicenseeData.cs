using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Common.Data.Admin
{
    public class GetLicenseeData
    {
        public List<Guid> Licensees  { get; set; }
        public bool UseBrandFilter { get; set; }

        public GetLicenseeData()
        {
            Licensees = new List<Guid>();
        }
    }
}