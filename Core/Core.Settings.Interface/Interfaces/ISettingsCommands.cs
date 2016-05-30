using System.Collections.Generic;

namespace AFT.RegoV2.Core.Settings.Interface.Interfaces
{
    public interface ISettingsCommands
    {
        void Save(string key, string value);
        void Save(IEnumerable<KeyValuePair<string, string>> keyValues);
    }
}
