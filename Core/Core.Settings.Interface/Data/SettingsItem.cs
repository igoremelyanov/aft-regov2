using System;
using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Settings.Interface.Data
{
    public class SettingsItem
    {
        public Guid Id { get; set; }

        [MaxLength(450)]
        public string Key { get; set; }

        public string Value { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
    }
}
