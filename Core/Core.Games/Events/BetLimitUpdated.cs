﻿using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interface.Data;

namespace AFT.RegoV2.Core.Game.Events
{
    public class BetLimitUpdated : DomainEventBase
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public Guid BrandId { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public BetLimitUpdated()
        {

        }

        public BetLimitUpdated(GameProviderBetLimit betLimit)
        {
            Name = betLimit.Name;
            Code = betLimit.Code;
            Description = betLimit.Description;
            BrandId = betLimit.BrandId;
            CreatedBy = betLimit.CreatedBy;
            CreatedDate = betLimit.DateCreated;
        }
    }
}
