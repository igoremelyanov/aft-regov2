using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using AFT.RegoV2.Core.Settings.Interface.Data;
using AFT.RegoV2.Core.Settings.Interface.Exceptions;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;

namespace AFT.RegoV2.Core.Settings.ApplicationServices
{
    public class SettingsQueries : ISettingsQueries
    {
        private ISettingsRepository _settingsRepository;

        public SettingsQueries(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public string Get(string key)
        {
            var settingsItem = _settingsRepository.Settings.SingleOrDefault(x => x.Key == key);
            return PrepareGetResult(settingsItem, key);
        }

        public async Task<string> GetAsync(string key)
        {
            var settingsItem = await _settingsRepository.Settings.SingleOrDefaultAsync(x => x.Key == key);
            return PrepareGetResult(settingsItem, key);
        }

        public Dictionary<string, string> Get(IEnumerable<string> keys)
        {
            var settingsItems = _settingsRepository.Settings.Where(x => keys.Contains(x.Key)).ToList();
            return PrepareGetResult(keys, settingsItems);
        }

        public async Task<Dictionary<string, string>> GetAsync(IEnumerable<string> keys)
        {
            var settingsItems = await _settingsRepository.Settings.Where(x => keys.Contains(x.Key)).ToListAsync();
            return PrepareGetResult(keys, settingsItems);
        }
        
        protected string PrepareGetResult(SettingsItem settingsItem, string key)
        {
            if (settingsItem == null)
            {
                throw new MissingKeyException(key);
            }

            return settingsItem.Value;
        }

        protected Dictionary<string, string> PrepareGetResult(IEnumerable<string> keys, List<SettingsItem> settingsItems)
        {
            var result = new Dictionary<string, string>();

            foreach (var key in keys)
            {
                var settingsItem = settingsItems.FirstOrDefault(x => x.Key == key);
                var value = PrepareGetResult(settingsItem, key);

                result.Add(key, value);
            }

            return result;
        }
    }
}
