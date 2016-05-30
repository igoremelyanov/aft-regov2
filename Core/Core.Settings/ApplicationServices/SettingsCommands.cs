using System;
using System.Collections.Generic;
using System.Linq;

using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Settings.Interface.Data;
using AFT.RegoV2.Core.Settings.Interface.Events;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared.Utils;

namespace AFT.RegoV2.Core.Settings.ApplicationServices
{
    public class SettingsCommands : ISettingsCommands
    {
        private ISettingsRepository _settingsRepository;
        private readonly IEventBus _eventBus;
        private readonly IActorInfoProvider _actorInfoProvider;

        public SettingsCommands(
            ISettingsRepository settingsRepository,
            IEventBus eventBus,
            IActorInfoProvider actorInfoProvider)
        {
            _settingsRepository = settingsRepository;
            _eventBus = eventBus;
            _actorInfoProvider = actorInfoProvider;
        }

        public void Save(string key, string value)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var settingsItem = UpdateOrCreateSettingsItem(key, value);

                _settingsRepository.SaveChanges();

                _eventBus.Publish(new SettingsItemChanged(settingsItem));

                scope.Complete();
            }
        }

        public void Save(IEnumerable<KeyValuePair<string, string>> items)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var settingsItems = new List<SettingsItem>();
                foreach (var item in items)
                {
                    var settingItem = UpdateOrCreateSettingsItem(item.Key, item.Value);
                    settingsItems.Add(settingItem);
                }

                _settingsRepository.SaveChanges();

                foreach (var settingsItem in settingsItems)
                {
                    _eventBus.Publish(new SettingsItemChanged(settingsItem));
                }

                scope.Complete();
            }
        }

        private SettingsItem UpdateOrCreateSettingsItem(string key, string value)
        {
            var settingsItem = _settingsRepository.Settings.SingleOrDefault(x => x.Key == key);
            if (settingsItem == null)
            {
                settingsItem = new SettingsItem()
                {
                    Id = Guid.NewGuid(),
                    Key = key
                };
                _settingsRepository.Settings.Add(settingsItem);
            }

            settingsItem.Value = value;
            settingsItem.UpdatedBy = _actorInfoProvider.Actor.UserName;
            settingsItem.UpdatedOn = DateTimeOffset.UtcNow;

            return settingsItem;
        }
    }
}
