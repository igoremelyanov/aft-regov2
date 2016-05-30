using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events
{
    public class BrandActivated : DomainEventBase
    {
        public Guid Id { get; set; }
    }
}