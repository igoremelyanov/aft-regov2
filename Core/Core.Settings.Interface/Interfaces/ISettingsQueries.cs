using System.Collections.Generic;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Settings.Interface.Interfaces
{
    public interface ISettingsQueries
    {
        string Get(string key);
        Task<string> GetAsync(string key);

        Dictionary<string, string> Get(IEnumerable<string> keys);
        Task<Dictionary<string, string>> GetAsync(IEnumerable<string> keys);
    }
}
