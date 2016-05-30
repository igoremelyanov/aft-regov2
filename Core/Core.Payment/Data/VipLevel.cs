using System;

namespace AFT.RegoV2.Core.Payment.Data
{
    public class VipLevel
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }
        public string Name { get; set; }
    }
}
