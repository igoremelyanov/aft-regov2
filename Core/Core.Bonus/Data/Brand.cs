using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Bonus.Core.Data
{
    public class Brand : Identity
    {
        public Brand()
        {
            Currencies = new List<Currency>();
            Vips = new List<VipLevel>();
            RiskLevels = new List<RiskLevel>();
            WalletTemplates = new List<WalletTemplate>();
        }

        public string TimezoneId { get; set; }

        public virtual List<Currency> Currencies { get; set; }
        public virtual List<VipLevel> Vips { get; set; }
        public virtual List<RiskLevel> RiskLevels { get; set; }
        public virtual List<WalletTemplate> WalletTemplates { get; set; }
    }

    public class WalletTemplate : Identity
    {
        public bool IsMain { get; set; }
        public virtual List<Product> Products { get; set; }
    }

    public class Product : Identity
    {
        public Guid ProductId { get; set; }
        public Guid WalletTemplateId { get; set; }
        public virtual WalletTemplate WalletTemplate { get; set; }
    }

    public class Game : Identity
    {
        public Guid ProductId { get; set; }
    }

    public class Currency: Identity
    {
        public string Code { get; set; }
        public Guid BrandId { get; set; }
        public virtual Brand Brand { get; set; }
    }
    public class VipLevel: Identity
    {
        public string Code { get; set; }
    }

    public class RiskLevel : Identity
    {
        public bool IsActive { get; set; }
    };
}
