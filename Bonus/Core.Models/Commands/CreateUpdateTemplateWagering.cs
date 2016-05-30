using System;
using System.Collections.Generic;
using AFT.RegoV2.Bonus.Core.Models.Enums;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    public class CreateUpdateTemplateWagering
    {
        public CreateUpdateTemplateWagering()
        {
            GameContributions = new List<CreateUpdateGameContribution>();
        }

        public bool HasWagering { get; set; }
        public WageringMethod Method { get; set; }
        public decimal Multiplier { get; set; }
        public decimal Threshold { get; set; }
        public List<CreateUpdateGameContribution> GameContributions { get; set; }
        public bool IsAfterWager { get; set; }
    }

    public class CreateUpdateGameContribution
    {
        public Guid GameId { get; set; }
        public decimal Contribution { get; set; }
    }
}