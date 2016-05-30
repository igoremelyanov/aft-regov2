using System;

namespace AFT.RegoV2.Core.Common.Data.Brand
{
    public class AssignBrandCultureRequest
    {
        public Guid Brand { get; set; }
        public string[] Cultures { get; set; }
        public string DefaultCulture { get; set; }
    }
}