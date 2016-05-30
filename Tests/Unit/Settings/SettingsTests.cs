using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;

using Microsoft.Practices.Unity;
using NUnit.Framework;


namespace AFT.RegoV2.Tests.Unit.Settings
{
    public class SettingsTests : AdminWebsiteUnitTestsBase
    {
        private ISettingsCommands _settingsCommands { get; set; }
        private ISettingsQueries _settingsQueries { get; set; }
        
        public override void BeforeEach()
        {
            base.BeforeEach();

            _settingsCommands = Container.Resolve<ISettingsCommands>();
            _settingsQueries = Container.Resolve<ISettingsQueries>();
        }

        [Test]
        public void SettingsItems_saves_and_restores()
        {
            string key = TestDataGenerator.GetRandomString();
            string value = TestDataGenerator.GetRandomString();

            _settingsCommands.Save(key, value);
            var restoredValue = _settingsQueries.Get(key);

            Assert.AreEqual(value, restoredValue);
        }
        
        [Test]
        public void SettingsItems_saves_and_restores_array()
        {
            var settings = new Dictionary<string, string>
            {
                { TestDataGenerator.GetRandomString(), TestDataGenerator.GetRandomString() },
                { TestDataGenerator.GetRandomString(), TestDataGenerator.GetRandomString() },
                { TestDataGenerator.GetRandomString(), TestDataGenerator.GetRandomString() },
            };
            
            _settingsCommands.Save(settings);
            var restoredValues = _settingsQueries.Get(settings.Keys);

            CollectionAssert.AreEquivalent(settings.Keys, restoredValues.Keys);
            CollectionAssert.AreEquivalent(settings.Values, restoredValues.Values);
            CollectionAssert.AreEqual(settings.ToList().OrderBy(x => x.Key), restoredValues.ToList().OrderBy(x => x.Key));
        }

        [Test]
        public void SettingsItems_changes_and_restores()
        {
            string key = TestDataGenerator.GetRandomString();
            string value1 = TestDataGenerator.GetRandomString();
            string value2 = TestDataGenerator.GetRandomString();

            _settingsCommands.Save(key, value1);
            _settingsCommands.Save(key, value2);
            var restoredValue = _settingsQueries.Get(key);

            Assert.AreEqual(value2, restoredValue);
        }

        [Test]
        public void SettingsItems_changes_and_restores_array()
        {
            var settings = new Dictionary<string, string>
            {
                { TestDataGenerator.GetRandomString(), TestDataGenerator.GetRandomString() },
                { TestDataGenerator.GetRandomString(), TestDataGenerator.GetRandomString() },
                { TestDataGenerator.GetRandomString(), TestDataGenerator.GetRandomString() },
            };

            _settingsCommands.Save(settings);

            var keys = settings.Keys.ToList();
            foreach (var key in keys)
            {
                settings[key] = TestDataGenerator.GetRandomString();
            }

            _settingsCommands.Save(settings);

            var restoredValues = _settingsQueries.Get(settings.Keys);

            CollectionAssert.AreEquivalent(settings.Keys, restoredValues.Keys);
            CollectionAssert.AreEquivalent(settings.Values, restoredValues.Values);
            CollectionAssert.AreEqual(settings.ToList().OrderBy(x => x.Key), restoredValues.ToList().OrderBy(x => x.Key));
        }

        [Test]
        public void SettingsItems_saves_and_restores_with_multiple_settings()
        {
            string key1 = TestDataGenerator.GetRandomString();
            string value1 = TestDataGenerator.GetRandomString();
            string key2 = TestDataGenerator.GetRandomString();
            string value2 = TestDataGenerator.GetRandomString();
            string key3 = TestDataGenerator.GetRandomString();
            string value3 = TestDataGenerator.GetRandomString();

            _settingsCommands.Save(key1, value1);
            _settingsCommands.Save(key2, value2);
            _settingsCommands.Save(key3, value3);
            var restoredValue1 = _settingsQueries.Get(key1);
            var restoredValue2 = _settingsQueries.Get(key2);
            var restoredValue3 = _settingsQueries.Get(key3);

            Assert.AreEqual(value1, restoredValue1);
            Assert.AreEqual(value2, restoredValue2);
            Assert.AreEqual(value3, restoredValue3);
        }

        [Test]
        public async Task Async_and_non_async_get_are_equivalent()
        {
            string key = TestDataGenerator.GetRandomString();
            string value = TestDataGenerator.GetRandomString();

            _settingsCommands.Save(key, value);
            var restoredValue = _settingsQueries.Get(key);
            var restoredValueAsync = await _settingsQueries.GetAsync(key);

            Assert.AreEqual(value, restoredValue);
            Assert.AreEqual(value, restoredValueAsync);
        }

        [Test]
        public async Task Async_and_non_async_get_array_are_equivalent()
        {
            var settings = new Dictionary<string, string>
            {
                { TestDataGenerator.GetRandomString(), TestDataGenerator.GetRandomString() },
                { TestDataGenerator.GetRandomString(), TestDataGenerator.GetRandomString() },
                { TestDataGenerator.GetRandomString(), TestDataGenerator.GetRandomString() },
            };

            _settingsCommands.Save(settings);

            var keys = settings.Keys.ToList();
            foreach (var key in keys)
            {
                settings[key] = TestDataGenerator.GetRandomString();
            }

            _settingsCommands.Save(settings);

            var restoredValues = _settingsQueries.Get(settings.Keys);
            var restoredValuesAsync = await _settingsQueries.GetAsync(settings.Keys);

            var expectedSettings = settings.OrderBy(x => x.Key).ToList();
            CollectionAssert.AreEqual(expectedSettings, restoredValues.ToList().OrderBy(x => x.Key));
            CollectionAssert.AreEqual(expectedSettings, restoredValuesAsync.ToList().OrderBy(x => x.Key));
        }
    }
}
