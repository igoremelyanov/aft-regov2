﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFT.RegoV2.Core.Brand.Interface.Data
{
    public class BrandCurrency
    {
        [Key, ForeignKey("Brand"), Column(Order = 0)]
        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }

        [Key, ForeignKey("Currency"), Column(Order = 1)]
        public string CurrencyCode { get; set; }
        public Currency Currency { get; set; }

        public DateTimeOffset DateAdded { get; set; }
        public string AddedBy { get; set; }
        public Guid? DefaultPaymentLevelId { get; set; }
    }
}