using System;

using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Settings.Interface.Data;

namespace AFT.RegoV2.Core.Settings.Interface.Events
{
    public class SettingsItemChanged : DomainEventBase
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }

        public SettingsItemChanged()
        {
        }

        public SettingsItemChanged(SettingsItem settingsItem)
        {
            Key = settingsItem.Key;
            Value = settingsItem.Value;
            UpdatedBy = settingsItem.UpdatedBy;
            UpdatedOn = settingsItem.UpdatedOn;
        }
    }
}
