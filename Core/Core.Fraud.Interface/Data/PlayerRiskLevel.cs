﻿using System;

namespace AFT.RegoV2.Core.Fraud.Interface.Data
{
    public class PlayerRiskLevel
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }

        public Guid RiskLevelId { get; set; }
        public virtual RiskLevel RiskLevel { get; set; }

        public PlayerRiskLevelStatus Status { get; set; }

        public string Description { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }

    }
}
