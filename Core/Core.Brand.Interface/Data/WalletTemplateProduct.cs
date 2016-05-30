using System;

namespace AFT.RegoV2.Core.Brand.Interface.Data
{
    public class WalletTemplateProduct
    {
        public Guid             Id { get; set; }
        public Guid  WalletTemplateId { get; set; }
        public WalletTemplate   WalletTemplate { get; set; }
        public Guid             ProductId { get; set; }
    }
}