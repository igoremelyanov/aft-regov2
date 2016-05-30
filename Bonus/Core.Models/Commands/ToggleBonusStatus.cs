using System;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    public class ToggleBonusStatus
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
        public string Remarks { get; set; }
    }
}