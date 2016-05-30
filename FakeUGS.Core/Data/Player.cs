﻿using System;

namespace FakeUGS.Core.Data
{
    public class Player
    {
        public Guid     Id { get; set; }
        public Guid     VipLevelId { get; set; }
        public Guid     BrandId { get; set; }

        public string   Name { get; set; }
        public string   CultureCode { get; set; }
        public virtual GameCulture Culture {get;set;}
        public string   CurrencyCode { get; set; }
        public virtual GameCurrency   Currency { get; set; }
    }
}
