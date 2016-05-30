﻿using System;
﻿
namespace AFT.RegoV2.Core.Payment.Interface.Data
{
    public class BrandCurrency
    {
        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }
        
        public string CurrencyCode { get; set; }
        public Currency Currency { get; set; }
    }
}

