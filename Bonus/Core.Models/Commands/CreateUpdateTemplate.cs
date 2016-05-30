using System;

namespace AFT.RegoV2.Bonus.Core.Models.Commands
{
    public class CreateUpdateTemplate
    {
        public Guid Id { get; set; }
        public int Version { get; set; }

        public CreateUpdateTemplateInfo Info { get; set; }
        public CreateUpdateTemplateAvailability Availability { get; set; }
        public CreateUpdateTemplateRules Rules { get; set; }
        public CreateUpdateTemplateWagering Wagering { get; set; }
        public CreateUpdateTemplateNotification Notification { get; set; }
    }
}