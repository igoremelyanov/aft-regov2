using System.Data.Entity;
using System.Threading.Tasks;

using AFT.RegoV2.Core.Settings.Interface.Data;

namespace AFT.RegoV2.Core.Settings.Interface.Interfaces
{
    public interface ISettingsRepository
    {
        IDbSet<SettingsItem> Settings { get; }

        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
