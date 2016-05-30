using System;

namespace AFT.RegoV2.Core.Brand.Interface.Data
{
    public class BrandProduct
    {
        public Guid BrandId { get; set; }
        public Guid ProductId { get; set; }

        public Brand Brand { get; set; }
    }
}