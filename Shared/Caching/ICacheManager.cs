using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Shared.Caching
{
    public interface ICacheManager
    {
        Task<T> GetOrCreateObjectAsync<T>(object cacheKey, TimeSpan? expiresIn, Func<Task<T>> getFunc);
        Task<T> GetOrCreateObjectAsync<T>(CacheType cacheType, object cacheKey, TimeSpan? expiresIn, Func<Task<T>> getFunc);
        T GetOrCreateObject<T>(object cacheKey, TimeSpan? expiresIn, Func<T> getFunc);
        T GetOrCreateObject<T>(CacheType cacheType, object cacheKey, TimeSpan? expiresIn, Func<T> getFunc);
        Task ClearAsync(CacheType key = CacheType.Any);
        Task<CacheStats> GetCacheStatsAsync();
    }
}
