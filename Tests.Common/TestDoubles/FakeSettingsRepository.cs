using System.Data.Entity;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Settings.Interface.Data;
using AFT.RegoV2.Core.Settings.Interface.Interfaces;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeSettingsRepository : ISettingsRepository
    {
        private readonly FakeDbSet<SettingsItem> _settingsItems = new FakeDbSet<SettingsItem>();
        
        public IDbSet<SettingsItem> Settings { get { return _settingsItems; } }


        public int SaveChanges()
        {
            return 0;
        }

        public Task<int> SaveChangesAsync()
        {
            SaveChanges();

            return Task.FromResult(0);
        }
    }
}
